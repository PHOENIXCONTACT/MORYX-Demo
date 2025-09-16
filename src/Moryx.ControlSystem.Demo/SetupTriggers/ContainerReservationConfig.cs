// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Runtime.Serialization;
using Moryx.ControlSystem.Setups;
using Moryx.Demo.Products;
using Moryx.Serialization;

namespace Moryx.ControlSystem.Demo.SetupTriggers;

public class ContainerReservationConfig : SetupTriggerConfig
{
    public override string PluginName => nameof(ContainerReservationTrigger);

    [DataMember]
    public bool Reserve { get; set; }

    [DataMember]
    [PrimitiveValues(nameof(ElectronicDeviceType.Housing), nameof(ElectronicDeviceType.CircuitBoard))]
    public string Property { get; set; }
}