namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Op codes for web socket per rfc
/// </summary>
public enum OpCode : byte { 
    ContinuationFrame = 0x0, 
    Text = 0x1, 
    Binary = 0x2, 
    ConnClosed = 0x8, 
    Ping = 0x9, 
    Pong = 0xA 
};
