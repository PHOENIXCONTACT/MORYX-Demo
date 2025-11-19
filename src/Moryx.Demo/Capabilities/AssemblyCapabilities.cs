// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Linq;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.AbstractionLayer.Products;

namespace Moryx.Demo.Capabilities;

public class AssemblyCapabilities : CapabilitiesBase
{
    public IReadOnlyList<string> Reservations { get; set; }

    public ProductType EquippedMaterial { get; set; }

    protected override bool ProvidedBy(ICapabilities provided)
    {
        var providedAssembly = provided as AssemblyCapabilities;
        if (providedAssembly == null)
        {
            return false;
        }

        // Check for required material
        if (EquippedMaterial != null)
        {
            return EquippedMaterial.Identity.Identifier == providedAssembly.EquippedMaterial?.Identity.Identifier;
        }

        // Check for empty reservation
        if (Reservations is { Count: 0 } && providedAssembly.Reservations.Count > 0)
        {
            return false;
        }

        // Check if reservation is present
        if (Reservations?.All(providedAssembly.Reservations.Contains) == false)
        {
            return false;
        }

        return true;
    }
}
