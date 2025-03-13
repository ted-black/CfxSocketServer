namespace CfxSocketServer;

public class Constant
{
    public const long MaxIntPlusOne = 65536;
    public const int SocketPort = 44335;
    public const int MaxConnCount = 128;
    public const int BufferSize = 1024;
    public const byte IsFinalFrameMask = 0x80;
    public const byte IsNonFinalFrameMask = 0x7F;
    public const byte IsMaskMask = 0x80;
    public const byte IsNonMaskMask = 0x7F;
    public const byte OpCodeMask = 0xF;
    public const byte SetOpCodeMask = 0xF0;
    public const byte SetPayloadLengthMask = 0x80;
    public const string HandShakeNewline = "\r\n\r\n";
    public const string MagicKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    public const string SecWebSocketKey = "Sec-WebSocket-Key";
    public const string HandShakeHeader = @"HTTP/1.1 101 Switching Protocols
Upgrade: websocket
Connection: Upgrade
Sec-WebSocket-Accept: ";

    public class Security
    {
        public static string WebSocketCert { get { return Environment.GetEnvironmentVariable("WebSocketCert"); } }
        public static string WebSocketKey { get { return Environment.GetEnvironmentVariable("WebSocketKey"); } }
    }

    public class Channel
    {
        public const string UserName = "userName";
    }
}
