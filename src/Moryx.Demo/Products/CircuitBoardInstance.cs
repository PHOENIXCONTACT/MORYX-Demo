// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Identity;
using Moryx.AbstractionLayer.Products;

namespace Moryx.Demo.Products;

public class CircuitBoardInstance : ProductInstance<CircuitBoardType>
{
    public IIdentity Identity { get; set; }
}