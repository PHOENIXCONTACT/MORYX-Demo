// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using Moryx.AbstractionLayer.Workplans;

namespace Moryx.Demo.Activities;

[Display(Name = "Assemble", Description = "Task which does something with a product")]
public class AssemblyTask : TaskStep<AssemblyActivity, AssemblyParameters>
{
}
