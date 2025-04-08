using CfxSocketServer.Channel;
using CfxSocketServer.Comm;
using CfxSocketServer.Comm.Server;
using CfxSocketServer.Session.SessionSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CfxSocketServer.Session;

public class WebSocketSessionManager
{
    private readonly ConcurrentDictionary<Guid, WebSocketSession> webSocketSessions = new();
    private readonly WebSocketChannelCollection webSocketChannels = new();

    /// <inheritdoc cref="IWebSocketSessionManager.StartSession"/>
    public void StartSessionListener()
    {
        WebSocketListener webSocketListener = new();
        webSocketListener.OnWebSocketSessionOpen += WebSocketSessionOpen;
        webSocketListener.Start();
    }

    /// <summary>
    /// Open handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WebSocketSessionOpen(object sender, EventArgs e)
    {
        OnWebSocketSessionOpenEventArgs onWebSocketSessionOpenEventArgs = (OnWebSocketSessionOpenEventArgs)e;
        WebSocketSession webSocketSession = onWebSocketSessionOpenEventArgs.WebSocketSession;

        webSocketSession.OnClose += WebSocketSessionClose;
        webSocketSession.OnMessage += WebSocketSessionMessage;
        webSocketSession.OnPing += WebSocketSessionPing;

        webSocketSession.Start();

        webSocketSessions.TryAdd(webSocketSession.Id, webSocketSession);

        AssociateWithChannel(webSocketSession);

        Console.WriteLine($"Session opened, sessionId: {webSocketSession.Id}, sessionName: {webSocketSession.Name}");
        CommProcessor commProcessor = new(webSocketSessions, webSocketChannels);
        commProcessor.BroadcastSession(webSocketSession.Id);
    }

    /// <summary>
    /// Associate the session with the channel if previously subscribed
    /// </summary>
    /// <param name="webSocketSession"></param>
    private void AssociateWithChannel(WebSocketSession webSocketSession)
    {
        foreach (KeyValuePair<Guid, IWebSocketChannel<IChannelWriter>> channel in webSocketChannels.Channels)
        {
            if(channel.Value.Name.Contains(webSocketSession.Name))
            {
                bool isSubscribed = channel.Value.Subscribe(webSocketSession);
                if (isSubscribed)
                {
                    CommProcessor commProcessor = new(webSocketSessions, webSocketChannels);
                    commProcessor.BroadcastChannel(channel.Value as WebSocketChannel<IChannelWriter>);
                }
            }
        }
    }

    /// <summary>
    /// Ping handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WebSocketSessionPing(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// Message handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WebSocketSessionMessage(object sender, EventArgs e)
    {
        OnTransmissionArgs onTransmissionArgs = (OnTransmissionArgs)e;
        CommProcessor commProcessor = new(webSocketSessions, webSocketChannels);
        commProcessor.ProcessMessage(onTransmissionArgs);
    }

    /// <summary>
    /// Close handler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WebSocketSessionClose(object sender, EventArgs e)
    {
        OnWebSocketSessionCloseEventArgs onWebSocketSessionCloseEventArgs = (OnWebSocketSessionCloseEventArgs)e;
        webSocketSessions.TryRemove(onWebSocketSessionCloseEventArgs.WebSocketSessionId, out _);
        Console.WriteLine($"Session removed, sessionId: {onWebSocketSessionCloseEventArgs.WebSocketSessionId}");
    }
}
