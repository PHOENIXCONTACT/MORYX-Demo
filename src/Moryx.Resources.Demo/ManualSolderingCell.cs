// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Activities;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.Processes;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Properties;
using Moryx.Factory;
using Moryx.Resources.Demo.Messages;
using Moryx.Resources.Demo.ResourcesInputs;
using Moryx.Serialization;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Processes;
using Moryx.VisualInstructions;
using System.Threading;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
public class ManualSolderingCell : DemoCellBase, IProcessReporter
{
    private readonly Dictionary<Guid, long> _currentSessionInstructionMappings = [];

    #region Constructors

    [ResourceConstructor, Display(Name = "Default setup for resource")]
    public void Construct([Display(Name = "Number of positions (max. 10)"), Range(0, 10)] short numberOfProcessHolder)
    {
        var processHolderGroup = Graph.Instantiate<ProcessHolderGroup>();
        processHolderGroup.Name = "Manual Soldering Workspace";
        processHolderGroup.Description = "The set of places to hold processes for manual soldering";
        Workspace = processHolderGroup;

        for (var i = 0; i < numberOfProcessHolder; i++)
        {
            var position = Graph.Instantiate<ProcessHolderPosition>();
            position.Name = "Manual Soldering Workspace Position";
            Workspace.Positions.Add(position);
        }
    }

    [ResourceConstructor(IsDefault = true), Display(Name = "Setup resource with process-holder positions")]
    public void Construct() => Construct(4);

    #endregion

    #region References

    [ResourceReference(ResourceRelationType.Extension)]
    public IVisualInstructor Instructor { get; set; }

    private readonly object _workspaceLock = new();
    [ResourceReference(ResourceRelationType.Extension)]
    public ProcessHolderGroup Workspace { get; set; }

    #endregion

    #region Properties

    [EntrySerialize, DataMember, DefaultValue(180)]
    [EntryVisualization("°C", "device_thermostat")]
    public int SolderingIronTemperature { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.NOMINAL_POWER))]
    [EntryVisualization("W", "electrical_services")]
    public override int NominalPower { get => 240; }

    #endregion

    #region Lifecycle

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);
        UpdateCapabilities();
    }

    protected override Task OnStartAsync(CancellationToken cancellationToken)
    {
        Driver.Received += OnMessageReceived;

        return base.OnStartAsync(cancellationToken);
    }

    protected override Task OnStopAsync(CancellationToken cancellationToken)
    {
        if (Driver != null)
        {
            Driver.Received -= OnMessageReceived;
        }

        return base.OnStopAsync(cancellationToken);
    }

    protected override void UpdateCapabilities()
        => Capabilities = _disabled ? NullCapabilities.Instance : new SolderingCapabilities { ManualSoldering = true };

    private void OnMessageReceived(object sender, object message)
    {
        ReadyToWork rtw;
        lock (_workspaceLock)
        {
            if (message is not WorkpieceArrivedMessage arrived || !Workspace.TryGetEmptyPosition(out var position))
            {
                Logger.LogWarning("Recieved unexpected message {message} from {sender}", message, sender);
                return;
            }
            rtw = Session.StartSession(ActivityClassification.Production, ReadyToWorkType.Pull, arrived.ProcessId);
            position.Mount(rtw);
        }
        PublishReadyToWork(rtw);
        CellState = "Running";
        if (Workspace.IsFull())
        {
            Driver?.Send(new SolderProductMessage());
        }
    }

    protected override IEnumerable<Session> ProcessEngineAttached()
    {
        lock (_workspaceLock)
        {
            return Workspace.Attach();
        }
    }

    protected override IEnumerable<Session> ProcessEngineDetached()
    {
        lock (_workspaceLock)
        {
            return Workspace.Detach();
        }
    }

    #endregion

    #region Session Handling

    public override void StartActivity(ActivityStart activityStart)
    {
        var position = Workspace.GetPositionBySession(activityStart);
        if (position is null)
        {
            Logger.LogWarning("Recieved unknown session {session} in {method}", activityStart, nameof(ICell.StartActivity));
            return;
        }
        position.Update(activityStart);
        _currentSessionInstructionMappings[activityStart.Id] = Instructor.Execute(Name, activityStart, new ManualSolderingCellInputs(), SolderingCompleted);
    }

    private void SolderingCompleted(int result, ManualSolderingCellInputs input, ActivityStart session)
    {
        var position = Workspace.GetPositionBySession(session);
        if (position is null)
        {
            Logger.LogWarning("Recieved instruction result for unkonw {session}", session);
            return;
        }

        session.Tracing<SolderingTracing>().Trace(st =>
        {
            st.Temperature = SolderingIronTemperature;
            st.EnergyConsumption = NominalPower * (DateTime.Now - st.Started).Value.TotalSeconds;
        });

        var completed = session.CreateResult(result);
        position.Update(completed);
        PublishActivityCompleted(completed);

        if (Workspace.IsEmpty())
        {
            CellState = "Idle";
        }
    }

    public override void ProcessAborting(Activity affectedActivity)
    {
        var position = Workspace.GetPositionByActivityId(affectedActivity.Id);
        if (position is null)
        {
            PublishActivityCompleted(Session.WrapUnknownActivity((Activity)affectedActivity));
            return;
        }

        // Report current activity as failed
        affectedActivity.Fail();
        var result = position.ConvertSession<ActivityStart>()
            .CreateResult(affectedActivity.Result.Numeric);
        position.Update(result);
        PublishActivityCompleted(result);
        Instructor?.Clear(_currentSessionInstructionMappings[result.Id]);
    }

    public override void SequenceCompleted(SequenceCompleted completed)
    {
        var position = Workspace.GetPositionBySession(completed);
        if (position is null)
        {
            Logger.LogWarning("Recieved unknown session {session} in {method}", completed, nameof(ICell.SequenceCompleted));
            return;
        }
        if (!completed.NextCells.Contains(Id))
        {
            position.Unmount();
            Driver?.Send(new ReleaseWorkpieceMessage());
            return;
        }
        var rtw = completed.ContinueSession();
        position.Update(rtw);
        PublishReadyToWork(rtw);
    }

    #endregion

    #region IProcessRemoval

    [EntrySerialize, Display(Name = "Report broken process", Description = "Uses the IProcessReporter interface to report the current process as broken")]
    public void ReportBrokenProcess() => Report(p => ProcessBroken?.Invoke(this, p));

    [EntrySerialize, Display(Name = "Report removed process", Description = "Uses the IProcessReporter interface to report the current process as removed")]
    public void ReportRemovedProcess() => Report(p => ProcessRemoved?.Invoke(this, p));

    private void Report(Action<Process> invocation)
    {
        if (Workspace.IsEmpty())
        {
            return;
        }

        var processes = Array.Empty<Process>();
        lock (_workspaceLock)
        {
            processes = [.. Workspace.Positions.Where(p => !p.IsEmpty()).Select(p => p.Process)];
        }
        Parallel.ForEach(processes, invocation);
    }

    public event EventHandler<Process> ProcessBroken;
    public event EventHandler<Process> ProcessRemoved;

    #endregion
}
