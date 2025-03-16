namespace CfxSocketServer.Comm;

/// <summary>
/// Command of message
/// </summary>
public enum Command
{
    CreateChannel = 1,
    AddToChannel = 2,
    RemoveFromChannel = 3,
    ListChannels = 4,
    ListSessions = 5
}
