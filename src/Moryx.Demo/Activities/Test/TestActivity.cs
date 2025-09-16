// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.Demo.Capabilities;

namespace Moryx.Demo.Activities;

[ActivityResults(typeof(TestResult))]
public class TestActivity : Activity<TestParameters, TestTracing>
{
    public override ProcessRequirement ProcessRequirement => ProcessRequirement.Required;

    public override ICapabilities RequiredCapabilities => new TestCapabilities { Voltage = Parameters.Voltage };

    protected override ActivityResult CreateResult(long resultNumber)
    {
        return ActivityResult.Create((TestResult)resultNumber);
    }

    protected override ActivityResult CreateFailureResult()
    {
        return ActivityResult.Create(TestResult.Failed);
    }
}