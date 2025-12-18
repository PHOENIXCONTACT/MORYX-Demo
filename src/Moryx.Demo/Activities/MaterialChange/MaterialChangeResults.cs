// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.ControlSystem.VisualInstructions;
using Moryx.Demo.Properties;
using Moryx.VisualInstructions;
using System.ComponentModel.DataAnnotations;

namespace Moryx.Demo.Activities
{
    namespace Moryx.Demo.Activities.MaterialChange
    {
        public enum MaterialChangeResults
        {
            [Display(ResourceType = typeof(Strings), Name = nameof(Strings.COMPLETED)), EnumInstruction]
            Success,
            TechnicalError,
            [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FAILED)), EnumInstruction]
            Failed
        }
    }
}
