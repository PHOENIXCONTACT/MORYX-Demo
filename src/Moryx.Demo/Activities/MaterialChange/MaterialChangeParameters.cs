// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Products;
using Moryx.ControlSystem.VisualInstructions;

namespace Moryx.Demo.Activities;

public class MaterialChangeParameters : VisualInstructionParameters
{
    public IProductType Material { get; set; }

    protected override void Populate(IProcess process, Parameters instance)
    {
        base.Populate(process, instance);

        var parameters = (MaterialChangeParameters)instance;
        parameters.Material = Material;
    }
}