// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Properties;
using System.ComponentModel.DataAnnotations;

namespace Moryx.Demo.Activities;

public enum SolderingResults
{
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.COMPLETED)), EnumInstruction]
    Completed,

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FAILED)), EnumInstruction]
    Failed
}
