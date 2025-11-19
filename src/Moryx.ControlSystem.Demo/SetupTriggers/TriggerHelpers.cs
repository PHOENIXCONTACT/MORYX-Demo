// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using Moryx.AbstractionLayer.Products;
using Moryx.Demo.Products;

namespace Moryx.ControlSystem.Demo.SetupTriggers;

internal static class TriggerHelpers
{
    internal static ProductType ExtractMaterial(ElectronicDeviceType product, string property)
    {
        ProductType material;
        switch (property)
        {

            case nameof(ElectronicDeviceType.Housing):
                material = product.Housing.Product;
                break;
            case nameof(ElectronicDeviceType.CircuitBoard):
                material = product.CircuitBoard.Product;
                break;
            default:
                throw new NotSupportedException("Unknown property!");
        }

        return material;
    }
}
