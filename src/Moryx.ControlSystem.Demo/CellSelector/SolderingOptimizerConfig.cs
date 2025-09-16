// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel;
using System.Runtime.Serialization;
using Moryx.ControlSystem.Cells;

namespace Moryx.ControlSystem.Demo.CellSelector;

public class SolderingOptimizerConfig : CellSelectorConfig
{
    public override string PluginName
    {
        get => nameof(SolderingOptimizer);
        set { }
    }

    [DataMember, DefaultValue(3)]
    [Description("Threshold of open soldering activities per cell")]
    public int SolderingCountThreshold { get; set; }
}