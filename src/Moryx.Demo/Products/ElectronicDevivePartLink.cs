// Copyright (c) 2026, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Products;
using Moryx.Serialization;

namespace Moryx.Demo.Products;

/// <summary>
/// An example for a custom product part link with parameters
/// </summary>
public class ElectronicDevivePartLink : ProductPartLink<ElectronicDeviceType>
{
    [EntrySerialize, DataMember]
    [Display(Name = "Name", Description = "A descriptive name for the component")]
    public string Name { get; set; }

    [EntrySerialize, DataMember]
    [Display(Name = "Position", Description = "The position of the electronic device in the cabinet")]
    public CabinetPosition Position { get; set; }
}
