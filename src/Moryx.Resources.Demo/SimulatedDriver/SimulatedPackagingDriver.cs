// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.Simulation;
using Moryx.Resources.Demo.Messages;
using System;
using System.Collections.Generic;

namespace Moryx.Resources.Demo.SimulatedDriver;

[ResourceRegistration]
public class SimulatedPackagingDriver : DemoSimulatedDriverBase
{

    [ResourceReference(ResourceRelationType.Driver, ResourceReferenceRole.Source)]
    public PackagingCell Cell { get; set; }

    public override IEnumerable<ICell> Usages => [Cell];

    public override event EventHandler<object> Received;

    public override void Ready(Activity activity)
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
        Received?.Invoke(this, new PackagingCompletedMessage { Result = result.Result });
    }

    public override void Send(object payload)
    {
        switch (payload)
        {
            case PackProductMessage:
                SimulatedState = SimulationState.Executing;
                break;
            case ReleaseWorkpieceMessage:
                SimulatedState = SimulationState.Idle;
                break;
        }
    }

}

