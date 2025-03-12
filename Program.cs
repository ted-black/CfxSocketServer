using CfxSocketServer.Session;

namespace CfxSocketServer;

internal class Program
{
    static void Main(string[] args)
    {
        WebSocketSessionManager webSocketSessionManager = new();
        webSocketSessionManager.StartSession();
    }
}
