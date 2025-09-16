// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.AbstractionLayer.Identity;
using Moryx.ControlSystem.Activities;
using Moryx.Demo.Capabilities;

namespace Moryx.Demo.Activities;

[ActivityResults(typeof(AssemblyResults))]
public class AssemblyActivity : Activity<AssemblyParameters, EnergyTracing>, IInstanceModificationActivity, IMountingActivity
{
    public IIdentity InstanceIdentity { get; set; }

    public InstanceModificationType ModificationType { get; set; } = InstanceModificationType.Changed;

    public override ProcessRequirement ProcessRequirement => ProcessRequirement.Required;

    public override ICapabilities RequiredCapabilities => new AssemblyCapabilities
    {
        EquippedMaterial = Parameters.Material
    };

    public MountOperation Operation => MountOperation.Mount;

    protected override ActivityResult CreateResult(long resultNumber)
    {
        return ActivityResult.Create((AssemblyResults)resultNumber);
    }

    protected override ActivityResult CreateFailureResult()
    {
        return ActivityResult.Create(AssemblyResults.Failed);
    }
}