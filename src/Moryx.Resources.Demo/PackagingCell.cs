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
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Properties;
using Moryx.Factory;
using Moryx.Notifications;
using Moryx.Resources.Demo.Messages;
using Moryx.Serialization;
using Moryx.VisualInstructions;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
public class PackagingCell : DemoCellBase, INotificationSender
{
    [ResourceReference(ResourceRelationType.Extension)]
    public IVisualInstructor BoxInstructor { get; set; }

    [ResourceReference(ResourceRelationType.Decorated)]
    public IVisualInstructor Instructor { get; set; }

    [EntrySerialize, EntryVisualization("", "pallet")]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.PACKING_COUNT))]
    private int BoxCount { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.MAINTENANCE_INSTRUCTION))]
    [DataMember, EntrySerialize, DefaultValue("https://example.url/maintenance-instruction")]
    public string MaintenanceInstruction { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.PACKING_QUANTITY))]
    [DataMember, EntrySerialize, DefaultValue(10)]
    [EntryVisualization("", "inventory")]
    public int PackagingAmount { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.NOMINAL_POWER))]
    [EntryVisualization("W", "electrical_services")]
    public override int NominalPower { get => 230; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.COMPANY))]
    [DataMember, EntrySerialize, DefaultValue("MORYX Industry")]
    [EntryVisualization("", "business")]
    public string Company { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.SKIP_PRINTING))]
    [DataMember, EntrySerialize]
    public bool SkipPrinting { get; set; }

    #region Maintenance
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.TOTAL_CYCLES))]
    [DataMember, EntrySerialize, ReadOnly(true)]
    [EntryVisualization("", "cycle")]
    public int TotalCycles { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.CYCLES_SINCE_MAINTENANCE))]
    [EntrySerialize, ReadOnly(true)]
    [EntryVisualization("", "autorenew")]
    public int CyclesSinceMaintenance { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.MAINTENANCE_THRESHOLD))]
    [EntrySerialize, DataMember]
    [EntryVisualization("", "engineering")]
    public int MaintenanceThreshold { get; set; }

    private void CheckMaintenancePeriode()
    {
        if (CyclesSinceMaintenance < MaintenanceThreshold)
        {
            return;
        }

        DoMaintenance();
    }

    [EntrySerialize]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.METHOD_DO_MAINTENANCE))]
    public void DoMaintenance()
    {
        if (Instructor is null)
        {
            return;
        }

        CellState = "Maintenance";

        if (_instructionId != 0)
        {
            Instructor.Clear(_instructionId);
        }

        _instructionId = Instructor.Execute(new ActiveInstruction()
        {
            Title = Strings.PACKING_MAINTENANCE_TITLE,
            Instructions = [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = Strings.PACKING_MAINTENANCE_MESSAGE },
                new VisualInstruction() { Type = InstructionContentType.Media, Content = MaintenanceInstruction }
            ],

            Results = [new() { Key = "postpone", DisplayValue = Strings.POSTPONE }, new() { Key = "done", DisplayValue = Strings.DONE }]
        }, MaintenanceCompleted);
    }

    private void MaintenanceCompleted(ActiveInstructionResponse response)
    {
        if (response.SelectedResult.Key == "postpone")
        {
            CellState = "Idle";
            return;
        }

        CyclesSinceMaintenance = 0;
        Instructor.Clear(_instructionId);
        _instructionId = 0;
    }
    #endregion

    #region Lifecycle
    protected override void UpdateCapabilities()
    {
        Capabilities = new PackingCapabilities();
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
            case PackagingCompletedMessage completed:
                if (CurrentSession is ActivityStart _activityStart)
                {
                    BoxCount++;

                    var result = _activityStart.CreateResult(completed.Result);
                    _activityStart.Tracing<EnergyTracing>().Trace(et => et.EnergyConsumption = NominalPower * (DateTime.Now - et.Started).Value.TotalSeconds);
                    PublishActivityCompleted(result);

                    if (BoxCount % PackagingAmount == 0)
                    {
                        BoxInstructor?.Clear(_instructionId);
                        _instructionId = BoxInstructor?.Execute(Name, _activityStart, BoxSwitched,
                        [
                            new VisualInstruction
                            {
                                Type = InstructionContentType.Text,
                                Content = $"In der Box befinden sich {BoxCount} Teile. Die aktuelle Verpackungseinheit ist {PackagingAmount}. Bitte wechseln Sie die Box!"
                            }
                        ]) ?? 0;
                    }
                }
                CellState = "Idle";
                break;
        }
    }

    private long _instructionId;
    public void BoxSwitched(int result, ActivityStart session)
    {
        BoxCount = 0;
    }

    public override void StartActivity(ActivityStart activityStart)
    {
        CurrentSession = activityStart;
        Driver.Send(new PackProductMessage { ActivityId = activityStart.Activity.Id });
    }

    public override void SequenceCompleted(SequenceCompleted completed)
    {
        base.SequenceCompleted(completed);
        IncreaceCycles();
    }

    private void IncreaceCycles()
    {
        CyclesSinceMaintenance++;
        TotalCycles++;
        CheckMaintenancePeriode();
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
