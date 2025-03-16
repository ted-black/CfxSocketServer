using CfxSocketServer.Comm.Server;

namespace CfxSocketServer.Session;

public class OnWebSocketSessionCloseEventArgs : EventArgs
{
    /// <summary>
    /// On close op code
    /// </summary>
    public Transmission Transmission { get; set;}

    /// <summary>
    /// WebSocket session id
    /// </summary>
    public Guid WebSocketSessionId { get; set; }

    /// <summary>
    /// WebSocket session name
    /// </summary>
    public string WebSocketSessionName { get; set; }
}
