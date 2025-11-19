// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Moryx.AbstractionLayer.Recipes;
using Moryx.ControlSystem.Recipes;
using Moryx.Demo.Properties;
using Moryx.Serialization;

namespace Moryx.Demo.Recipes;

public class DemoRecipe : ProductionRecipe, IOrderBasedRecipe
{
    public int Amount { get; set; }
    public string OrderNumber { get; set; }
    public string OperationNumber { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.TEST_TIME))]
    [EntrySerialize, DefaultValue(10)]
    public int TestTime { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoRecipe"/> class.
    /// </summary>
    public DemoRecipe()
    {
    }

    /// <summary>
    /// Create a cloned <see cref="DemoRecipe"/>
    /// </summary>
    public DemoRecipe(DemoRecipe source)
        : base(source)
    {
        TestTime = source.TestTime;
    }

    /// <inheritdoc />
    public override IRecipe Clone()
    {
        return new DemoRecipe(this);
    }

}
