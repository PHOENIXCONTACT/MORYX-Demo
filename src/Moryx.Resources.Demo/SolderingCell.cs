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
using Moryx.AbstractionLayer.Capabilities;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Activities;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Properties;
using Moryx.Factory;
using Moryx.Resources.Demo.Messages;
using Moryx.Serialization;
using Moryx.Threading;
using Moryx.VisualInstructions;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
public class SolderingCell : DemoCellBase
{
    private int _parallelOperationId;
    public IParallelOperations ParallelOperations { get; set; }

    private long _instructionId;
    [ResourceReference(ResourceRelationType.Extension)]
    public IVisualInstructor Instructor { get; set; }

    [DataMember, EntrySerialize, DefaultValue(200), Display(ResourceType = typeof(Strings), Name = nameof(Strings.SOLDERING_TEMPERATURE))]
    [EntryVisualization("°C", "device_thermostat")]
    public int SolderingTemperature { get; set; }

    [DataMember, EntrySerialize, DefaultValue(20), Display(ResourceType = typeof(Strings), Name = nameof(Strings.HYSTERESIS))]
    public int Hysteresis { get; set; }

    [DataMember, DefaultValue(270)]
    private int _nominalPowerThreshold;
    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.MAINTENANCE_THRESHOLD))]
    [EntryVisualization("W", "engineering")]
    public int NominalPowerThreshold { get => _nominalPowerThreshold; set => _nominalPowerThreshold = value; }

    private int _nominalPower;
    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.NOMINAL_POWER))]
    [EntryVisualization("W", "electrical_services")]
    public override int NominalPower
    {
        get => _nominalPower;
        protected set
        {
            _nominalPower = value;
            OnProcessDataOccured();
            CheckMaintenancePeriode();
        }
    }

    private void CheckMaintenancePeriode()
    {
        if (_nominalPower < _nominalPowerThreshold)
        {
            return;
        }

        if (_parallelOperationId == 0)
        {
            return;
        }

        ParallelOperations.StopExecution(_parallelOperationId);
        DoMaintenance();
    }

    [EntrySerialize]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.METHOD_DO_MAINTENANCE))]
    public void DoMaintenance()
    {
        if (CurrentSession is ReadyToWork rtw)
        {
            PublishNotReadyToWork(rtw.PauseSession());
        }
        else if (CurrentSession is ActivityStart activityStart)
        {
            ProcessAborting(activityStart.Activity);
        }

        Disabled = true;
        CellState = "Maintenance";

        _instructionId = Instructor.Execute(new ActiveInstruction()
        {
            Title = Strings.SOLDERING_MAINTENANCE_TITLE,
            Instructions = [new VisualInstruction() { Type = InstructionContentType.Text, Content = Strings.SOLDERING_MAINTENANCE_MESSAGE }],
            Results = [new() { Key = "postpone", DisplayValue = Strings.POSTPONE }, new() { Key = "done", DisplayValue = Strings.DONE }]
        }, MaintenanceCompleted);
    }

    private void MaintenanceCompleted(ActiveInstructionResponse response)
    {
        NominalPower = 240;
        Disabled = false;
        Instructor.Clear(_instructionId);
        if (CurrentSession is not NotReadyToWork nrtw)
        {
            return;
        }

        PublishReadyToWork(nrtw.ResumeSession());
    }

    #region Lifecycle
    protected override void UpdateCapabilities()
    {
        if (_disabled)
        {
            Capabilities = NullCapabilities.Instance;
        }
        else
        {
            Capabilities = new SolderingCapabilities();
        }
    }

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);
        UpdateCapabilities();

        CellState = "Idle";
        NominalPower = 240;
        Driver.Received += OnMessageReceived;
    }

    protected override void OnDispose()
    {
        Instructor?.Clear(_instructionId);
        Driver.Received -= OnMessageReceived;

        base.OnDispose();
    }
    #endregion

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
            case SolderingCompletedMessage completed:
                if (CurrentSession is ActivityStart _activityStart && _activityStart != null)
                {
                    var result = _activityStart.CreateResult(completed.Result);
                    _activityStart.Tracing<EnergyTracing>().Trace(et => et.EnergyConsumption = NominalPower * (DateTime.Now - et.Started).Value.TotalSeconds);
                    PublishActivityCompleted(result);
                }
                CellState = "Idle";
                break;
        }
    }

    public override void StartActivity(ActivityStart activityStart)
    {
        CurrentSession = activityStart;
        Driver.Send(new SolderProductMessage { ActivityId = activityStart.Activity.Id });

        var temperature = SolderingTemperature - Hysteresis;
        activityStart.Tracing<SolderingTracing>().Trace(st =>
        {
            st.Temperature = temperature;
            st.EnergyConsumption = NominalPower * (DateTime.Now - st.Started).Value.TotalSeconds;
        });
    }
    #endregion

    [EntrySerialize]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.METHOD_WEAR_CYCLE))]
    public void StartWearCycle()
    {
        _parallelOperationId = ParallelOperations.ScheduleExecution(IncreaseNominalPower, 0, 1000);
    }

    private void IncreaseNominalPower()
    {
        NominalPower++;
    }

    protected override IEnumerable<Session> ProcessEngineAttached()
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<Session> ProcessEngineDetached()
    {
        throw new NotImplementedException();
    }
}
