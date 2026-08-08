// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MimeTypes;
using Moryx.Container;
using Moryx.Modules;
using Moryx.Orders.Assignment;
using Moryx.Orders.Documents;

namespace Moryx.Orders.Demo;

[DataContract]
public class DemoDocumentLoaderConfig : DocumentLoaderConfig
{
    [DataMember]
    [DefaultValue("./Backups/Orders")]
    public string BasePath { get; set; } = "./Backups/Orders";
}

public class LocalDocument : Document
{
    public string FilePath { get; set; }

    public override Stream GetStream()
    {
        var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
        return fs;
    }
}

[Plugin(LifeCycle.Singleton, typeof(IDocumentLoader), Name = nameof(DemoDocumentLoader))]
[ExpectedConfig(typeof(DemoDocumentLoaderConfig))]
public class DemoDocumentLoader : IDocumentLoader
{
    private DemoDocumentLoaderConfig _config;

    public Task InitializeAsync(DocumentLoaderConfig config, CancellationToken cancellationToken = default)
    {
        _config = config as DemoDocumentLoaderConfig;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Document>> LoadAsync(Operation operation, CancellationToken cancellationToken)
    {
        var rawPath = _config?.BasePath;
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            rawPath = "./Backups/Orders";
        }

        var path = Path.GetFullPath(rawPath);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var productPath = Path.Combine(path, operation.Product.Identity.ToString());
        var documents = new List<Document>();
        if (Directory.Exists(productPath))
        {
            var files = Directory.GetFiles(productPath);
            foreach (var file in files)
            {
                var fileExtension = Path.GetExtension(file);
                var document = new LocalDocument
                {
                    Number = $"{documents.Count + 1}",
                    Revision = 1,
                    Description = Path.GetFileName(file),
                    Type = "Document '" + fileExtension + "'",
                    FilePath = file,
                    ContentType = MimeTypeMap.GetMimeType(fileExtension)
                };
                documents.Add(document);
            }
        }

        return Task.FromResult(documents as IReadOnlyList<Document>);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
    }
}
