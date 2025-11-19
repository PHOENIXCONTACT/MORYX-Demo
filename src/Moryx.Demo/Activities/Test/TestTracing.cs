// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Moryx.AbstractionLayer.Activities;

namespace Moryx.Demo.Activities;

public class TestTracing : Tracing, IEnergyTracing
{
    [DataMember]
    public double EnergyConsumption { get; set; }

    public int[] FaultyContacts
    {
        get => FromErrorCode(ErrorCode);
        set => ErrorCode = ToErrorCode(value);
    }

    public int Contact { get; set; }

    public double Resistance { get; set; }

    private static int ToErrorCode(int[] contacts)
    {
        var aggrated = contacts.Aggregate(0, (current, next) => (current << 3) + next);
        return aggrated + 42;
    }

    private static int[] FromErrorCode(int errorCode)
    {
        var aggrated = errorCode - 42;
        var values = new List<int>();
        while (aggrated > 0)
        {
            var value = aggrated & 0b111;
            values.Add(value);
            aggrated >>= 3;
        }
        return [.. values];
    }
}
