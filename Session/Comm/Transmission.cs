using System.Collections.Generic;
using CfxSocketServer.Session.Frame;

namespace CfxSocketServer.Session.Comm;

public class Transmission : ITransmission
{
    /// <summary>
    /// Payload bytes received
    /// </summary>
    public List<byte> Payload { get; set; }

    /// <summary>
    /// Frame header
    /// </summary>
    public OpCode? OpCode { get; set; }
}
