// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Processes;
using Moryx.AbstractionLayer.Products;
using Moryx.AbstractionLayer.Recipes;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Products;
using Moryx.Serialization;
using Moryx.VisualInstructions;

namespace Moryx.Demo.Activities;

public class AssemblyParameters : VisualInstructionParameters, IConsultantValues
{
    [EntrySerialize, DataMember]
    public int Power { get; set; }

    [EntrySerialize, DataMember]
    [AllowedValuesAttribute(nameof(ElectronicDeviceType.Housing), nameof(ElectronicDeviceType.CircuitBoard))]
    public string Property { get; set; }

    [DefaultValue(10)]
    [EntrySerialize, DataMember]
    public int ExecutionTimeSec { get; set; }

    public ProductType Material { get; set; }

    protected override void Populate(Process process, Parameters instance)
    {
        base.Populate(process, instance);

        var parameters = (AssemblyParameters)instance;
        parameters.Property = Property;
        parameters.ExecutionTimeSec = ExecutionTimeSec;

        var product = (process.Recipe as IProductRecipe)?.Product as ElectronicDeviceType;
        if (product == null)
        {
            return;
        }

        switch (Property)
        {
            case nameof(ElectronicDeviceType.Housing):
                parameters.Material = product.Housing.Product;
                break;
            case nameof(ElectronicDeviceType.CircuitBoard):
                parameters.Material = product.CircuitBoard.Product;
                break;
        }
    }
}
