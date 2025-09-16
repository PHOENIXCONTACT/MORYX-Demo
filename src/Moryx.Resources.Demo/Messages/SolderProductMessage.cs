// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

namespace Moryx.Resources.Demo.Messages;

/// <summary>
/// Message specific to start a soldering activity
/// </summary>

public class SolderProductMessage
{
    /// <summary>
    /// Id of the Soldering activity
    /// </summary>
    public long ActivityId { get; set; }
}
