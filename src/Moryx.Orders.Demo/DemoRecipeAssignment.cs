// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Recipes;
using Moryx.Container;
using Moryx.ControlSystem.Recipes;
using Moryx.Demo.Recipes;
using Moryx.Orders.Assignment;

namespace Moryx.Orders.Demo;

/// <summary>
/// Recipe assignment for the Demo
/// </summary>
[Plugin(LifeCycle.Singleton, typeof(IRecipeAssignment), Name = nameof(DemoRecipeAssignment))]
public class DemoRecipeAssignment : RecipeAssignmentBase<RecipeAssignmentConfig>
{
    /// <inheritdoc />
    public override async Task<IReadOnlyList<IProductRecipe>> SelectRecipes(Operation operation, IOperationLogger operationLogger)
    {
        if (operation.CreationContext != null &&
            operation.CreationContext.RecipePreselection != 0)
        {
            var recipe = ProductManagement.LoadRecipe(operation.CreationContext.RecipePreselection);
            return [(IProductRecipe)recipe];
        }

        if (operation.Recipes.Any() && operation.Recipes[0] is IRecipe template && template.TemplateId != 0)
        {
            var recipe = ProductManagement.LoadRecipe(template.TemplateId);
            return [(IProductRecipe)recipe];
        }

        return [await LoadDefaultRecipe(operation.Product)];
    }

    /// <inheritdoc />
    public override Task<bool> ProcessRecipe(IProductRecipe clone, Operation operation, IOperationLogger operationLogger)
    {
        // Copy values from known recipe types
        // IOrderBasedRecipe
        if (clone is IOrderBasedRecipe orderBasedRecipe)
        {
            orderBasedRecipe.OrderNumber = operation.Order.Number;
            orderBasedRecipe.OperationNumber = operation.Number;
        }
        // Demo Recipe
        if (clone is DemoRecipe demoRecipe)
        {
            demoRecipe.Amount = operation.TotalAmount;
        }

        return Task.FromResult(true);
    }
}
