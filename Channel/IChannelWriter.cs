using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfxSocketServer.Channel;

public interface IChannelWriter
{
    /// <summary>
    /// Session id
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Send text payload
    /// </summary>
    /// <param name="payload"></param>
    Task WriteTextAsync(string payload);

    /// <summary>
    /// Send binary payload
    /// </summary>
    /// <param name="payload"></param>
    Task WriteBinaryAsync(List<byte> payload);
}
