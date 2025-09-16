// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel;

namespace Moryx.Resources.Demo.ResourcesInputs;

public class ManualSolderingCellInputs
{
    [DisplayName("Soldering Temperature")]
    public int SolderingTemperature { get; set; }

    [DisplayName("Removed additional material")]
    public bool RemovedAdditionalMaterial { get; set; }

    [DisplayName("Soldering Tip Used")]
    public SolderingTipEnum SolderingTipUsed { get; set; }
}

public enum SolderingTipEnum
{
    Round,
    Flat
}
