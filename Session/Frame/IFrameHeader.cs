namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Frame header interface per RFC
/// </summary>
interface IFrameHeader
{
    /// <summary>
    /// Is final frame
    /// </summary>
    bool IsFinal { get;}

    /// <summary>
    /// The frame's op code
    /// </summary>
    OpCode? OpCode { get; }

    /// <summary>
    /// Is the data represented by the frame masked
    /// </summary>
    bool IsMask { get; }

    /// <summary>
    /// The size of the payload
    /// </summary>
    ulong PayloadLength { get; }

    /// <summary>
    /// What the mask is
    /// </summary>
    byte[] Mask { get; }
}
