namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Frame header interface per RFC
/// </summary>
interface IFrameHeader
{
    bool IsFinal { get;}
    OpCode? OpCode { get; }
    bool IsMask { get; }
    ulong PayloadLength { get; }
    byte[] Mask { get; }
}
