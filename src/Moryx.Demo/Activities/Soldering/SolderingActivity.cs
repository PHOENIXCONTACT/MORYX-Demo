// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.Demo.Capabilities;

namespace Moryx.Demo.Activities;

[ActivityResults(typeof(SolderingResults))]
public class SolderingActivity : Activity<SolderingParameters, SolderingTracing>
{
    public override ProcessRequirement ProcessRequirement => ProcessRequirement.Required;

    public override ICapabilities RequiredCapabilities => new SolderingCapabilities { ManualSoldering = Parameters.RepairSoldering };

    protected override ActivityResult CreateResult(long resultNumber)
    {
        return ActivityResult.Create((SolderingResults)resultNumber);
    }

    protected override ActivityResult CreateFailureResult()
    {
        return ActivityResult.Create(SolderingResults.Failed);
    }
}
