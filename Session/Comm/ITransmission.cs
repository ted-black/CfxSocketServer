using CfxSocketServer.Session.Frame;

namespace CfxSocketServer.Session.Comm;

interface ITransmission
{
    /// <summary>
    /// Payload bytes received
    /// </summary>
    List<byte> Payload { get; set; }

    /// <summary>
    /// Op code -- message type.
    /// </summary>
    OpCode? OpCode { get; set; }
}
