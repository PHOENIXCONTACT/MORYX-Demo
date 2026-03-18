// Copyright (c) 2026 Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Drivers;
using Moryx.AbstractionLayer.Resources;
using Moryx.Container;
using Moryx.Serialization;

namespace Moryx.Resources.Samples;

public class AvailableResources(Type type, bool returnId) : PossibleValuesAttribute
{
    public override bool OverridesConversion { get => true; }
    public override bool UpdateFromPredecessor { get => true; }

    public override IEnumerable<string> GetValues(IContainer localContainer, IServiceProvider serviceProvider)
    {
        var graph = localContainer.Resolve<IResourceGraph>();
        var list =
            graph.GetResources<Resource>(res => res.GetType() == type)
            .Select(resource => $"{resource.Id}: {resource.Name}").ToArray();
        return list;
    }

    public override object Parse(IContainer container, IServiceProvider serviceProvider, string value)
    {
        var id = long.Parse(value.Substring(0, value.IndexOf(":")));
        if (returnId)
            return id;
        return container.Resolve<IResourceGraph>().GetResource<Resource>(id);
    }
}

[ResourceRegistration]
[DependencyRegistration(InstallerMode.All)]
public class TestCell : Resource
{
    [ResourceReference(ResourceRelationType.Driver, IsRequired = true)]
    public TestDriver Driver { get; set; }
    [ResourceConstructor]
    public void CreateWithExistingDriver([AvailableResources(typeof(TestDriver), returnId: false)] TestDriver driver)
    {
        Driver = driver;
    }

    [ResourceConstructor]
    public void CreateWithExistingDriverId([AvailableResources(typeof(TestDriver), returnId: true)] long driverId)
    {
        Driver = Graph.GetResource<TestDriver>(driverId);
    }

    [ResourceConstructor]
    public async Task CreateDriverAsync(string driverName, string initial)
    {
        var driver = Graph.Instantiate<TestDriver>(driverName, "");
        driver.Strings ??= new();
        driver.Strings.Add(initial);
        await Graph.SaveAsync(driver);
        Driver = driver;
    }

    [ResourceConstructor]
    public void CreateDriver(string driverName, string initial, bool save)
    {
        var driver = Graph.Instantiate<TestDriver>(driverName, "");
        driver.Strings ??= new();
        driver.Strings.Add(initial);
        if(save)
        {
            Graph.SaveAsync(driver).GetAwaiter().GetResult();
        }
        Driver = driver;
    }
}

[ResourceRegistration]
public class TestDriver : Driver
{

    [DataMember, EntrySerialize]
    public List<string> Strings { get; set; }
}

