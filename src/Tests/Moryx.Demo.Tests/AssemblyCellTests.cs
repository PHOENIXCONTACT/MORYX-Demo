// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moq;
using Moryx.AbstractionLayer.Activities;
using Moryx.AbstractionLayer.Drivers.Message;
using Moryx.AbstractionLayer.Processes;
using Moryx.ControlSystem.Activities;
using Moryx.ControlSystem.Cells;
using Moryx.Demo.Activities;
using Moryx.Resources.Demo;
using Moryx.Resources.Demo.Messages;
using Moryx.VisualInstructions;
using NUnit.Framework;

namespace Moryx.Demo.Tests;

[TestFixture]
public class AssemblyCellTests
{
    private const string CellName = "AssemblyCellUnderTest";

    private Mock<IMessageDriver> _driver = null!;
    private Mock<IVisualInstructor> _instructor = null!;
    private Mock<IVisualInstructor> _setupInstructor = null!;
    private AssemblyCell _cell = null!;

    [SetUp]
    public void SetUp()
    {
        _driver = new Mock<IMessageDriver>(MockBehavior.Strict);
        _driver.SetupGet(driver => driver.Name).Returns("AssemblyDriver");
        _driver.SetupGet(driver => driver.HasChannels).Returns(false);
        _driver.Setup(driver => driver.Send(It.IsAny<object>()));

        _instructor = new Mock<IVisualInstructor>(MockBehavior.Strict);
        _instructor
            .Setup(instructor => instructor.Execute(It.IsAny<ActiveInstruction>(), It.IsAny<Action<ActiveInstructionResponse>>()))
            .Returns(42);
        _instructor.Setup(instructor => instructor.Clear(It.IsAny<long>()));

        _setupInstructor = new Mock<IVisualInstructor>(MockBehavior.Strict);
        _setupInstructor
            .Setup(instructor => instructor.Execute(It.IsAny<ActiveInstruction>(), It.IsAny<Action<ActiveInstructionResponse>>()))
            .Returns(84);
        _setupInstructor.Setup(instructor => instructor.Clear(It.IsAny<long>()));

        _cell = new AssemblyCell
        {
            Name = CellName,
            Driver = _driver.Object,
            Instructor = _instructor.Object,
            SetupInstructor = _setupInstructor.Object
        };
    }

    [Test]
    public void StartActivity_WithAutomaticAssemblyActivity_SendsAssembleMessageAndSetsRunningSession()
    {
        var activity = new AssemblyActivity
        {
            Id = 123,
            Parameters = new AssemblyParameters()
        };
        var activityStart = CreateActivityStart(activity, ActivityClassification.Production);

        _cell.ManualMode = false;

        _cell.StartActivity(activityStart);

        _driver.Verify(
            driver => driver.Send(It.Is<AssembleProductMessage>(message => message.ActivityId == activity.Id)),
            Times.Once);
        _instructor.Verify(
            instructor => instructor.Execute(It.IsAny<ActiveInstruction>(), It.IsAny<Action<ActiveInstructionResponse>>()),
            Times.Never);
        Assert.That(_cell.CurrentSession, Is.SameAs(activityStart));
        Assert.That(_cell.CellState, Is.EqualTo("Running"));
    }

    [Test]
    public void StartActivity_WithAssemblyActivity_CallsDriver()
    {
        var activity = new AssemblyActivity
        {
            Id = 123,
            Parameters = new AssemblyParameters()
        };
        var activityStart = CreateActivityStart(activity, ActivityClassification.Production);

        _cell.ManualMode = false;

        _cell.StartActivity(activityStart);

        _driver.Verify(
            driver => driver.Send(It.Is<AssembleProductMessage>(message => message.ActivityId == activity.Id)),
            Times.Once);
        _instructor.Verify(
            instructor => instructor.Execute(It.IsAny<ActiveInstruction>(), It.IsAny<Action<ActiveInstructionResponse>>()),
            Times.Never);
        // Assert.Fail();
        Assert.That(_cell.CurrentSession, Is.SameAs(activityStart));
        Assert.That(_cell.CellState, Is.EqualTo("Running"));
    }

    [Test]
    public void StartActivity_WithManualAssemblyActivity_ExecutesVisualInstructionAndDoesNotSendDriverMessage()
    {
        var activityStart = CreateActivityStart(new AssemblyActivity
        {
            Id = 123,
            Parameters = new AssemblyParameters()
        }, ActivityClassification.Production);

        _cell.ManualMode = true;

        _cell.StartActivity(activityStart);

        _instructor.Verify(
            instructor => instructor.Execute(
                It.Is<ActiveInstruction>(instruction => instruction.Title == CellName),
                It.IsAny<Action<ActiveInstructionResponse>>()),
            Times.Once);
        _driver.Verify(driver => driver.Send(It.IsAny<AssembleProductMessage>()), Times.Never);
        Assert.That(_cell.CurrentSession, Is.SameAs(activityStart));
        Assert.That(_cell.CellState, Is.EqualTo("Running"));
    }

