// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.AbstractionLayer.Products;
using Moryx.AbstractionLayer.Resources;
using Moryx.Bindings;
using Moryx.ControlSystem.Activities;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.Materials;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Properties;
using Moryx.Factory;
using Moryx.Notifications;
using Moryx.Resources.Demo.Messages;
using Moryx.Serialization;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
public class AssemblyCell : DemoCellBase, IMaterialContainer, INotificationSender
{
    private readonly IBindingResolverFactory _resolverFactory = new ResourceBindingResolverFactory();

    [ResourceReference(ResourceRelationType.Extension)]
    public IVisualInstructor Instructor { get; set; }

    [ResourceReference(ResourceRelationType.Custom)]
    public IVisualInstructor SetupInstructor { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.NOMINAL_POWER))]
    [EntryVisualization("W", "electrical_services")]
    public override int NominalPower { get => 225; }

    [DataMember, EntrySerialize, ReadOnly(true), Display(ResourceType = typeof(Strings), Name = nameof(Strings.MATERIAL_IDENTIFIER))]
    [EntryVisualization("", "category")]
    public string MaterialIdentifier { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.RESERVATIONS))]
    public List<string> Reservations { get; set; } = [];

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.INSTANCE_COUNT))]
    public int InstanceCount { get; set; }

    public ProductType ProvidedMaterial { get; set; }

    [DataMember, EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.MANUAL_MODE))]
    [EntryVisualization("", "sign_language")]
    public bool ManualMode { get; set; }

    public ICell SuppliedCell => this;

    private long _instructionId;

    public event EventHandler MaterialChanged;
    public event EventHandler FillingLevelChanged;

    protected override void OnInitialize()
    {
        if (!string.IsNullOrEmpty(MaterialIdentifier))
        {
            ProvidedMaterial = new ProductReference(new ProductIdentity(MaterialIdentifier, ProductIdentity.LatestRevision));
        }

        base.OnInitialize();
        UpdateCapabilities();
        Driver.Received += OnMessageReceived;
    }

    protected override void OnDispose()
    {
        Driver.Received -= OnMessageReceived;

        base.OnDispose();
    }

    public override IEnumerable<Session> ControlSystemAttached()
    {
        yield return Session.StartSession(ActivityClassification.Setup, ReadyToWorkType.Push);
    }

    private void OnMessageReceived(object sender, object message)
    {
        switch (message)
        {
            case WorkpieceArrivedMessage arrived:
                CellState = "Running";
                CurrentSession = Session.StartSession(ActivityClassification.Production, ReadyToWorkType.Pull, arrived.ProcessId);
                PublishReadyToWork((ReadyToWork)CurrentSession);
                break;
            case AssemblyCompletedMessage completed:
                if (CurrentSession is ActivityStart _activityStart)
                {
                    var result = _activityStart.CreateResult(completed.Result);
                    FillingLevelChanged.Invoke(this, null);
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
        switch (activityStart.Activity)
        {
            case AssemblyActivity assembly:
                if (ManualMode)
                {
                    _instructionId = Instructor.Execute(Name, activityStart, CompletedInstruction);
                }
                else
                {
                    Driver.Send(new AssembleProductMessage { ActivityId = activityStart.Activity.Id });
                }

                break;

            case MaterialChangeActivity materialChange:
                var instructionResolver = new VisualInstructionBinder(materialChange.Parameters.Instructions, _resolverFactory);
                var resolvedInstructions = instructionResolver.ResolveInstructions(this);
                _instructionId = (SetupInstructor ?? Instructor).Execute(Name, activityStart, CompleteMaterialChange, resolvedInstructions);
                break;

            case MaterialReservationActivity materialReservation:
                if (materialReservation.Parameters.Reserve)
                {
                    Reservations.Add(materialReservation.Parameters.Order);
                }
                else
                {
                    Reservations.Remove(materialReservation.Parameters.Order);
                }

                UpdateCapabilities();

                var result = activityStart.CreateResult(0);
                PublishActivityCompleted(result);
                break;
        }
    }

    public override void ProcessAborting(IActivity affectedActivity)
    {
        Instructor.Clear(_instructionId);
        _instructionId = 0;
        PublishResult((int)DefaultActivityResult.Failed);
    }

    private void CompleteMaterialChange(int result, ActivityStart session)
    {
        var materialChange = (MaterialChangeActivity)session.Activity;
        SetMaterial(materialChange.Parameters.Material);

        UpdateCapabilities();
        MaterialChanged?.Invoke(this, EventArgs.Empty);
        RaiseResourceChanged();

        _instructionId = 0;
        PublishResult(result);
    }

    private void CompletedInstruction(int result, ActivityStart session)
    {
        _instructionId = 0;
        var mcresult = session.CreateResult(result);
        PublishActivityCompleted(mcresult);
    }

    public void SetMaterial(ProductType material)
    {
        MaterialIdentifier = material?.Identity.Identifier;
        ProvidedMaterial = material;
    }

    [EntrySerialize]
    public void ResetMaterial()
    {
        Reservations.Clear();
        SetMaterial(null);
        UpdateCapabilities();
        MaterialChanged?.Invoke(this, EventArgs.Empty);
        RaiseResourceChanged();
    }

    protected override void UpdateCapabilities()
    {
        if (_disabled)
        {
            Capabilities = NullCapabilities.Instance;
        }
        else
        {
            Capabilities = new CombinedCapabilities(
            [
                new AssemblyCapabilities
                {
                    Reservations = Reservations,
                    EquippedMaterial = ProvidedMaterial,
                },
                new MaterialCapabilities()
            ]);
        }
    }

    public override void SequenceCompleted(SequenceCompleted completed)
    {
        if (completed.AcceptedClassification == ActivityClassification.Setup)
        {
            var rtw = Session.StartSession(ActivityClassification.Setup, ReadyToWorkType.Push);
            // ToDo: Fix issue that another material setup can be dispatched to this cell, overriding
            // the material that was just setup.
            //PublishReadyToWork(rtw);
        }
        else if (completed.AcceptedClassification == ActivityClassification.Production)
        {
            base.SequenceCompleted(completed);
        }
    }
}
