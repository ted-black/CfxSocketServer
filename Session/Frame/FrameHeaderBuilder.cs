using System.Net.Security;

namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Frame header manager build frame objects for outbound and inbound payloads.
/// All the frame header values are computed and set per the RFC.
/// </summary>
public class FrameHeaderBuilder : IFrameHeaderBuilder
{
    /// <inheritdoc cref="IFrameHeaderBuilder.GetOutboundFrameHeader(ulong, OpCode)"/>
    public List<byte> GetOutboundFrameHeader(ulong length, OpCode opCode)
    {
        // Construct the frame header
        //
        FrameHeader frameHeader = new(opCode, length);

        return frameHeader;
    }

    /// <inheritdoc cref="IFrameHeaderBuilder.GetInboundFrameHeaderAsync(SslStream)"/>
    public async Task<FrameHeader> GetInboundFrameHeaderAsync(SslStream sslStream)
    {
        // Get bytes from stream according to length field
        //
        byte[] frameBuffer = new byte[14];
        int bytesToRead = 2;
        int bytesRead = 0;
        for (int i = 0; i < 2; i++)
        {
            bytesRead += await sslStream.ReadAsync(frameBuffer, bytesRead, bytesToRead);
            bytesToRead = (frameBuffer[1] & 127) switch
            {
                126 => 6,
                127 => 12,
                _ => 4,
            };
        }

        // Capture the bytes actually read
        //
        List<byte> byteList = new();
        for(int i = 0; i < bytesRead; i++)
        {
            byteList.Add(frameBuffer[i]);
        }

        // Construct the frame header
        //
        FrameHeader frameHeader = new(byteList.ToArray());

        return frameHeader;
    }
}
