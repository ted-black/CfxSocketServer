using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfxSocketServer.Channel;

/// <summary>
/// Manage the collection of webSocket channels
/// </summary>
public class WebSocketChannelCollection
{
    private readonly ConcurrentDictionary<Guid, IWebSocketChannel<IChannelWriter>> channels = [];
}
