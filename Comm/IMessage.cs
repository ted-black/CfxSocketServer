namespace CfxSocketServer.Comm;

/// <summary>
/// Interface for message object
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Sesion id
    /// </summary>
    Guid SessionId { get; set; }

    /// <summary>
    /// Message type
    /// </summary>
    MessageType Type { get; set; }

    /// <summary>
    /// Command
    /// </summary>
    Command Command { get; set; }

    /// <summary>
    /// Content
    /// </summary>
    string Content { get; set; }
}
