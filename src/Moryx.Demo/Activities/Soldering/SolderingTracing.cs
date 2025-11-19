// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Activities;

namespace Moryx.Demo.Activities;

public class SolderingTracing : Tracing, IEnergyTracing
{
    [DataMember]
    public int Temperature { get; set; }

    [DataMember]
    public double EnergyConsumption { get; set; }
}
