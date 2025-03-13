using CfxSocketServer.Session.SessionSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CfxSocketServer.Session;

public class WebSocketSessionManager
{
    private ConcurrentDictionary<Guid, IWebSocketSession> webSocketSessions = new();

    /// <inheritdoc cref="IWebSocketSessionManager.StartSession"/>
    public void StartSession()
    {
        WebSocketListener webSocketListener = new();
        webSocketListener.OnWebSocketSessionOpen += WebSocketSessionOpen;
        webSocketListener.Start();
    }

    private void WebSocketSessionOpen(object sender, EventArgs e)
    {
        OnWebSocketSessionOpenEventArgs onWebSocketSessionOpenEventArgs = (OnWebSocketSessionOpenEventArgs)e;
        IWebSocketSession webSocketSession = onWebSocketSessionOpenEventArgs.WebSocketSession;

        webSocketSession.OnClose += WebSocketSessionClose;
        webSocketSession.OnMessage += WebSocketSessionMessage;
        webSocketSession.OnPing += WebSocketSessionPing;

        webSocketSession.Start();

        webSocketSessions.TryAdd(webSocketSession.Id, webSocketSession);
    }

    private void WebSocketSessionPing(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void WebSocketSessionMessage(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void WebSocketSessionClose(object sender, EventArgs e)
    {
        OnWebSocketSessionCloseEventArgs onWebSocketSessionCloseEventArgs = (OnWebSocketSessionCloseEventArgs)e;
        webSocketSessions.TryRemove(onWebSocketSessionCloseEventArgs.WebSocketSessionId, out _);
    }
}
