namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Frame header object per RFC, reads and writes to byte array via properties
/// </summary>
public class FrameHeader: List<byte>, IFrameHeader
{
    #region constructors

    /// <summary>
    /// Inbound frame header constructor
    /// </summary>
    /// <param name="bytes"></param>
    public FrameHeader(byte[] bytes)
    {
        AddRange(bytes);
    }

    /// <summary>
    /// Outbound frame header contructor
    /// </summary>
    /// <param name="opCode"></param>
    /// <param name="payloadLength"></param>
    /// <param name="isFinalFrame"></param>
    public FrameHeader(OpCode? opCode, ulong payloadLength, bool isFinalFrame = true)
    {
        // A new frame is at least 2 bytes, when constructed this way
        //
        AddRange(new byte[2]);
        IsFinal = isFinalFrame;
        OpCode = opCode;
        PayloadLength = payloadLength;
    }

    #endregion

    #region public properties

    /// <summary>
    /// Is final frame 
    /// </summary>
    public bool IsFinal { 
        get 
        {
            return (this[0] & Constant.IsFinalFrameMask) == Constant.IsFinalFrameMask;
        }
        private set
        {
            this[0] =  value ? (byte)(this[0] | Constant.IsFinalFrameMask) : (byte)(this[0] & Constant.IsNonFinalFrameMask);
        } 
    }

    /// <summary>
    /// Is mask
    /// </summary>
    public bool IsMask
    {
        get
        {
            return (this[1] & Constant.IsMaskMask) == Constant.IsMaskMask;
        }
    }

    /// <summary>
    /// The op code
    /// </summary>
    public OpCode? OpCode
    {
        get
        {
            return (OpCode)(this[0] & Constant.OpCodeMask);
        }
        private set
        {
            this[0] = (byte)(this[0] & Constant.SetOpCodeMask | (byte)value);
        }
    }

    /// <summary>
    /// The length of the payload
    /// </summary>
    public ulong PayloadLength { 
        get {
            ulong length;
            switch (this[1] & 127)
            {
                case 126:
                    length = GetLengthFromWord(this);
                    break;
                case 127:
                    length = GetLengthFromQuadWord(this);
                    break;
                default:
                    length = (ulong)this[1] & 127;
                    break;
            }

            return length;
        } 

        private set { 
            if(value < 126)
            {
                // length is the 7 least significant bits of 2nd byte
                //
                this[1] = (byte)(this[1] & Constant.SetPayloadLengthMask | (byte)value);
            }
            else if(value < Constant.MaxIntPlusOne)
            {
                // Length is a word
                //
                this[1] = (byte)(this[1] & Constant.SetPayloadLengthMask | 0x7E);
                byte[] lengthBytes = BitConverter.GetBytes(value);
                for(int i = lengthBytes.Length; i > 0; i--)
                {
                    if (i < 3)
                    {
                        Add(lengthBytes[i - 1]);
                    }
                }

            }
            else
            {
                // Length is a quad word
                //
                this[1] = (byte)(this[1] & Constant.SetPayloadLengthMask | 0x7F);
                byte[] payloadLength = BitConverter.GetBytes(value);
                for (int i = payloadLength.Length; i > 0; i--)
                {
                    Add(payloadLength[i - 1]);
                }
            }
        } 
    }

    /// <summary>
    /// The mask
    /// </summary>
    public byte[] Mask { 
        get 
        {
            byte[] mask = new byte[4];
            if (IsMask)
            {
                switch (this[1] & 0x7F)
                {
                    case 126:
                        for (int i = 0; i < 4; i++)
                        {
                            mask[i % 4] = this[i % 4 + 4];
                        }
                        break;
                    case 127:
                        for (int i = 0; i < 4; i++)
                        {
                            mask[i % 4] = this[i % 4 + 10];
                        }
                        break;
                    default:
                        for (int i = 0; i < 4; i++)
                        {
                            mask[i % 4] = this[i % 4 + 2];
                        }
                        break;
                }

                return mask;
            }
            else return null;
        } 
    }

    /// <summary>
    /// To string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(IsFinal)} = {IsFinal} , {nameof(OpCode)} = {OpCode} , {nameof(IsMask)} = {IsMask} , {nameof(PayloadLength)} = {PayloadLength} , HeaderLength = {Count}";
    }

    #endregion

    #region private methods

    /// <summary>
    /// Get a quad word from bytes
    /// </summary>
    /// <param name="frameBytes"></param>
    /// <returns></returns>
    private ulong GetLengthFromQuadWord(List<byte> frameBytes)
    {
        ulong length = 0;
        List<ulong> quadWords = [];
        for (int i = 0; i < 8; i++)
        {
            int shift = (7 - i) * 8;
            ulong num = frameBytes[i + 2];
            quadWords.Add(num << shift);
        }

        foreach (ulong quadWord in quadWords)
        {
            length |= quadWord;
        }

        return length;
    }

    /// <summary>
    /// Get a word from bytes
    /// </summary>
    /// <param name="frameBytes"></param>
    /// <returns></returns>
    private ulong GetLengthFromWord(List<byte> frameBytes)
    {
        ulong length;
        ushort left = frameBytes[2];
        length = (ulong)(left << 8 | frameBytes[3]);
        return length;
    }

    #endregion
}
