using System;

namespace CfxSocketServer.Session.Comm;

/// <summary>
/// Event args for on message event
/// </summary>
public class OnTransmissionArgs: EventArgs
{
    /// <summary>
    /// The message
    /// </summary>
    public Transmission Transmission { get; set; }
}
