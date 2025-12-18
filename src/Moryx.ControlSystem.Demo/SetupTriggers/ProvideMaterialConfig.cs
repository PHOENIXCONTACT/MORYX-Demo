// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Moryx.ControlSystem.Setups;
using Moryx.Demo.Products;
using Moryx.Serialization;

namespace Moryx.ControlSystem.Demo.SetupTriggers;

public class ProvideMaterialConfig : SetupTriggerConfig
{
    public override string PluginName => nameof(ProvideMaterialTrigger);

    [DataMember]
    [AllowedValuesAttribute(nameof(ElectronicDeviceType.Housing), nameof(ElectronicDeviceType.CircuitBoard))]
    public string Property { get; set; }
}
