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
    private CommProcessor commProcessor;

    /// <inheritdoc cref="IWebSocketSessionManager.StartSession"/>
    public void StartSession()
    {
        WebSocketListener webSocketListener = new();
        webSocketListener.OnWebSocketSessionOpen += WebSocketSessionOpen;
        webSocketListener.Start();

        commProcessor = new CommProcessor(webSocketSessions, webSocketChannels);
    }

    private void WebSocketSessionOpen(object sender, EventArgs e)
    {
        OnWebSocketSessionOpenEventArgs onWebSocketSessionOpenEventArgs = (OnWebSocketSessionOpenEventArgs)e;
        WebSocketSession webSocketSession = onWebSocketSessionOpenEventArgs.WebSocketSession;

        webSocketSession.OnClose += WebSocketSessionClose;
        webSocketSession.OnMessage += WebSocketSessionMessage;
        webSocketSession.OnPing += WebSocketSessionPing;

        webSocketSession.Start();

        webSocketSessions.TryAdd(webSocketSession.Id, webSocketSession);

        Console.WriteLine($"Session opened, sessionId: {webSocketSession.Id}, sessionName: {webSocketSession.Name}");

        commProcessor.BroadcastSession(webSocketSession.Id);
    }

    private void WebSocketSessionPing(object sender, EventArgs e)
    {
    }

    private void WebSocketSessionMessage(object sender, EventArgs e)
    {
        OnTransmissionArgs onTransmissionArgs = (OnTransmissionArgs)e;
        commProcessor.ProcessMessage(onTransmissionArgs);
    }

    private void WebSocketSessionClose(object sender, EventArgs e)
    {
        OnWebSocketSessionCloseEventArgs onWebSocketSessionCloseEventArgs = (OnWebSocketSessionCloseEventArgs)e;
        webSocketSessions.TryRemove(onWebSocketSessionCloseEventArgs.WebSocketSessionId, out _);
        Console.WriteLine($"Session removed, sessionId: {onWebSocketSessionCloseEventArgs.WebSocketSessionId}");
    }
}
