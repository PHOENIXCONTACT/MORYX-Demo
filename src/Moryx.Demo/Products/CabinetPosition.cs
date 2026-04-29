// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using Moryx.Serialization;

namespace Moryx.Demo.Products;

/// <summary>
/// DTO for defining the position in a cabinet
/// </summary>
public class CabinetPosition
{
    [EntrySerialize]
    [Display(Name = "Rail Number", Description = "Defines the rail within the cabinet")]
    [Range(1, 10)]
    public int RailNumber { get; set; }

    [EntrySerialize]
    [Display(Name = "Rail Position", Description = "Defines the position on the rail")]
    [Range(0, 100)]
    public int RailPosition { get; set; }

    public string HiddenUnpersistedProperty { get; set; } = Guid.NewGuid().ToString();
}