    [Test]
    public void StartActivity_WithMaterialChangeActivity_UsesSetupInstructorWhenAvailable()
    {
        var activityStart = CreateActivityStart(new MaterialChangeActivity
        {
            Parameters = new MaterialChangeParameters
            {
                Instructions = []
            }
        }, ActivityClassification.Setup);

        _cell.StartActivity(activityStart);

        _setupInstructor.Verify(
            instructor => instructor.Execute(
                It.Is<ActiveInstruction>(instruction => instruction.Title == CellName),
                It.IsAny<Action<ActiveInstructionResponse>>()),
            Times.Once);
        _instructor.Verify(
            instructor => instructor.Execute(It.IsAny<ActiveInstruction>(), It.IsAny<Action<ActiveInstructionResponse>>()),
            Times.Never);
        _driver.Verify(driver => driver.Send(It.IsAny<object>()), Times.Never);
        Assert.That(_cell.CurrentSession, Is.SameAs(activityStart));
        Assert.That(_cell.CellState, Is.EqualTo("Running"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void StartActivity_WithMaterialReservationActivity_UpdatesReservationsAndPublishesCompletion(bool reserve)
    {
        const string order = "Order-4711";
        if (!reserve)
        {
            _cell.Reservations.Add(order);
        }

        var activity = new MaterialReservationActivity
        {
            Parameters = new MaterialReservationParameters
            {
                Reserve = reserve,
                Order = order
            }
        };
        var activityStart = CreateActivityStart(activity, ActivityClassification.Setup);
        ActivityCompleted? completed = null;
        _cell.ActivityCompleted += (_, result) => completed = result;

        _cell.StartActivity(activityStart);

        Assert.That(_cell.Reservations.Contains(order), Is.EqualTo(reserve));
        Assert.That(completed, Is.Not.Null);
        Assert.That(completed!.CompletedActivity, Is.SameAs(activity));
        Assert.That(completed.CompletedActivity.Result.Numeric, Is.EqualTo(0));
        Assert.That(_cell.CellState, Is.EqualTo("Running"), "The reservation branch publishes the result without replacing the current session.");
    }

    [Test]
    public void ProcessAborting_WithRunningManualInstruction_ClearsInstructionAndPublishesFailedActivityCompletion()
    {
        var activity = new AssemblyActivity
        {
            Id = 123,
            Parameters = new AssemblyParameters()
        };
        var activityStart = CreateActivityStart(activity, ActivityClassification.Production);
        ActivityCompleted? completed = null;
        _cell.ActivityCompleted += (_, result) => completed = result;

        _cell.ManualMode = true;
        _cell.StartActivity(activityStart);

        _cell.ProcessAborting(activity);

        _instructor.Verify(instructor => instructor.Clear(42), Times.Once);
        Assert.That(completed, Is.Not.Null);
        Assert.That(completed!.CompletedActivity, Is.SameAs(activity));
        Assert.That(completed.CompletedActivity.Result.Success, Is.False);
        Assert.That(_cell.CurrentSession, Is.SameAs(completed));
        Assert.That(_cell.CellState, Is.EqualTo("Idle"));
    }

    [Test]
    public void SequenceCompleted_WithProductionSequence_ReleasesWorkpieceAndSetsIdleState()
    {
        var activityStart = CreateActivityStart(new AssemblyActivity
        {
            Id = 123,
            Parameters = new AssemblyParameters()
        }, ActivityClassification.Production);
        _cell.StartActivity(activityStart);
        var sequenceCompleted = CreateSequenceCompleted(ActivityClassification.Production);

        _cell.SequenceCompleted(sequenceCompleted);

        _driver.Verify(driver => driver.Send(It.IsAny<ReleaseWorkpieceMessage>()), Times.Once);
        Assert.That(_cell.CurrentSession, Is.Null);
        Assert.That(_cell.CellState, Is.EqualTo("Idle"));
    }

    [Test]
    public void SequenceCompleted_WithSetupSequence_DoesNotReleaseWorkpieceOrChangeRunningSession()
    {
        var activityStart = CreateActivityStart(new MaterialChangeActivity
        {
            Parameters = new MaterialChangeParameters
            {
                Instructions = []
            }
        }, ActivityClassification.Setup);
        _cell.StartActivity(activityStart);
        var sequenceCompleted = CreateSequenceCompleted(ActivityClassification.Setup);

        _cell.SequenceCompleted(sequenceCompleted);

        _driver.Verify(driver => driver.Send(It.IsAny<ReleaseWorkpieceMessage>()), Times.Never);
        Assert.That(_cell.CurrentSession, Is.SameAs(activityStart));
        Assert.That(_cell.CellState, Is.EqualTo("Running"));
    }

    private static ActivityStart CreateActivityStart(Activity activity, ActivityClassification classification)
    {
        activity.Process ??= new Process { Id = 1 };

        var readyToWork = Session.StartSession(classification, ReadyToWorkType.Pull, activity.Process.Id);
        return readyToWork.StartActivity(activity);
    }

    private static SequenceCompleted CreateSequenceCompleted(ActivityClassification classification)
    {
        var readyToWork = Session.StartSession(classification, ReadyToWorkType.Pull);
        return readyToWork.CompleteSequence(null!, processActive: false);
    }
}
