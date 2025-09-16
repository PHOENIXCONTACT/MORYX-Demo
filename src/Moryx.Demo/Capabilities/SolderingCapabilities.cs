// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Capabilities;

namespace Moryx.Demo.Capabilities;

public class SolderingCapabilities : CapabilitiesBase
{
    public bool ManualSoldering { get; set; }

    protected override bool ProvidedBy(ICapabilities provided)
    {
        var providedSoldering = provided as SolderingCapabilities;
        if (providedSoldering == null)
        {
            return false;
        }

        if (ManualSoldering && !providedSoldering.ManualSoldering)
        {
            return false;
        }

        return true;
    }
}