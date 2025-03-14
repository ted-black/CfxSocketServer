namespace CfxSocketServer.Session.Frame;

public class FrameResolver : IFrameResolver
{
    /// <inheritdoc cref="IFrameResolver.ContinuationFrameByteQueue"/>
    public List<List<byte>> ContinuationFrameByteQueue { get; set; }

    /// <inheritdoc cref="IFrameResolver.InitialFrameOpCode"/>
    public OpCode? InitialFrameOpCode { get; set; }

    /// <inheritdoc cref="IFrameResolver.ResolveFrame(FrameHeader, List{byte})"/>
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

    /// <inheritdoc cref="IFrameResolver.Reset"/>
    public void Reset()
    {
        InitialFrameOpCode = null;
        ContinuationFrameByteQueue.Clear();
    }
}
