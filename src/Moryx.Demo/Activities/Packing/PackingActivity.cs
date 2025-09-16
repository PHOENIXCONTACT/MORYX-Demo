// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.ControlSystem.Activities;
using Moryx.Demo.Capabilities;

namespace Moryx.Demo.Activities;

[ActivityResults(typeof(PackingResult))]
public class PackingActivity : Activity<PackingParameters, EnergyTracing>, IMountingActivity
{
    public override ProcessRequirement ProcessRequirement => ProcessRequirement.Required;

    public override ICapabilities RequiredCapabilities => new PackingCapabilities();

    public MountOperation Operation => MountOperation.Unmount;

    protected override ActivityResult CreateResult(long resultNumber)
    {
        return ActivityResult.Create((PackingResult)resultNumber);
    }

    protected override ActivityResult CreateFailureResult()
    {
        return ActivityResult.Create(PackingResult.Scrap);
    }
}