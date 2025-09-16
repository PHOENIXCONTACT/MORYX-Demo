// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Recipes;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Products;
using Moryx.Serialization;

namespace Moryx.Demo.Activities;

public class SolderingParameters : VisualInstructionParameters, IConsultantValues
{
    public bool RepairSoldering { get; set; }

    public int ExecutionTimeSec { get; set; }

    [EntrySerialize, DataMember]
    public int Power { get; set; }

    [EntrySerialize, DataMember]
    public VisualInstruction[] ManualSoldering { get; set; }

    [EntrySerialize, DataMember]
    public VisualInstruction[] FaultyContactInstructions { get; set; }


    protected override void Populate(IProcess process, Parameters instance)
    {
        base.Populate(process, instance);

        var parameters = (SolderingParameters)instance;
        var product = (ElectronicDeviceType)((IProductRecipe)process.Recipe).Product;
        parameters.ExecutionTimeSec = (int)(product.CircuitBoard?.Product.SolderingPoints.Sum(sp => sp.Diameter) ?? 0);

        var instructions = new List<VisualInstruction>();
        var lastTest = process.GetActivity(ActivitySelectionType.LastOrDefault, a => a is TestActivity);
        if (lastTest == null && product.CircuitBoard != null)
        {
            instructions.AddRange(ManualSoldering);
            foreach (var solderingPoint in product.CircuitBoard.Product.SolderingPoints)
            {
                instructions.Add(new VisualInstruction
                {
                    Type = InstructionContentType.Text,
                    Content = $"Soldering Points: ({solderingPoint.PosX},{solderingPoint.PosY}) with {solderingPoint.Diameter}mm²"
                }); ;
            }
        }
        else if (lastTest != null)
        {
            instructions.Add(new VisualInstruction
            {
                Type = InstructionContentType.Text,
                Content = "Repair defective solder joints"
            });
            parameters.RepairSoldering = lastTest.Result.Numeric > 0;
            var faultyContacts = lastTest.Tracing.Transform<TestTracing>().FaultyContacts;
            if (faultyContacts.Length == 0)
            {
                instructions.Clear();
                instructions.Add(new VisualInstruction
                {
                    Type = InstructionContentType.Text,
                    Content = "Problem detected during soldering."
                });
                instructions.Add(new VisualInstruction
                {
                    Type = InstructionContentType.Text,
                    Content = "Please check entire article."
                });
            }
            else
            {
                foreach (var contact in faultyContacts)
                {
                    if (FaultyContactInstructions == null)
                    {
                        return;
                    }

                    var instruction = FaultyContactInstructions[contact - 1];
                    instructions.Add(instruction);
                }
            }
        }

        parameters.Instructions = [.. instructions];
    }
}