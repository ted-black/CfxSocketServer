using CfxSocketServer.Channel;
using CfxSocketServer.Comm.Server;
using CfxSocketServer.Session;
using CfxSocketServer.Session.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

namespace CfxSocketServer.Comm;

/// <summary>
/// Comm processing between server and clientd
/// </summary>
/// <param name="webSocketSessions"></param>
/// <param name="webSocketChannels"></param>
public class CommProcessor(ConcurrentDictionary<Guid, WebSocketSession> webSocketSessions, WebSocketChannelCollection webSocketChannels) : ICommProcessor
{
    private readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    /// <inheritdoc cref="ICommProcessor.BroadcastSession(Guid)"/>
    public void BroadcastSession(Guid sessionId)
    {
        string content = JsonConvert.SerializeObject(webSocketSessions.Values.ToArray(), jsonSerializerSettings);

        Message message = new()
        {
            Command = Command.ListSessions,
            Type = MessageType.Command,
            Content = content,
            SessionId = sessionId
        };

        foreach (IWebSocketSession session in webSocketSessions.Values)
        {
            session.WriteTextAsync(JsonConvert.SerializeObject(message, jsonSerializerSettings)).Wait();
        }
    }

    /// <inheritdoc cref="ICommProcessor.ProcessMessage(OnTransmissionArgs)"/>
    public void ProcessMessage(OnTransmissionArgs onTransmissionArgs)
    {
        string messageString = ASCIIEncoding.UTF8.GetString(onTransmissionArgs.Transmission.Payload.ToArray());

        Message message = JsonConvert.DeserializeObject<Message>(messageString);

        switch (message.Type)
        {
            case MessageType.Command:
                ProcessCommand(message);
                break;
            case MessageType.Content:
                ProcessContent(message);
                break;
        }
    }

    private void ProcessContent(Message message)
    {
        IWebSocketChannel<IChannelWriter> channel = webSocketChannels.Get(message.ChannelId);
        if (channel != null)
        {
            Message contentMessage = new()
            {
                Type = MessageType.Content,
                ChannelId = message.ChannelId,
                Content = JsonConvert.SerializeObject(message.Content, jsonSerializerSettings)
            };
            channel.WriteTextAllAsync(JsonConvert.SerializeObject(contentMessage, jsonSerializerSettings)).Wait();
        }
    }

    private void ProcessCommand(Message message)
    {
        switch (message.Command)
        {
            case Command.CreateChannel:
                List<SessionInfo> sessions =JsonConvert.DeserializeObject<List<SessionInfo>>(message.Content);
                CreateChannel(sessions);
                break;
        }
    }

    /// <summary>
    /// Create the channel if is does NOT exist and broadcast if succesful
    /// </summary>
    /// <param name="sessions"></param>
    private void CreateChannel(List<SessionInfo> sessions)
    {
        WebSocketChannel<IChannelWriter> channel = new();

        foreach (SessionInfo session in sessions)
        {
            WebSocketSession webSocketSession = webSocketSessions.FirstOrDefault(s => s.Key == session.Id).Value;
            if (webSocketSession != null)
            {
                channel.Subscribe(webSocketSession);
            }
        }

        bool channelAdded = webSocketChannels.Add(channel);

        if (channelAdded)
        {
            BroadcastChannels(channel);
        }
    }

    private void BroadcastChannels(WebSocketChannel<IChannelWriter> channel)
    {
        List<WebSocketChannel<IChannelWriter>> channels = [];
        channels.Add(channel);
        string content = JsonConvert.SerializeObject(channels, jsonSerializerSettings);

        foreach (IWebSocketSession webSocketSession in channel.Subscribers.Values.Cast<IWebSocketSession>())
        {
            webSocketSession.ChannelId = channel.Id;
            Message message = new()
            {
                Command = Command.ListChannels,
                Type = MessageType.Command,
                Content = content,
                SessionId = webSocketSession.Id
            };

            webSocketSession.WriteTextAsync(JsonConvert.SerializeObject(message, jsonSerializerSettings));
        }
    }
}
