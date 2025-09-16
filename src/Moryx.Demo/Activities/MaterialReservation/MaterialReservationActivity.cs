// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.AbstractionLayer.Capabilities;
using Moryx.ControlSystem.Activities;
using Moryx.Demo.Capabilities;

namespace Moryx.Demo.Activities;

[ActivityResults(typeof(DefaultActivityResult))]
public class MaterialReservationActivity : Activity<MaterialReservationParameters, EnergyTracing>, IControlSystemActivity
{
    public ActivityClassification Classification => ActivityClassification.Setup;

    public override ProcessRequirement ProcessRequirement => ProcessRequirement.NotRequired;

    public override ICapabilities RequiredCapabilities => new AssemblyCapabilities
    {
        EquippedMaterial = Parameters.Material,
        Reservations = Parameters.Reserve ? null : new[] { Parameters.Order }
    };

    protected override ActivityResult CreateResult(long resultNumber)
    {
        return ActivityResult.Create((DefaultActivityResult)resultNumber);
    }

    protected override ActivityResult CreateFailureResult()
    {
        return ActivityResult.Create(DefaultActivityResult.Failed);
    }

}