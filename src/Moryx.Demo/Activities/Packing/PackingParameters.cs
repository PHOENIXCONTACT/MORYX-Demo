// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Serialization;
using System.Runtime.Serialization;

namespace Moryx.Demo.Activities;

public class PackingParameters : VisualInstructionParameters, IConsultantValues
{
    public int ExecutionTimeSec => 3;

    [EntrySerialize, DataMember]
    public int Power { get; set; }

    protected override void Populate(IProcess process, Parameters instance)
    {
        var parameters = (PackingParameters)instance;
    }
}