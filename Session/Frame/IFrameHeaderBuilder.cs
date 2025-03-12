using System.Net.Security;

namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Frame header manager build frame objects for outbound and inbound payloads.
/// All the frame header values are computed and set per the RFC.
/// </summary>
public interface IFrameHeaderBuilder
{
    List<byte> GetOutboundFrameHeader(ulong length, OpCode opCode);

    Task<FrameHeader> GetInboundFrameHeaderAsync(SslStream sslStream);
}
