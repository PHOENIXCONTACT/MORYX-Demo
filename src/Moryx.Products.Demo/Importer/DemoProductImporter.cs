// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Products;
using Moryx.AbstractionLayer.Recipes;
using Moryx.AbstractionLayer.Workplans;
using Moryx.Container;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Activities;
using Moryx.Demo.Products;
using Moryx.Demo.Recipes;
using Moryx.Logging;
using Moryx.Modules;
using Moryx.Products.Management;
using Moryx.VisualInstructions;
using Moryx.Workplans;

namespace Moryx.Products.Demo.Importer;

/// <summary>
/// Imports products for Demo
/// </summary>
[ExpectedConfig(typeof(DemoProductImporterConfig))]
[Plugin(LifeCycle.Transient, typeof(IProductImporter), Name = nameof(DemoProductImporter))]
public class DemoProductImporter : ProductImporterBase<DemoProductImporterConfig, DemoImportParameters>, ILoggingComponent
{
    #region Dependency Injection
    /// <inheritdoc />
    public IModuleLogger Logger { get; set; }

    /// <summary>
    /// Product Storage
    /// </summary>
    public IProductStorage Storage { get; set; }

    /// <summary>
    /// Injected Workplan Storage
    /// </summary>
    public IWorkplans WorkplanStorage { get; set; }
    #endregion

    protected override Task<ProductImporterResult> ImportAsync(ProductImportContext context, DemoImportParameters parameters, CancellationToken cancellationToken)
    {
        var circuitBoardProduct = CreateCircuitBoard("1313131", 0, "Product-1 BOARD");
        var housingProduct = CreateHousing("0101010", 1, "Product-1 HOUSING GY", PlasticColor.Grau);
        var electronicDeviceProduct = CreateElectronicDevice(circuitBoardProduct, housingProduct, "2525252", 1, "Product-1");
        var controlCabinetProduct = CreateControlCabinet(
            [
                new ProductPartLink<CircuitBoardType>{ Product = circuitBoardProduct },
            ], 162, "3737373", 1, "Product-Connector-1");
        var workplan = CreateTtcWorkplan(parameters);
        var recipe = CreateRecipe(electronicDeviceProduct, workplan);

        Storage.SaveTypeAsync(circuitBoardProduct, cancellationToken);
        Storage.SaveTypeAsync(housingProduct, cancellationToken);
        Storage.SaveTypeAsync(electronicDeviceProduct, cancellationToken);
        Storage.SaveTypeAsync(controlCabinetProduct, cancellationToken);
        WorkplanStorage.SaveWorkplanAsync(workplan, cancellationToken);
        Storage.SaveRecipeAsync(recipe, cancellationToken);

        circuitBoardProduct = CreateCircuitBoard("1414141", 4, "Product-2 BOARD");
        housingProduct = CreateHousing("0202020", 5, "Product-2 HOUSING BK", PlasticColor.Schwarz);
        electronicDeviceProduct = CreateElectronicDevice(circuitBoardProduct, housingProduct, "2626262", 13, "Product-2");
        controlCabinetProduct = CreateControlCabinet(
            [
               new ProductPartLink<CircuitBoardType>{ Product = circuitBoardProduct },
            ], 131, "3838383", 1, "Product-Connector-2");

        workplan = CreateTtWorkplan(parameters);
        recipe = CreateRecipe(electronicDeviceProduct, workplan);

        Storage.SaveTypeAsync(circuitBoardProduct, cancellationToken);
        Storage.SaveTypeAsync(housingProduct, cancellationToken);
        Storage.SaveTypeAsync(electronicDeviceProduct, cancellationToken);
        Storage.SaveTypeAsync(controlCabinetProduct, cancellationToken);
        WorkplanStorage.SaveWorkplanAsync(workplan, cancellationToken);
        Storage.SaveRecipeAsync(recipe, cancellationToken);

        return Task.FromResult(new ProductImporterResult { Saved = true });
    }

