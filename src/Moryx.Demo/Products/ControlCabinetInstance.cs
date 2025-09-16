// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Identity;
using Moryx.AbstractionLayer.Products;
using System.Collections.Generic;

namespace Moryx.Demo.Products;

public class ControlCabinetInstance : ProductInstance<ControlCabinetType>, IIdentifiableObject
{
    public IIdentity Identity { get; set; }
    public ICollection<CircuitBoardInstance> Parts { get; set; }
}
