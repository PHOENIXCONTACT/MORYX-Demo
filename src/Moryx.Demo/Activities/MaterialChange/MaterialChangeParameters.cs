// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Processes;
using Moryx.AbstractionLayer.Products;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.VisualInstructions;

namespace Moryx.Demo.Activities;

public class MaterialChangeParameters : VisualInstructionParameters
{
    public ProductType Material { get; set; }

    protected override void Populate(Process process, Parameters instance)
    {
        base.Populate(process, instance);

        var parameters = (MaterialChangeParameters)instance;
        parameters.Material = Material;
    }
}
