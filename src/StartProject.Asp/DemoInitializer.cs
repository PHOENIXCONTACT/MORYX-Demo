// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Moryx.AbstractionLayer.Resources;
using Moryx.Container;
using Moryx.ControlSystem.Processes;
using Moryx.Factory;
using Moryx.Resources.AssemblyInstruction;
using Moryx.Resources.Demo;
using Moryx.Resources.Demo.SimulatedDriver;

namespace StartProject.Asp;

[Plugin(LifeCycle.Transient, typeof(IResourceInitializer), Name = nameof(DemoInitializer))]
[Display(Name = "Resource Initializer", Description = "Creates a full set of resources to run the simulated production of electronic goods.")]
public class DemoInitializer : ResourceInitializerBase
{
    public override string Name => nameof(DemoInitializer);

    public override string Description => "Setup the simulation resources for the MORYX Demo";

    public override IReadOnlyList<Resource> Execute(IResourceGraph graph)
    {
        // Visual instructions
        var instructor = graph.Instantiate<VisualInstructor>();
        instructor.Name = "Visual Instructor";
        instructor.Description = "A shared visual instructor for the whole factory";

        var fixUpPlace = graph.Instantiate<FixUpCell>();
        fixUpPlace.Name = "Repair Station";
        fixUpPlace.Description = "Shared repair station for processes requiring fix up";
        fixUpPlace.Position = new() { PositionX = 0.8, PositionY = 0.8 };
        fixUpPlace.Construct(instructor);

        var buildingALoc = graph.Instantiate<MachineLocation>();
        buildingALoc.Name = "Building A location";
        buildingALoc.SpecificIcon = "home";
        buildingALoc.PositionX = 0.5;
        buildingALoc.PositionY = 0.5;

        var buildingA = graph.Instantiate<ManufacturingFactory>();
        buildingA.Name = "Building-A";
        buildingA.BackgroundUrl = "/api/moryx/media/c3ac0547-bb43-4e85-8805-242bf19cec31/master/stream";
        buildingALoc.Children.Add(buildingA);

        var buildingBLoc = graph.Instantiate<MachineLocation>();
        buildingBLoc.Name = "Building B location";
        buildingBLoc.SpecificIcon = "settings";
        buildingBLoc.PositionX = 0.7;
        buildingBLoc.PositionY = 0.7;

        var buildingB = graph.Instantiate<ManufacturingFactory>();
        buildingB.Name = "Building-B";
        buildingB.BackgroundUrl = "/api/moryx/media/c3ac0547-bb43-4e85-8805-242bf19cec31/master/stream";
        buildingBLoc.Children.Add(buildingB);

        // Assembly group
        var machineGroup = graph.Instantiate<MachineGroup>();
        machineGroup.Name = "Assembly";
        machineGroup.Description = "Assembly Workshop";

        //// Assembly group
        var driversSimulatedGroup = graph.Instantiate<MachineGroup>();
        driversSimulatedGroup.Name = "Simulated Drivers";
        driversSimulatedGroup.Description = "Simulated Drivers for simulation purposes";

        double positionX = 0.10, positionY = 0.10;
        for (var i = 1; i <= 6; i++)
        {
            var machineLocation = graph.Instantiate<MachineLocation>();
            machineLocation.Name = $"A-{i}";
            machineLocation.Description = $"Assembly Workplace {i}";
            machineLocation.PositionX = positionX;
            machineLocation.PositionY = positionY;
            machineLocation.SpecificIcon = "precision_manufacturing";
            positionX += 0.10;
            machineGroup.Children.Add(machineLocation);

            // Simulated Assembly driver
            var simulatedAssemblyDriver = graph.Instantiate<SimulatedAssemblyDriver>();
            simulatedAssemblyDriver.Name = $"Assembly Workplace-{i} Simulated driver";
            driversSimulatedGroup.Children.Add(simulatedAssemblyDriver);

            var machine = graph.Instantiate<AssemblyCell>();
            machine.Name = $"Assembly-{i}";
            machine.Instructor = instructor;
            machine.Driver = simulatedAssemblyDriver;
            machineLocation.Children.Add(machine);

            // todo: just for the first test cases. Remove it later
            switch (i)
            {
                case 1:
                    machine.MaterialIdentifier = "0188732";
                    break;
                case 2:
                    machine.MaterialIdentifier = "1312725";
                    break;
                case 3:
                    machine.MaterialIdentifier = "0031465";
                    break;
                case 4:
                    machine.MaterialIdentifier = "9660048";
                    break;
            }
            //----------------------------
        }
        buildingA.Children.Add(machineGroup);

        // Solder group
        machineGroup = graph.Instantiate<MachineGroup>();
        machineGroup.Name = "Soldering";
        machineGroup.Description = "Automatic and manual soldering stations";
        positionX = 0.25;
        positionY = 0.35;
        for (var i = 1; i <= 3; i++)
        {
            var machineLocation = graph.Instantiate<MachineLocation>();
            machineLocation.Name = $"S-{i}";
            machineLocation.Description = $"Soldering Workplace {i}";
            machineLocation.PositionX = positionX;
            positionX += 0.10;
            machineLocation.PositionY = positionY;
            machineLocation.SpecificIcon = "border_color";
            machineGroup.Children.Add(machineLocation);

            if (i < 3)
            {
                // Simulated driver
                var simulatedDriver = graph.Instantiate<SimulatedSolderingDriver>();
                simulatedDriver.Name = $"Soldering Machine-{i} Simulated driver";
                driversSimulatedGroup.Children.Add(simulatedDriver);

                var machine = graph.Instantiate<SolderingCell>();
                machine.Name = $"Soldering Machine-{i}";
                machine.Driver = simulatedDriver;
                machine.Hysteresis = 10;
                machine.NominalPowerThreshold = 270;
                machine.Instructor = instructor;
                machine.SolderingTemperature = 180 + i * 20;
                machineLocation.Children.Add(machine);
            }
            else
            {
                // Simulated driver
                var simulatedDriver = graph.Instantiate<SimulatedManualSolderingDriver>();
                simulatedDriver.Name = $"Manual Soldering Workplace Simulated driver";
                driversSimulatedGroup.Children.Add(simulatedDriver);

                var workspace = graph.Instantiate<ProcessHolderGroup>();
                workspace.Name = "Manual Soldering Workspace";
                var workspaceSlot1 = graph.Instantiate<ProcessHolderPosition>();
                workspaceSlot1.Name = "Slot 1";
                workspace.Positions.Add(workspaceSlot1);
                var workspaceSlot2 = graph.Instantiate<ProcessHolderPosition>();
                workspaceSlot2.Name = "Slot 2";
                workspace.Positions.Add(workspaceSlot2);

                var machine = graph.Instantiate<ManualSolderingCell>();
                machine.Name = "Manual Soldering Workplace";
                machine.Instructor = instructor;
                machine.Driver = simulatedDriver;
                machine.Workspace = workspace;
                //machine.Children.Add(workspace);
                machine.SolderingIronTemperature = 190;
                machineLocation.Children.Add(machine);
            }
        }
        buildingA.Children.Add(machineGroup);

        // Test group
        machineGroup = graph.Instantiate<MachineGroup>();
        machineGroup.Name = "Test";
        machineGroup.Description = "Test Stations";
        positionX = 0.30;
        positionY = 0.60;
        for (var i = 1; i <= 2; i++)
        {
            var machineLocation = graph.Instantiate<MachineLocation>();
            machineLocation.Name = $"T-{i}";
            machineLocation.Description = $"Test Station {i}";
            machineLocation.PositionX = positionX;
            positionX += 0.10;
            machineLocation.PositionY = positionY;
            machineLocation.SpecificIcon = "science";
            machineGroup.Children.Add(machineLocation);

            var simulatedDriver = graph.Instantiate<SimulatedTestDriver>();
            simulatedDriver.Name = $"Testing Station-{i} Simulated driver";
            driversSimulatedGroup.Children.Add(simulatedDriver);

            var machine = graph.Instantiate<TestSimulationCell>();
            machine.Name = $"Testing Station-{i}";
            machine.SuccessRate = 100;
            machine.Voltage = 12;
            machine.Driver = simulatedDriver;
            machineLocation.Children.Add(machine);

        }
        buildingB.Children.Add(machineGroup);

        //Packing group
        machineGroup = graph.Instantiate<MachineGroup>();
        machineGroup.Name = "Packaging";
        machineGroup.Description = "Packaging workplace for the comissioning of produced parts.";

        var packLocation = graph.Instantiate<MachineLocation>();
        packLocation.PositionX = 0.35;
        packLocation.PositionY = 0.85;
        packLocation.Name = "P-1";
        packLocation.Description = $"Packaging Workplace-1";
        packLocation.SpecificIcon = $"pallet";
        machineGroup.Children.Add(packLocation);

        var simulatedPackagingDriver = graph.Instantiate<SimulatedPackagingDriver>();
        simulatedPackagingDriver.Name = $"Packing Station-1 Simulated driver";
        driversSimulatedGroup.Children.Add(simulatedPackagingDriver);

        var packingCell = graph.Instantiate<PackagingCell>();
        packingCell.Name = "Packaging Station-1";
        packingCell.Instructor = instructor;
        packingCell.MaintenanceInstruction = "/api/moryx/media/c25c8af9-e148-4d30-a776-8b2e976f4193/master/stream";
        packingCell.MaintenanceThreshold = 100;
        packingCell.PackagingAmount = 10;
        packingCell.Driver = simulatedPackagingDriver;
        packLocation.Children.Add(packingCell);

        buildingB.Children.Add(machineGroup);
        buildingB.Children.Add(driversSimulatedGroup);

        var factory = graph.Instantiate<ManufacturingFactory>();
        factory.Name = "Simulated Factory";
        factory.BackgroundUrl = "/api/moryx/media/4e840e76-6f49-4cc8-aea1-bf000805546d/master/stream";
        factory.Children.Add(instructor);
        factory.Children.Add(fixUpPlace);
        factory.Children.Add(buildingALoc);
        factory.Children.Add(buildingBLoc);

        return [factory];
    }
}
