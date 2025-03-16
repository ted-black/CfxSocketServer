using CfxSocketServer.Channel;
using System.Net.Security;

namespace CfxSocketServer.Session;

/// <summary>
/// Interface for WebSocket session
/// </summary>
public interface IWebSocketSession : IChannelWriter, IDisposable
{
    /// <summary>
    /// Event handler for message recieved
    /// </summary>
    event EventHandler OnMessage;

    /// <summary>
    /// Event handler for close session event
    /// </summary>
    event EventHandler OnClose;

    /// <summary>
    /// Event handler to respond to ping
    /// </summary>
    event EventHandler OnPing;

    /// <summary>
    /// Query string parameters
    /// </summary>
    Dictionary<string, string> QueryStringParams { get; set; }

    /// <summary>
    /// Am i listening
    /// </summary>
    bool IsListen { get; set; }

    /// <summary>
    /// Start listening for transmissions
    /// </summary>
    void Start();

    /// <summary>
    /// Send a conn closed payload
    /// </summary>
    /// <param name="payload"></param>
    Task SendConnClose(List<byte> payload);
}
