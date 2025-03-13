using System.Net.Security;

namespace CfxSocketServer.Session.Frame;

/// <summary>
/// Frame header manager build frame objects for outbound and inbound payloads.
/// All the frame header values are computed and set per the RFC.
/// </summary>
public interface IFrameHeaderBuilder
{
    /// <summary>
    /// Get outbound frame header
    /// </summary>
    /// <param name="length"></param>
    /// <param name="opCode"></param>
    /// <returns></returns>
    List<byte> GetOutboundFrameHeader(ulong length, OpCode opCode);

    /// <summary>
    /// Get inbound frame header
    /// </summary>
    /// <param name="sslStream"></param>
    /// <returns></returns>
    Task<FrameHeader> GetInboundFrameHeaderAsync(SslStream sslStream);
}
