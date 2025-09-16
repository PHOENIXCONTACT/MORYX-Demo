// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Cells;
using System;
using System.Collections.Generic;
using Moryx.Resources.Demo.Messages;
using Moryx.Simulation;
using Moryx.Demo.Activities;

namespace Moryx.Resources.Demo.SimulatedDriver;

[ResourceRegistration]
public class SimulatedAssemblyDriver : DemoSimulatedDriverBase
{

    [ResourceReference(ResourceRelationType.Driver, ResourceReferenceRole.Source)]
    public AssemblyCell Cell { get; set; }

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
        if (result.Activity is not AssemblyActivity)
        {
            return;
        }

        Received?.Invoke(this, new AssemblyCompletedMessage { Result = result.Result });
    }

    public override void Send(object payload)
    {
        switch (payload)
        {
            case AssembleProductMessage:
                SimulatedState = SimulationState.Executing;
                break;
            case ReleaseWorkpieceMessage:
                SimulatedState = SimulationState.Idle;
                break;
        }
    }
}
