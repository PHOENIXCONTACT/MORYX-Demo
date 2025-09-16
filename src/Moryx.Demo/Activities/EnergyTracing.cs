// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using System.Runtime.Serialization;

namespace Moryx.Demo.Activities;

public class EnergyTracing : Tracing, IEnergyTracing
{
    [DataMember]
    public double EnergyConsumption { get; set; }
}

public interface IEnergyTracing
{
    public double EnergyConsumption { get; set; }
}
