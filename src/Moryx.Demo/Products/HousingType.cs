// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using Moryx.AbstractionLayer.Products;
using Moryx.Demo.Properties;

namespace Moryx.Demo.Products;

[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HOUSING))]
public class HousingType : ProductType
{
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.COLOR))]
    public PlasticColor Color { get; set; }

    protected override ProductInstance Instantiate()
    {
        return new HousingInstance();
    }
}

public enum PlasticColor
{
    Gruen,
    Rot,
    Schwarz,
    Grau
}
