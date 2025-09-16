// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Moryx.AbstractionLayer;
using Moryx.Demo.Properties;
using System.ComponentModel.DataAnnotations;

namespace Moryx.Demo.Activities;

[Display(Name = "Test", Description = "Task which does something with a product")]
public class TestTask : TaskStep<TestActivity, TestParameters>
{
    public TestTask([Display(ResourceType = typeof(Strings), Name = nameof(Strings.TEST_DESCRIPTOR))] string descriptor) { }

    public TestTask() { }
}