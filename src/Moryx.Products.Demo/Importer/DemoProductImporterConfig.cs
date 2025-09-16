// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Products;

namespace Moryx.Products.Demo.Importer;

/// <summary>
/// Config for the <see cref="DemoProductImporter"/>
/// </summary>
public class DemoProductImporterConfig : ProductImporterConfig
{
    public override string PluginName => nameof(DemoProductImporter);
}