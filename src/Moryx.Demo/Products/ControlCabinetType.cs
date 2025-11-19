// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.Demo.Properties;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Moryx.AbstractionLayer.Products;
using Moryx.Serialization;

namespace Moryx.Demo.Products;

[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CONTROL_CABINET))]
public class ControlCabinetType : ProductType
{

    [DisplayName("Part Link"), Description(" Parts")]
    public List<ProductPartLink<CircuitBoardType>> Parts { get; set; } = [];

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.WEIGHT))]
    public double Weight { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.COLOR))]
    public MetalColor Color { get; set; }


    protected override ProductInstance Instantiate()
    {
        return new ControlCabinetInstance
        {
            Parts = Parts.Instantiate<CircuitBoardInstance>()
        };

    }
}

public enum MetalColor
{
    Black,
    White,
    Grey
}
