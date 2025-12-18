// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Activities;
using Moryx.Container;
using Moryx.ControlSystem.Cells;

namespace Moryx.ControlSystem.Demo.CellSelector;

[Plugin(LifeCycle.Transient, typeof(ICellSelector), Name = nameof(LoadBalancer))]
public class LoadBalancer : CellSelectorBase
{
    /// <summary>
    /// Keep track of the primary target of an activity
    /// </summary>
    private readonly Dictionary<IActivity, ICell> _primaryTarget = [];

    public IReadOnlyList<ICell> SelectCells(IActivity activity, IReadOnlyList<ICell> availableCells)
    {
        // Nothing to balance
        if (availableCells.Count <= 1)
        {
            return availableCells;
        }

        // First clear completed activities from our primary target tracking
        var cellLoad = availableCells.ToDictionary(c => c, c => 0);
        lock (_primaryTarget)
        {
            // Clean-up
            foreach (var key in _primaryTarget.Keys.ToList())
            {
                if (key.Result != null)
                {
                    _primaryTarget.Remove(key);
                }
            }

            // Count usages
            foreach (var cell in _primaryTarget.Values.Where(cellLoad.ContainsKey))
            {
                cellLoad[cell]++;
            }
        }

        // Sort by current load
        var loadBalanced = cellLoad.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToList();

        // Update primary target for this activity
        lock (_primaryTarget)
        {
            _primaryTarget[activity] = loadBalanced[0];
        }

        return loadBalanced;
    }

    public override Task<IReadOnlyList<ICell>> SelectCellsAsync(Activity activity, IReadOnlyList<ICell> availableCells, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