    private static DemoRecipe CreateRecipe(ElectronicDeviceType electronicDeviceProduct, Workplan workplan)
    {
        return new DemoRecipe()
        {
            Classification = RecipeClassification.Default,
            Name = "Production and Packaging",
            Product = electronicDeviceProduct,
            State = RecipeState.Released,
            TestTime = 4,
            Workplan = workplan
        };
    }

    private static ElectronicDeviceType CreateElectronicDevice(CircuitBoardType circuitBoardProduct, HousingType housingProduct, string materialNumber, short revision, string name)
    {
        return new ElectronicDeviceType
        {
            Identity = new ProductIdentity(materialNumber, revision),
            Name = name,
            CircuitBoard = new ProductPartLink<CircuitBoardType> { Product = circuitBoardProduct },
            Housing = new ProductPartLink<HousingType> { Product = housingProduct },
        };
    }
    private static ControlCabinetType CreateControlCabinet(List<ProductPartLink<CircuitBoardType>> parts, double Weight, string materialNumber, short revision, string name)
    {
        return new ControlCabinetType
        {
            Identity = new ProductIdentity(materialNumber, revision),
            Name = name,
            Parts = parts,
            Weight = Weight,
        };
    }

    private static HousingType CreateHousing(string materialNumber, short revision, string name, PlasticColor color)
    {
        return new HousingType()
        {
            Identity = new ProductIdentity(materialNumber, revision),
            Name = name,
            Color = color
        };
    }

    private static CircuitBoardType CreateCircuitBoard(string materialNumber, short revision, string name)
    {
        return new CircuitBoardType()
        {
            Identity = new ProductIdentity(materialNumber, revision),
            Name = name,
            Voltage = 12,
            SolderingPoints =
            [
                new SolderingPoints() { PosX = 0, PosY = 1, Diameter = 1.5, Number = 1 },
                new SolderingPoints() { PosX = 0, PosY = 2, Diameter = 2.5, Number = 2 },
                new SolderingPoints() { PosX = 4, PosY = 1, Diameter = 1.5, Number = 3 },
                new SolderingPoints() { PosX = 4, PosY = 2, Diameter = 2.5, Number = 4 },
                new SolderingPoints() { PosX = 2, PosY = 2, Diameter = 0.75, Number = 5 },
                new SolderingPoints() { PosX = 3, PosY = 3, Diameter = 0.75, Number = 6 }
            ],
        };
    }

