// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Identity;
using Moryx.AbstractionLayer.Products;

namespace Moryx.Demo.Products;

public class HousingInstance : ProductInstance<HousingType>, IIdentifiableObject
{
    public IIdentity Identity { get; set; }
}