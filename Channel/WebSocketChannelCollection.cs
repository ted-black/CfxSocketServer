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
public class WebSocketChannelCollection : IWebSocketChannelCollection
{
    private readonly ConcurrentDictionary<Guid, IWebSocketChannel<IChannelWriter>> channels = [];

    /// <inheritdoc cref="IWebSocketChannelCollection.Add(IWebSocketChannel{IChannelWriter})"/>
    public bool Add(IWebSocketChannel<IChannelWriter> channelToAdd)
    {
        foreach (KeyValuePair<Guid, IWebSocketChannel<IChannelWriter>> channel in channels)
        {
            if (channel.Value.IsExist(channelToAdd))
            {
                return false;
            }
        }

        return channels.TryAdd(channelToAdd.Id, channelToAdd);
    }

    /// <inheritdoc cref="IWebSocketChannelCollection.Channels"/>
    public ConcurrentDictionary<Guid, IWebSocketChannel<IChannelWriter>> Channels => channels;

    /// <inheritdoc cref="IWebSocketChannelCollection.Get(Guid)"/>
    public IWebSocketChannel<IChannelWriter> Get(Guid id)
    {
        channels.TryGetValue(id, out IWebSocketChannel<IChannelWriter> channel);
        return channel;
    }

    /// <inheritdoc cref="IWebSocketChannelCollection.Remove(IWebSocketChannel{IChannelWriter})"/>
    public void Remove(IWebSocketChannel<IChannelWriter> channel)
    {
        channels.TryRemove(channel.Id, out _);
    }
}
