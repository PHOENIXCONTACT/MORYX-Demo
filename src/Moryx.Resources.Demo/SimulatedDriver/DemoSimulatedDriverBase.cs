// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Drivers;
using Moryx.AbstractionLayer.Drivers.Message;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.Simulation;
using Moryx.ProcessData;
using Moryx.StateMachines;
using Moryx.Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Moryx.Resources.Demo.SimulatedDriver;

[ResourceRegistration, ResourceAvailableAs(typeof(IProcessDataPublisher))]
public abstract class DemoSimulatedDriverBase : Driver, IMessageDriver, ISimulationDriver
{
    public event EventHandler<SimulationState> SimulatedStateChanged;

    public bool HasChannels => false;
    public IDriver Driver => this;
    public string Identifier => Name;

    public abstract void Send(object payload);

    protected async Task OnStartAsync()
    {
        await OnStartAsync();

        SimulatedState = SimulationState.Idle;
    }

    /// <summary>
    /// Injected
    /// </summary>
    public IParallelOperations ParallelOperations { get; set; }

    private SimulationState _simulatedState;
    public SimulationState SimulatedState
    {
        get => _simulatedState;
        protected set
        {
            _simulatedState = value;
            SimulatedStateChanged?.Invoke(this, value);
        }
    }

    public abstract IEnumerable<ICell> Usages { get; }

    public Task SendAsync(object payload)
    {
        return Task.CompletedTask;
    }

    public abstract void Ready(Activity activity);
    public abstract void Ready(long processId);
    public abstract void Result(SimulationResult result);
    public IMessageChannel Channel<TChannel>(string identifier)
    {
        throw new NotImplementedException();
    }

    public IMessageChannel Channel<TSend, TReceive>(string identifier)
    {
        throw new NotImplementedException();
    }

    public IMessageChannel Channel(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task SendAsync(object payload, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public abstract event EventHandler<object> Received;
}

public class SimulatedDriverState : SyncDriverState<DemoSimulatedDriverBase>
{
    [StateDefinition(typeof(SimulatedDriverState), IsInitial = true)]
    public int InitialState = 0;

    public SimulatedDriverState(DemoSimulatedDriverBase context, StateMap stateMap) : base(context, stateMap, StateClassification.Offline)
    {
    }

    public void ForceState(StateClassification classification)
    {
        Classification = classification;
    }
}
