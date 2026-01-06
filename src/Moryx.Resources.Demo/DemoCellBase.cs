// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Drivers.Message;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Cells;
using Moryx.Demo.Properties;
using Moryx.Factory;
using Moryx.Notifications;
using Moryx.ProcessData;
using Moryx.Resources.Demo.Messages;
using Moryx.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
[ResourceAvailableAs(typeof(IProcessDataPublisher))]
public abstract class DemoCellBase : Cell, INotificationSender, IProcessDataPublisher
{
    private readonly Random _randomness = new();

    #region Properties
    public string Identifier => Id.ToString(CultureInfo.InvariantCulture);

    private string _cellState;
    [DataMember, EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.CELL_STATE))]
    public string CellState
    {
        get => _cellState;
        set
        {
            if (_cellState != value)
            {
                OnProcessDataOccured();
            }

            _cellState = value;
        }
    }

    [DataMember, EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.NOMINAL_POWER))]
    public virtual int NominalPower { get; protected set; }

    [DataMember]
    protected bool _disabled;

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.DISABLED))]
    [EntryVisualization("", "power_off")]
    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            UpdateCapabilities();
            if (_disabled)
            {
                CellState = "Disabled";
            }
            else
            {
                CellState = "Idle";
            }
        }
    }

    protected abstract void UpdateCapabilities();

    private Session _currentSession;
    public Session CurrentSession
    {
        get => _currentSession; protected set
        {
            _currentSession = value;
            CellState = _currentSession switch
            {
                ControlSystem.Cells.ActivityStart => "Running",
                ControlSystem.Cells.ReadyToWork => "Running",
                ControlSystem.Cells.ActivityCompleted => "Idle",
                ControlSystem.Cells.SequenceCompleted => "Idle",
                _ => "Idle"
            };
        }
    }
    #endregion

    #region References
    /// <summary>
    /// Notification adapter
    /// </summary>
    public INotificationAdapter NotificationAdapter { get; set; }

    [ResourceReference(ResourceRelationType.Driver, IsRequired = true)]
    public IMessageDriver Driver { get; set; }
    #endregion

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_cellState))
        {
            _cellState = "Idle";
        }

        await base.OnInitializeAsync(cancellationToken);
    }

    #region Session
    protected override IEnumerable<Session> ProcessEngineAttached()
    {
        yield break;
    }

    protected override IEnumerable<Session> ProcessEngineDetached()
    {
        yield break;
    }

    public override void StartActivity(ActivityStart activityStart)
    {

    }

    public override void ProcessAborting(Activity affectedActivity)
    {
        if (CurrentSession is not ActivityStart activityStart)
        {
            PublishActivityCompleted(Session.WrapUnknownActivity((Activity)affectedActivity));
            return;
        }

        // Report current activity as failed
        affectedActivity.Fail();
        CurrentSession = activityStart.CreateResult();
        PublishActivityCompleted(CurrentSession as ActivityCompleted);
    }

    protected void PublishResult(int result)
    {
        var activityStart = (ActivityStart)CurrentSession;
        CurrentSession = activityStart.CreateResult(result);
        PublishActivityCompleted((ActivityCompleted)CurrentSession);
    }

    public override void SequenceCompleted(SequenceCompleted completed)
    {
        CurrentSession = null;
        Driver?.Send(new ReleaseWorkpieceMessage());
    }
    #endregion

    #region Notifications
    [EntrySerialize]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.METHOD_PUBLISH_NOTIFICATION))]
    public void PublishNotification(string title, string message, Severity severity)
    {
        NotificationAdapter.Publish(this, new Notification()
        {
            Title = title,
            Message = message,
            Severity = severity,
            IsAcknowledgable = true
        });
    }

    public void Acknowledge(Notification notification, object tag)
    {
        NotificationAdapter.Acknowledge(this, notification);
    }
    #endregion

    #region Process Data
    protected Measurement GetCellMeasurement()
    {
        var fraction = NominalPower * FactorFromCellState();
        var randomness = _randomness.NextDouble() / 5 + 0.9;
        var currentPower = fraction * randomness;

        var measurement = new Measurement("cell_report", DateTime.Now);
        measurement.Fields.Add(new DataField("state", CellState));
        measurement.Fields.Add(new DataField("current_power", currentPower));
        measurement.Tags.Add(new DataTag("source", Name));

        var session = CurrentSession;
        if (session != null && session.Process != null)
        {
            measurement.Fields.Add(new DataField("process_id", (int)session.Process.Id));
        }

        return measurement;
    }

    private double FactorFromCellState()
    {
        if (CellState == "Disabled")
        {
            return 0;
        }

        if (CellState == "Maintenance")
        {
            return 0;
        }

        if (CellState == "Idle")
        {
            return 0.5;
        }

        if (CellState == "Running")
        {
            return 1;
        }
        else
        {
            return 1;
        }
    }

    protected virtual void OnProcessDataOccured()
    {
        ProcessDataOccurred?.Invoke(this, GetCellMeasurement());
    }

    public event EventHandler<Measurement> ProcessDataOccurred;
    #endregion
}
