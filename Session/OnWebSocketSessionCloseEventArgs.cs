using CfxSocketServer.Session.Comm;

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
}
