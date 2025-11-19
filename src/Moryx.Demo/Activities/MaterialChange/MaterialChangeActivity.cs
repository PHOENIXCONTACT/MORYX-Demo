// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.ControlSystem.Activities;
using Moryx.Demo.Activities.Moryx.Demo.Activities.MaterialChange;
using Moryx.Demo.Capabilities;

namespace Moryx.Demo.Activities;

[ActivityResults(typeof(MaterialChangeResults))]
public class MaterialChangeActivity : Activity<MaterialChangeParameters, EnergyTracing>, IControlSystemActivity
{
    public ActivityClassification Classification => ActivityClassification.Setup;

    public override ProcessRequirement ProcessRequirement => ProcessRequirement.NotRequired;

    public override ICapabilities RequiredCapabilities => new AssemblyCapabilities
    {
        Reservations = [] // Only change unreserved cells
    };

    protected override ActivityResult CreateResult(long resultNumber)
    {
        return ActivityResult.Create((MaterialChangeResults)resultNumber);
    }

    protected override ActivityResult CreateFailureResult()
    {
        return ActivityResult.Create(MaterialChangeResults.Failed);
    }

}
