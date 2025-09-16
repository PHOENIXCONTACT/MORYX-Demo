// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.Demo.Products;
using Moryx.Demo.Recipes;
using Moryx.Serialization;
using System.Runtime.Serialization;

namespace Moryx.Demo.Activities;

public class TestParameters : Parameters, IConsultantValues
{
    [EntrySerialize, DataMember]
    public int Power { get; set; }

    [EntrySerialize(EntrySerializeMode.Never)]
    public int ExecutionTimeSec { get; set; }

    [EntrySerialize(EntrySerializeMode.Never)]
    public int Voltage { get; set; }

    protected override void Populate(IProcess process, Parameters instance)
    {
        var demoRecipe = (DemoRecipe)process.Recipe;
        var product = (ElectronicDeviceType)demoRecipe.Product;

        var parameters = (TestParameters)instance;
        parameters.ExecutionTimeSec = demoRecipe.TestTime;
        parameters.Voltage = product.CircuitBoard?.Product.Voltage ?? 0;
    }

}