    private static Workplan CreateTtcWorkplan(DemoImportParameters parameters)
    {
        // Workplan
        var workplan = new Workplan()
        {
            Name = "Product-1 work plan production and packing (Imported)",
            State = WorkplanState.Released
        };

        // Boundaries
        var start = workplan.AddConnector("Start", NodeClassification.Start);
        var end = workplan.AddConnector("End", NodeClassification.End);
        var failed = workplan.AddConnector("Failed", NodeClassification.Failed);

        var housingAssembledConnection = workplan.AddConnector("HousingAssembled", NodeClassification.Intermediate);
        var housingAssemblyParameters = new AssemblyParameters()
        {
            Property = nameof(ElectronicDeviceType.Housing),
            Instructions =
            [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Insert housing {Product.Housing.Identity}." },
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Insert plug into the workpiece carrier."},
                new VisualInstruction() // Product image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/06392eda-f92b-4641-86d2-687603c654a7/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/06392eda-f92b-4641-86d2-687603c654a7/master/stream"
                },
                new VisualInstruction() //another product image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/06392eda-f92b-4641-86d2-687603c654a7/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/06392eda-f92b-4641-86d2-687603c654a7/master/stream"
                }
            ],
            ExecutionTimeSec = 3
        };

        var housingAssemblyStep = new AssemblyTask();
        workplan.AddStep(housingAssemblyStep, housingAssemblyParameters, [start], [housingAssembledConnection, housingAssembledConnection]);

        var circuitBoardAssembledConnection = workplan.AddConnector("CircuitBoardAssembled", NodeClassification.Intermediate);
        var circuitBoardAssemblyParameters = new AssemblyParameters()
        {
            Property = nameof(ElectronicDeviceType.CircuitBoard),
            Instructions =
            [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Insert circuit board {Product.CircuitBoard.Identity}" },
                new VisualInstruction() // Product image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/06392eda-f92b-4641-86d2-687603c654a7/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/06392eda-f92b-4641-86d2-687603c654a7/master/stream"
                }
            ],
            ExecutionTimeSec = 6
        };


        var circuitBoardAssemblyStep = new AssemblyTask();
        workplan.AddStep(circuitBoardAssemblyStep, circuitBoardAssemblyParameters, housingAssembledConnection, [circuitBoardAssembledConnection, circuitBoardAssembledConnection]);

        var solderedConnection = workplan.AddConnector("Soldered", NodeClassification.Intermediate);
        var solderingParameters = new SolderingParameters()
        {
            ManualSoldering =
            [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Solder joints manually" },
                new VisualInstruction() // Product picture with all soldering points
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/bf371525-56a2-4e1c-8e28-e16e73fa2f45/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/bf371525-56a2-4e1c-8e28-e16e73fa2f45/master/stream"
                },
                new VisualInstruction() // pdf with product details
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/c25c8af9-e148-4d30-a776-8b2e976f4193/master/stream"
                }
            ],
            FaultyContactInstructions =
            [
                new VisualInstruction() // Error 1 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/3c7d6df1-7835-4696-9fb7-3baecdc9b101/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/3c7d6df1-7835-4696-9fb7-3baecdc9b101/master/stream"
                },
                new VisualInstruction() // Error 2 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/53a79fff-174f-480c-8ed8-db103c30ebc6/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/53a79fff-174f-480c-8ed8-db103c30ebc6/master/stream"
                },
                new VisualInstruction() // Error 3 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/9d447908-e869-4304-961c-3b8e83b86828/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/9d447908-e869-4304-961c-3b8e83b86828/master/stream"
                },
                new VisualInstruction() // Error 4 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/2abd933a-e6fa-4ea8-9551-65c8c45482c9/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/2abd933a-e6fa-4ea8-9551-65c8c45482c9/master/stream"
                },
                new VisualInstruction() // Error 5 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/1672a7fb-cba5-4ff7-93ee-8fe2685dcc08/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/1672a7fb-cba5-4ff7-93ee-8fe2685dcc08/master/stream"
                },
                new VisualInstruction() // Error 6 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/25766930-8e7f-4bc2-a163-faf2213680c2/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/25766930-8e7f-4bc2-a163-faf2213680c2/master/stream"
                }
            ]
        };
        var solderingStep = new SolderingTask() { Parameters = solderingParameters, Inputs = [circuitBoardAssembledConnection], Outputs = [solderedConnection, failed] };
        workplan.Add(solderingStep);

        var testedConnection = workplan.AddConnector("Tested", NodeClassification.Intermediate);
        var testingParameters = new TestParameters();
        var testingStep = new TestTask("Test-Product-1") { Parameters = testingParameters, Inputs = [solderedConnection], Outputs = [testedConnection, circuitBoardAssembledConnection] };
        workplan.Add(testingStep);


        var packingParameters = new PackingParameters();
        var packagingStep = new PackingTask();
        workplan.AddStep(packagingStep, packingParameters, [testedConnection], [end, end]);

        return workplan;
    }

    private static Workplan CreateTtWorkplan(DemoImportParameters parameters)
    {
        // Workplan
        var workplan = new Workplan()
        {
            Name = "Product-2 work plan production and packing (Imported)",
            State = WorkplanState.Released
        };

        // Boundaries
        var start = workplan.AddConnector("Start", NodeClassification.Start);
        var end = workplan.AddConnector("End", NodeClassification.End);
        var failed = workplan.AddConnector("Failed", NodeClassification.Failed);

        var housingAssembledConnection = workplan.AddConnector("HousingAssembled", NodeClassification.Intermediate);
        var housingAssemblyParameters = new AssemblyParameters()
        {
            Property = nameof(ElectronicDeviceType.Housing),
            Instructions =
            [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Insert housing {Product.Housing.Identity}" },
                new VisualInstruction() // Product image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/3c71867d-395f-482a-a960-f89dc19d29ff/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/3c71867d-395f-482a-a960-f89dc19d29ff/master/stream"
                }
            ],
            ExecutionTimeSec = 3
        };

        var housingAssemblyStep = new AssemblyTask
        {
            Parameters = housingAssemblyParameters,
            Inputs = [start],
            Outputs = [housingAssembledConnection, housingAssembledConnection]
        };
        workplan.Add(housingAssemblyStep);

        var circuitBoardAssembledConnection = workplan.AddConnector("CircuitBoardAssembled", NodeClassification.Intermediate);
        var circuitBoardAssemblyParameters = new AssemblyParameters()
        {
            Property = nameof(ElectronicDeviceType.CircuitBoard),
            Instructions =
            [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Insert circuit board {Product.CircuitBoard.Identity}" },
                new VisualInstruction() // Product image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/3c71867d-395f-482a-a960-f89dc19d29ff/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/3c71867d-395f-482a-a960-f89dc19d29ff/master/stream"
                }
            ],
            ExecutionTimeSec = 6
        };

        var circuitBoardAssemblyStep = new AssemblyTask
        {
            Parameters = circuitBoardAssemblyParameters,
            Inputs = [housingAssembledConnection],
            Outputs = [circuitBoardAssembledConnection, circuitBoardAssembledConnection]
        };

        var solderedConnection = workplan.AddConnector("Soldered", NodeClassification.Intermediate);
        var solderingParameters = new SolderingParameters()
        {
            ManualSoldering =
            [
                new VisualInstruction() { Type = InstructionContentType.Text, Content = "Solder solder joints manually" },
                new VisualInstruction() // Product picture with all soldering points
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/9b95bebd-3036-4c9c-a995-837a497dad88/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/9b95bebd-3036-4c9c-a995-837a497dad88/master/stream"
                },
                new VisualInstruction() // pdf with product details
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/c25c8af9-e148-4d30-a776-8b2e976f4193/master/stream"
                }
            ],
            FaultyContactInstructions =
            [
                new VisualInstruction() // Error 1 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/baff24f2-37a9-4187-bacf-65bb61190030/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/baff24f2-37a9-4187-bacf-65bb61190030/master/stream"
                },
                new VisualInstruction() // Error 2 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/e74dc085-239f-45d5-b72e-f2c7dff14bf6/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/e74dc085-239f-45d5-b72e-f2c7dff14bf6/master/stream"
                },
                new VisualInstruction() // Error 3 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/b6e8d63f-7f7d-4ee6-8641-4350f1c28a37/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/b6e8d63f-7f7d-4ee6-8641-4350f1c28a37/master/stream"
                },
                new VisualInstruction() // Error 4 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/44a08c12-7b44-4acd-a1b3-533b0ae13c06/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/44a08c12-7b44-4acd-a1b3-533b0ae13c06/master/stream"
                },
                new VisualInstruction() // Error 5 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/3c3909fb-d131-4ce1-b6a0-5bc9b4275a82/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/3c3909fb-d131-4ce1-b6a0-5bc9b4275a82/master/stream"
                },
                new VisualInstruction() // Error 6 image
                {
                    Type = InstructionContentType.Media,
                    Content = parameters.AppLink + "/api/moryx/media/6ade1562-dcaa-44f6-873f-6e03a03baf99/master/stream",
                    Preview = parameters.AppLink + "/api/moryx/media/6ade1562-dcaa-44f6-873f-6e03a03baf99/master/stream"
                }
            ]
        };
        var solderingStep = new SolderingTask() { Parameters = solderingParameters, Inputs = [circuitBoardAssembledConnection], Outputs = [solderedConnection, failed] };
        workplan.Add(solderingStep);

        var testedConnection = workplan.AddConnector("Tested", NodeClassification.Intermediate);
        var testingParameters = new TestParameters();
        var testingStep = new TestTask("Test Product-2") { Parameters = testingParameters, Inputs = [solderedConnection], Outputs = [testedConnection, circuitBoardAssembledConnection] };
        workplan.Add(testingStep);

        var packingParameters = new PackingParameters();
        var packagingStep = new PackingTask
        {
            Parameters = packingParameters,
            Inputs = [testedConnection],
            Outputs = [end, end]
        };

        return workplan;
    }
}
