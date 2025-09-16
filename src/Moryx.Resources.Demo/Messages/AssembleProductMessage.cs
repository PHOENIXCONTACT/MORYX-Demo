// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

namespace Moryx.Resources.Demo.Messages;

/// <summary>
/// Message specific to start an assembly activity
/// </summary>
public class AssembleProductMessage
{
    /// <summary>
    /// Id of the assembly activity
    /// </summary>
    public long ActivityId { get; set; }
}
