using System.Net.Security;

namespace CfxSocketServer.Session;

/// <summary>
/// Interface for WebSocket session
/// </summary>
public interface IWebSocketSession
{
    /// <summary>
    /// Unique identifier for the session
    /// </summary>
    Guid Id { get; }

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
    /// Start listening for transmissions
    /// </summary>
    void Start();

    /// <summary>
    /// Send text payload
    /// </summary>
    /// <param name="payload"></param>
    Task WriteTextAsync(string payload);

    /// <summary>
    /// Send a binary payload
    /// </summary>
    /// <param name="payload"></param>
    Task WriteBinaryAsync(List<byte> payload);

    /// <summary>
    /// Send a conn closed payload
    /// </summary>
    /// <param name="payload"></param>
    Task SendConnClose(List<byte> payload);
}
