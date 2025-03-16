using CfxSocketServer.Channel;
using CfxSocketServer.Session;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;

namespace CfxSocketServer.Comm;

/// <summary>
/// Comm processing between server and clientd
/// </summary>
/// <param name="webSocketSessions"></param>
/// <param name="webSocketChannels"></param>
public class CommProcessor(ConcurrentDictionary<Guid, IWebSocketSession> webSocketSessions, WebSocketChannelCollection webSocketChannels)
{
    private JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    public void BroadcastSessions(Guid sessionId)
    {
        string content = JsonConvert.SerializeObject(webSocketSessions, jsonSerializerSettings); 
         
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
}
