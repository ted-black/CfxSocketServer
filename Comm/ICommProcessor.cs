using CfxSocketServer.Comm.Server;

namespace CfxSocketServer.Comm;

public interface ICommProcessor
{
    /// <summary>
    /// Broadcast sessions
    /// </summary>
    /// <param name="sessionId"></param>
    void BroadcastSession(Guid sessionId);

    /// <summary>
    /// Process message
    /// </summary>
    /// <param name="onTransmissionArgs"></param>
    void ProcessMessage(OnTransmissionArgs onTransmissionArgs);
}
