using CfxSocketServer.Session.Comm;

namespace CfxSocketServer.Session;

public class OnWebSocketSessionPingEventArgs : EventArgs
{
    /// <summary>
    /// The message
    /// </summary>
    public Transmission Transmission { get; set; }
}
