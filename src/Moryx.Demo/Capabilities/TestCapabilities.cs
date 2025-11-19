// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Capabilities;

namespace Moryx.Demo.Capabilities;

public class TestCapabilities : CapabilitiesBase
{
    public int Voltage { get; set; }

    protected override bool ProvidedBy(ICapabilities provided)
    {
        var providedTest = provided as TestCapabilities;
        if (providedTest == null)
        {
            return false;
        }

        if (Voltage > 0 && Voltage != providedTest.Voltage)
        {
            return false;
        }

        return true;
    }
}
