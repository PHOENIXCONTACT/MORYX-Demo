// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moryx.AbstractionLayer.Products;
using Moryx.Container;
using Moryx.Orders.Assignment;

namespace Moryx.Orders.Demo;

/// <summary>
/// Product assignment for the Demo
/// </summary>
[Plugin(LifeCycle.Singleton, typeof(IProductAssignment), Name = nameof(DemoProductAssignment))]
public class DemoProductAssignment : ProductAssignmentBase<ProductAssignmentConfig>
{
    /// <inheritdoc />
    public override Task<IProductType> SelectProduct(Operation operation, IOperationLogger operationLogger)
    {
        var productIdentity = (ProductIdentity)operation.Product.Identity;
        var selectedType = ProductManagement.LoadType(productIdentity);

        if (selectedType == null)
        {
            operationLogger.Log(LogLevel.Error, "Product not found");
            return null;
        }

        return Task.FromResult(selectedType);
    }
}