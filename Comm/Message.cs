using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CfxSocketServer.Comm;

public class Message : IMessage
{
    /// <inheritdoc cref="IMessage.SessionId"/>
    public Guid SessionId { get; set; }

    /// <inheritdoc cref="IMessage.ChannelId"/>
    public Guid ChannelId { get; set; }

    /// <inheritdoc cref="IMessage.Type"/>
    [JsonConverter(typeof(StringEnumConverter))]
    public MessageType Type { get; set; }

    /// <inheritdoc cref="IMessage.Command"/>
    [JsonConverter(typeof(StringEnumConverter))]
    public Command Command { get; set; }

    /// <inheritdoc cref="IMessage.Content"/>
    public string Content { get; set; }
}
