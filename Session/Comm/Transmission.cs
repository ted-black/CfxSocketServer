using System.Collections.Generic;
using CfxSocketServer.Session.Frame;

namespace CfxSocketServer.Session.Comm;

public class Transmission : ITransmission
{
    /// <inheritdoc cref="ITransmission.Payload"/>
    public List<byte> Payload { get; set; }

    /// <inheritdoc cref="ITransmission.OpCode"/>
    public OpCode? OpCode { get; set; }
}
