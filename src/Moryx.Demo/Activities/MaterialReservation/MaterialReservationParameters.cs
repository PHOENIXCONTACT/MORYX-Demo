// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Products;

namespace Moryx.Demo.Activities;

public class MaterialReservationParameters : Parameters
{
    public bool Reserve { get; set; }

    public string Order { get; set; }

    public IProductType Material { get; set; }

    protected override void Populate(IProcess process, Parameters instance)
    {
        var parameters = (MaterialReservationParameters)instance;
        parameters.Reserve = Reserve;
        parameters.Order = Order;
        parameters.Material = Material;
    }
}