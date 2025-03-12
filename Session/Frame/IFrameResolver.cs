namespace CfxSocketServer.Session.Frame;

public interface IFrameResolver
{
    /// <summary>
    /// Continuation frame byte queue
    /// </summary>
    List<List<byte>> ContinuationFrameByteQueue { get; set; }

    /// <summary>
    /// The initial frame's op code in a sequence of continuation frames.
    /// </summary>
    OpCode? InitialFrameOpCode { get; set; }

    /// <summary>
    /// Resolve continuation frames
    /// </summary>
    /// <param name="frameHeader"></param>
    /// <param name="payloadByteList"></param>
    List<byte> ResolveFrame(FrameHeader frameHeader, List<byte> payloadByteList);

    /// <summary>
    /// Reset the resolver
    /// </summary>
    void Reset();
}
