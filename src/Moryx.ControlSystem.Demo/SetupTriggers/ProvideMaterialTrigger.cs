// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Recipes;
using Moryx.Container;
using Moryx.ControlSystem.Setups;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Products;
using Moryx.Modules;
using Moryx.Workplans;
using System.Collections.Generic;

namespace Moryx.ControlSystem.Demo.SetupTriggers;

[ExpectedConfig(typeof(ProvideMaterialConfig))]
[Plugin(LifeCycle.Transient, typeof(ISetupTrigger), Name = nameof(ProvideMaterialTrigger))]
public class ProvideMaterialTrigger : SetupTriggerBase<ProvideMaterialConfig>
{
    public override SetupExecution Execution => SetupExecution.BeforeProduction; // SetupExecution.AfterProduction

    public override SetupEvaluation Evaluate(IProductRecipe recipe)
    {
        if (recipe.Product is not ElectronicDeviceType product)
        {
            return false;
        }

        var material = TriggerHelpers.ExtractMaterial(product, Config.Property);
        return SetupEvaluation.Provide(new AssemblyCapabilities { EquippedMaterial = material }, SetupClassification.Manual);
    }

    public override IReadOnlyList<IWorkplanStep> CreateSteps(IProductRecipe recipe)
    {
        var product = (ElectronicDeviceType)recipe.Product;
        var material = TriggerHelpers.ExtractMaterial(product, Config.Property);

        return
        [
            new MaterialChangeTask
            {
                Parameters = new MaterialChangeParameters
                {
                    Material = material,
                    Instructions =
                    [
                        new VisualInstruction
                        {
                            Type = InstructionContentType.Text,
                            Content = "Please setup " + material.Identity.Identifier + "-" + material.Name + " in {Resource.Name}"
                        }
                    ]
                }
            }
        ];
    }
}