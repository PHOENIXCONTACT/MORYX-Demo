// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Runtime.Serialization;

namespace Moryx.Products.Demo.Importer;

/// <summary>
/// Parameters for the <see cref="DemoProductImporter"/>
/// </summary>
[DataContract]
public class DemoImportParameters
{
    [DataMember]
    public string AppLink { get; set; } = "https://localhost:5000";
}
