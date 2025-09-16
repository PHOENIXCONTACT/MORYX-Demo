// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Cells;
using Moryx.Resources.Demo.Messages;
using Moryx.Resources.Demo.SimulatedDriver;
using Moryx.Simulation;
using System;
using System.Collections.Generic;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
public class SimulatedTestDriver : DemoSimulatedDriverBase
{
    [ResourceReference(ResourceRelationType.Driver, ResourceReferenceRole.Source)]
    public TestSimulationCell Cell { get; set; }

    public override IEnumerable<ICell> Usages => [Cell];

    public override event EventHandler<object> Received;

    public override void Ready(IActivity activity)
    {
        Ready(activity.Process.Id);
    }

    public override void Ready(long processId)
    {
        SimulatedState = SimulationState.Requested;
        Received?.Invoke(this, new WorkpieceArrivedMessage { ProcessId = processId });
    }

    public override void Result(SimulationResult result)
    {
        Received?.Invoke(this, new TestCompletedMessage { Result = result.Result });
    }

    public override void Send(object payload)
    {
        switch (payload)
        {
            case TestProductMessage:
                SimulatedState = SimulationState.Executing;
                break;
            case ReleaseWorkpieceMessage:
                SimulatedState = SimulationState.Idle;
                break;
        }
    }
}
