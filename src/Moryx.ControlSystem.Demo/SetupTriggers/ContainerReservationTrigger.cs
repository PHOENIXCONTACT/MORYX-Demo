// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Recipes;
using Moryx.Container;
using Moryx.ControlSystem.Recipes;
using Moryx.ControlSystem.Setups;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;
using Moryx.Demo.Products;
using Moryx.Modules;
using Moryx.Workplans;
using System.Collections.Generic;

namespace Moryx.ControlSystem.Demo.SetupTriggers;

[ExpectedConfig(typeof(ContainerReservationConfig))]
[Plugin(LifeCycle.Transient, typeof(ISetupTrigger), Name = nameof(ContainerReservationTrigger))]
public class ContainerReservationTrigger : SetupTriggerBase<ContainerReservationConfig>
{
    public override SetupExecution Execution => Config.Reserve ? SetupExecution.BeforeProduction : SetupExecution.AfterProduction;

    public override SetupEvaluation Evaluate(IProductRecipe recipe)
    {
        if (recipe.Product is not ElectronicDeviceType product)
        {
            return false;
        }

        // Reservation and release is always necessary
        var order = ((IOrderBasedRecipe)recipe).OrderNumber;
        var material = TriggerHelpers.ExtractMaterial(product, Config.Property);
        return Config.Reserve
            ? SetupEvaluation.Reserve(new AssemblyCapabilities { EquippedMaterial = material })
            : SetupEvaluation.Release(new AssemblyCapabilities { EquippedMaterial = material, Reservations = [order] });
    }

    public override IReadOnlyList<IWorkplanStep> CreateSteps(IProductRecipe recipe)
    {
        // Reservation and release is always necessary
        var product = (ElectronicDeviceType)recipe.Product;
        var order = ((IOrderBasedRecipe)recipe).OrderNumber;
        var material = TriggerHelpers.ExtractMaterial(product, Config.Property);

        return
        [
            new MaterialReservationTask
            {
                Parameters = new MaterialReservationParameters
                {
                    Order = order,
                    Reserve = Config.Reserve,
                    Material = material
                }
            }
        ];
    }
}
