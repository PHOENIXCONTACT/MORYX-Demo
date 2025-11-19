// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Linq;
using Moryx.AbstractionLayer.Activities;
using Moryx.Container;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.Processes;
using Moryx.Demo.Activities;
using Moryx.Demo.Capabilities;

namespace Moryx.ControlSystem.Demo.CellSelector;

//[ExpectedConfig(typeof(SolderingOptimizerConfig))]
[Plugin(LifeCycle.Transient, typeof(ICellSelector), Name = nameof(SolderingOptimizer))]
public class SolderingOptimizer : CellSelectorBase //<SolderingOptimizerConfig>
{
    /// <summary>
    /// Access to currently running processes and activities
    /// </summary>
    public IActivityPool ActivityPool { get; set; }

    public override IReadOnlyList<ICell> SelectCells(IActivity activity, IReadOnlyList<ICell> availableCells)
    {
        var solderingActivity = activity as SolderingActivity;
        if (solderingActivity == null || availableCells.Count == 1)
        {
            return availableCells;
        }

        // Determine automatic cells
        var autoCells = availableCells.ToList();
        var manualCapa = new SolderingCapabilities { ManualSoldering = true };
        autoCells.RemoveAll(c => c.Capabilities.Provides(manualCapa));

        // Determine amount of open soldering activities
        var openSoldering = ActivityPool.GetByCondition(a => a is SolderingActivity).Count;
        // Use only auto cells as long as activities per cell does not exceed threshold
        return openSoldering >= 3 /*Config.SolderingCountThreshold */ * autoCells.Count
            ? availableCells
            : autoCells;
    }
}
