// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

namespace Moryx.Resources.Demo.Messages;

/// <summary>
/// Message specific to notify that the workpiece has arrived to the cell
/// </summary>
public class WorkpieceArrivedMessage
{
    /// <summary>
    /// Id of the process
    /// </summary>
    public long ProcessId { get; set; }
}
