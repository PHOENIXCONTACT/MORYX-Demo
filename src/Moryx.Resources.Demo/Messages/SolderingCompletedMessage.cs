// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

namespace Moryx.Resources.Demo.Messages;

/// <summary>
/// Message specific to notify end of a Soldering activity
/// </summary>
public class SolderingCompletedMessage
{
    /// <summary>
    /// Result of the activity.
    /// Result that can be mapped to the enum activity result
    /// </summary>
    public int Result { get; set; }
}
