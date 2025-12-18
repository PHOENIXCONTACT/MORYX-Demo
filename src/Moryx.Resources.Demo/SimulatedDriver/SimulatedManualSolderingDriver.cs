// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.Simulation;
using Moryx.Resources.Demo.Messages;
using System;
using System.Collections.Generic;

namespace Moryx.Resources.Demo.SimulatedDriver
{
    [ResourceRegistration]
    public class SimulatedManualSolderingDriver : DemoSimulatedDriverBase
    {
        [ResourceReference(ResourceRelationType.Driver, ResourceReferenceRole.Source)]
        public ManualSolderingCell Cell { get; set; }

        public override IEnumerable<ICell> Usages => [Cell];

        public override event EventHandler<object> Received;

        public override void Ready(Activity activity)
            => Received?.Invoke(this, new WorkpieceArrivedMessage { ProcessId = activity.Process.Id });

        public override void Ready(long processId) { }

        public override void Result(SimulationResult result) { }

        public override void Send(object payload)
        {
            if (payload is SolderProductMessage)
            {
                SimulatedState = SimulationState.Executing;
            }
            else if (payload is ReleaseWorkpieceMessage)
            {
                SimulatedState = SimulationState.Idle;
            }
        }
    }
}
