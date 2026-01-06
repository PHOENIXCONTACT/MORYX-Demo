// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.AbstractionLayer.Resources;
using Moryx.ControlSystem.Activities;
using Moryx.ControlSystem.Capabilities;
using Moryx.ControlSystem.Cells;
using Moryx.ControlSystem.VisualInstructions;
using Moryx.Factory;
using Moryx.Operators;
using Moryx.Operators.Attendances;
using Moryx.Resources.Demo.Messages;
using Moryx.Resources.Demo.SimulatedDriver;
using Moryx.Serialization;
using Moryx.VisualInstructions;

namespace Moryx.Resources.Demo;

[ResourceRegistration]
[Display(Name = "Process Fix-Up Cell", Description = "Provides 'ProcessFixupCapabilities' to clean virtually mounted processes.")]
public class FixUpCell : DemoCellBase, IOperatorAssignable// ToDo: Fix issue in Factory Monitor module, IMachineLocation
{
    private bool _manualMode;
    [EntrySerialize, DataMember]
    [Display(Name = "Manual Mode", Description = "When active, prompts worker instructions to confirm process " +
        "fix up before releasing the virtually mounted process.")]
    public bool ManualMode
    {
        get => _manualMode; set
        {
            if (_manualMode == value)
            {
                return;
            }

            _manualMode = value;
            ClearInstructions();
        }
    }

    private long _currentInstruction;
    [ResourceReference(ResourceRelationType.Extension)]
    [Display(Name = "Visual Instructor", Description = "The visual instructor used to display instructions when set to manual mode.")]
    public IVisualInstructor Instructor { get; set; }

    [ResourceConstructor]
    [Display(Name = "Construct with simulated driver", Description = "Creates the cell and an " +
        "attached simulated driver. Also tries to reference a visual instructor.")]
    public void Construct() => Construct(Graph.GetResources<IVisualInstructor>().FirstOrDefault());

    public void Construct(IVisualInstructor instructor)
    {
        // Create Driver
        var driver = Graph.Instantiate<SimulatedFixUpDriver>();
        driver.Name = "Fix-Up Cell Driver";
        driver.Description = "Simulated driver to route relevant processes into the parent fix-up cell.";
        Driver = driver;
        Children.Add(driver);

        // Reference Instructor
        Instructor = instructor;
    }

    #region Lifecycle

    protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        await base.OnInitializeAsync(cancellationToken);
        UpdateCapabilities();
        Driver.Received += OnMessageReceived;
    }

    protected override void UpdateCapabilities() => Capabilities = _disabled
        ? Capabilities = NullCapabilities.Instance
        : new ProcessFixupCapabilities();

    private void OnMessageReceived(object sender, object message)
    {
        if (message is not WorkpieceArrivedMessage arrived)
        {
            return;
        }

        CurrentSession = Session.StartSession(ActivityClassification.Production, ReadyToWorkType.Pull, arrived.ProcessId);
        PublishReadyToWork(CurrentSession as ReadyToWork);
    }

    protected override void OnDispose()
    {
        Driver.Received -= OnMessageReceived;
        base.OnDispose();
    }

    #endregion

    #region Session Handling

    public override void StartActivity(ActivityStart activityStart)
    {
        CurrentSession = activityStart;
        if (_manualMode && Instructor is not null)
        {
            _currentInstruction = Instructor.Execute(new ActiveInstruction()
            {
                Title = Name,
                Results = [new InstructionResult() { Key = "Release", DisplayValue = "Release" }],
                Instructions = [$"Release process {CurrentSession.Process} to complete fix up activity".AsInstruction()]
            }, CompleteActivity);
        }
        else
        {
            CompleteActivity(null);
        }
    }

    private void CompleteActivity(ActiveInstructionResponse _)
    {
        if (CurrentSession is not ActivityStart activityStart)
        {
            throw new InvalidOperationException();
        }

        CurrentSession = activityStart.CreateResult(0);
        PublishActivityCompleted(CurrentSession as ActivityCompleted);
    }

    public override void ProcessAborting(
Activity affectedActivity)
    {
        base.ProcessAborting(affectedActivity);
        ClearInstructions();
    }

    private void ClearInstructions()
    {
        if (_currentInstruction == 0 || Instructor is null)
        {
            return;
        }

        Instructor.Clear(_currentInstruction);
        _currentInstruction = 0;
        CompleteActivity(null);
    }

    #endregion

    #region IOperatorAssignable

    public ICapabilities RequiredSkills => Capabilities;

    // Whenever someone is working the fix up cell, it switches to manual mode
    public void AttendanceChanged(IReadOnlyList<AttendanceChangedArgs> attandances) => ManualMode = attandances.Any();

    protected override IEnumerable<Session> ProcessEngineAttached()
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<Session> ProcessEngineDetached()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IMachineLocation

    /// <inheritdoc/>
    public IResource Machine => this;

    /// <inheritdoc/>
    public string SpecificIcon { get => "reset_wrench"; set { } }

    /// <inheritdoc/>
    public string Image { get; set; }

    /// <inheritdoc/>
    public Position Position { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ITransportPath> Origins => [];

    /// <inheritdoc/>
    public IEnumerable<ITransportPath> Destinations => [];

    #endregion
}
