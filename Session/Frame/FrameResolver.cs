namespace CfxSocketServer.Session.Frame;

public class FrameResolver : IFrameResolver
{
    /// <summary>
    /// Continuation frame byte queue
    /// </summary>
    public List<List<byte>> ContinuationFrameByteQueue { get; set; }

    /// <summary>
    /// The initial frame's op code in a sequence of continuation frames.
    /// </summary>
    public OpCode? InitialFrameOpCode { get; set; }

    /// <summary>
    /// Resolve continuation frames
    /// </summary>
    /// <param name="frameHeader"></param>
    /// <param name="payloadByteList"></param>
    /// <returns></returns>
    public List<byte> ResolveFrame(FrameHeader frameHeader, List<byte> payloadByteList)
    {
        if (!frameHeader.IsFinal)
        {
            if (InitialFrameOpCode == null)
            {
                InitialFrameOpCode = frameHeader.OpCode;
            }
            ContinuationFrameByteQueue.Insert(0, payloadByteList);
            return null;
        }
        else
        {
            foreach (List<byte> bytes in ContinuationFrameByteQueue)
            {
                payloadByteList.InsertRange(0, bytes);
            }
        }

        return payloadByteList;
    }

    /// <summary>
    /// Reset
    /// </summary>
    public void Reset()
    {
        InitialFrameOpCode = null;
        ContinuationFrameByteQueue.Clear();
    }
}
