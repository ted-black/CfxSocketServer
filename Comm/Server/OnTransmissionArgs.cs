using System;

namespace CfxSocketServer.Comm.Server;

/// <summary>
/// Event args for on message event
/// </summary>
public class OnTransmissionArgs: EventArgs
{
    /// <summary>
    /// The message
    /// </summary>
    public Transmission Transmission { get; set; }

    /// <summary>
    /// WebSocket session id
    /// </summary>
    public Guid WebSocketSessionId { get; set; }

    /// <summary>
    /// WebSocket session name
    /// </summary>
    public string WebSocketSessionName { get; set; }
}
