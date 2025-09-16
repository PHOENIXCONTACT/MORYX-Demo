// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Products;
using Moryx.Demo.Properties;
using System.ComponentModel.DataAnnotations;

namespace Moryx.Demo.Products;

[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PLUG_PRODUCT_TYPE))]
public class PlugElectronicType : ElectronicDeviceType
{
    public ProductPartLink<ElectronicDeviceType> Plug { get; set; }
}
