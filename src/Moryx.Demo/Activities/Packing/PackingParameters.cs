// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Processes;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Serialization;
using Moryx.VisualInstructions;

namespace Moryx.Demo.Activities;

public class PackingParameters : VisualInstructionParameters, IConsultantValues
{
    public int ExecutionTimeSec => 3;

    [EntrySerialize, DataMember]
    public int Power { get; set; }

    protected override void Populate(Process process, Parameters instance)
    {
        var parameters = (PackingParameters)instance;
    }
}
