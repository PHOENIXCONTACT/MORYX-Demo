// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using Moryx.AbstractionLayer.Products;
using Moryx.Demo.Properties;

namespace Moryx.Demo.Products;

[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ARTICLE))]
public class ElectronicDeviceType : ProductType
{
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.HOUSING))]
    public ProductPartLink<HousingType> Housing { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.CIRCUIT_BOARD))]
    public ProductPartLink<CircuitBoardType> CircuitBoard { get; set; }

    protected override ProductInstance Instantiate()
    {
        return new ElectronicDeviceInstance();
    }
}