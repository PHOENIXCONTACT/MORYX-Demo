// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Activities;
using Moryx.ControlSystem.Cells;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Properties;
using Moryx.Factory;
using Moryx.Notifications;
using Moryx.Resources.Demo.Messages;
using Moryx.Serialization;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
public class TestSimulationCell : DemoCellBase, INotificationSender
{
    private readonly Random _randomness = new();
    private int _voltage;

    [DataMember, EntrySerialize]
    [DefaultValue(90), Display(ResourceType = typeof(Strings), Name = nameof(Strings.SUCCESS_RATE)), Description("Durchschnittliche Erfolgsquote in Prozent")]
    [EntryVisualization("%", "iso")]
    public int SuccessRate { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.NOMINAL_POWER))]
    [EntryVisualization("W", "electrical_services")]
    public override int NominalPower { get => 220; }

    [DataMember, EntrySerialize]
    [DefaultValue(12), Display(ResourceType = typeof(Strings), Name = nameof(Strings.VOLTAGE)), Description("Testspannung der Zelle")]
    [EntryVisualization("V", "electric_bolt")]
    public int Voltage
    {
        get => _voltage;
        set
        {
            _voltage = value;
        }
    }

    protected override void UpdateCapabilities()
    {
        Capabilities = new TestCapabilities { Voltage = Voltage };
    }

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);
        UpdateCapabilities();

        Driver.Received += OnMessageReceived;
    }

    protected override void OnDispose()
    {
        Driver.Received -= OnMessageReceived;

        base.OnDispose();
    }

    #region Session
    private void OnMessageReceived(object sender, object message)
    {
        switch (message)
        {
            case WorkpieceArrivedMessage arrived:
                var rtw = Session.StartSession(ActivityClassification.Production, ReadyToWorkType.Pull, arrived.ProcessId);
                PublishReadyToWork(rtw);
                CellState = "Running";
                break;
            case TestCompletedMessage:
                if (CurrentSession is ActivityStart _activityStart && _activityStart != null)
                {
                    CompleteActivity();
                    CellState = "Idle";
                }
                break;
        }
    }

    public override void StartActivity(ActivityStart activityStart)
    {
        CurrentSession = activityStart;
        Driver.Send(new TestProductMessage { ActivityId = activityStart.Activity.Id });
    }

    private void CompleteActivity()
    {
        var activityStart = (ActivityStart)CurrentSession;
        if (activityStart is null)
        {
            return;
        }

        var randResult = _randomness.Next(100);
        var result = randResult < SuccessRate ? TestResult.Success : TestResult.Failed;
        double resistance;
        if (CurrentSession is ActivityStart ac)
        {
            if (result == TestResult.Failed)
            {
                var faultyContact = _randomness.Next(0b111111);
                var contact1 = faultyContact & 0b000111;
                var contact2 = (faultyContact & 0b111000) >> 3;

                var contacts = new List<int>();
                if (contact1 > 0 & contact1 <= 6)
                {
                    contacts.Add(contact1);
                }

                if (contact2 > 0 & contact2 <= 6)
                {
                    contacts.Add(contact2);
                }

                resistance = _randomness.NextDouble() * 200 + 25;

                if (contacts.Count > 0)
                {
                    ac.Activity.TransformTracing<TestTracing>().Trace(t =>
                    {
                        t.FaultyContacts = [.. contacts];
                        t.Resistance = resistance;
                        t.Contact = contacts[0];
                        t.EnergyConsumption = NominalPower * (DateTime.Now - ac.Activity.Tracing.Started).Value.TotalSeconds;
                    });
                }
                else
                {
                    ac.Activity.TransformTracing<TestTracing>().Trace(t =>
                    {
                        t.FaultyContacts = [.. contacts];
                        t.Resistance = resistance;
                        t.EnergyConsumption = NominalPower * (DateTime.Now - ac.Activity.Tracing.Started).Value.TotalSeconds;
                    });
                }
            }
            else
            {
                resistance = _randomness.NextDouble() * 0.1;
                ac.Activity.TransformTracing<TestTracing>().Trace(t =>
                {
                    t.EnergyConsumption = NominalPower * (DateTime.Now - ac.Activity.Tracing.Started).Value.TotalSeconds;
                    t.Resistance = resistance;
                });
            }
        }

        var output = activityStart.CreateResult((int)result);
        PublishActivityCompleted(output);
    }

    protected override IEnumerable<Session> ProcessEngineAttached()
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<Session> ProcessEngineDetached()
    {
        throw new NotImplementedException();
    }
    #endregion
}
