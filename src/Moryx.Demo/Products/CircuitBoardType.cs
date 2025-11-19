// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using Moryx.AbstractionLayer.Products;
using Moryx.Demo.Properties;
using Moryx.Serialization;

namespace Moryx.Demo.Products;

[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PRINTED_CIRCUIT_BOARD))]
public class CircuitBoardType : ProductType
{
    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.VOLTAGE))]
    public int Voltage { get; set; }

    [EntrySerialize, Display(ResourceType = typeof(Strings), Name = nameof(Strings.SOLDERING_POINT))]
    public SolderingPoints[] SolderingPoints { get; set; }

    protected override ProductInstance Instantiate()
    {
        return new CircuitBoardInstance();
    }
}

public class SolderingPoints
{
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.NUMBER))]
    public int Number { get; set; }

    public int PosX { get; set; }

    public int PosY { get; set; }

    [PrimitiveValues(0.75, 1.5, 2.5), Display(ResourceType = typeof(Strings), Name = nameof(Strings.DIAMETER))]
    public double Diameter { get; set; }

    public override string ToString()
    {
        return $"{Number}-({PosX},{PosY})-{Diameter}";
    }
}
