using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfxSocketServer.Channel;

/// <summary>
/// WebSocket channel, can write to groups of channel writers
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="webSocketSessions"></param>
public class WebSocketChannel<T>(ConcurrentDictionary<Guid, T> webSocketSessions) : IWebSocketChannel<T> where T : IChannelWriter, IComparable<IWebSocketChannel<T>>
{
    /// <inheritdoc cref="IWebSocketChannel{T}.Id"/>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribers"/>
    public List<SubscriberInfo> Subscribers { get; private set; } = [];

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(ISubscriberInfo)"/>
    public ChannelInfo GetChannelInfo()
    {
        ChannelInfo channelInfo = new ChannelInfo()
        {
            Id = Id,
            Subscribers = []
        };
        foreach (SubscriberInfo subscriberInfo in Subscribers)
        {
            channelInfo.Subscribers.Add(subscriberInfo);
        }

        return channelInfo;
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(ISubscriberInfo)"/>
    public async Task WriteTextAllAsync(string payload)
    {
        List<Task> taskList = [];

        foreach (SubscriberInfo subscriberInfo in Subscribers)
        {
            if (webSocketSessions.TryGetValue(subscriberInfo.Id, out T subscriber))
            {
                taskList.Add(subscriber.WriteTextAsync(payload));
            }
        }

        await Task.WhenAll([.. taskList]);
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.WriteBinaryAllAsync(List{byte})"/>
    public async Task WriteBinaryAllAsync(List<byte> bytes)
    {
        List<Task> taskList = [];

        foreach (SubscriberInfo subscriberInfo in Subscribers)
        {
            if (webSocketSessions.TryGetValue(subscriberInfo.Id, out T subscriber))
            {
                taskList.Add(subscriber.WriteBinaryAsync(bytes));
            }
        }

        await Task.WhenAll([.. taskList]);
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(ISubscriberInfo)"/>
    public void Subscribe(SubscriberInfo subscriber)
    {
        Subscribers.Add(subscriber);
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.UnSubscribe(ISubscriberInfo)"/>
    public void UnSubscribe(ISubscriberInfo subscriberToRemove)
    {
        Subscribers.RemoveAll(subscriber => subscriber.Id == subscriberToRemove.Id);
    }

    /// <inheritdoc cref="IComparable.CompareTo(object?)"/>
    public int CompareTo(IWebSocketChannel<T> otherChannel)
    {
        List<Guid> otherIds = [];
        List<Guid> subscriberIds = [];

        foreach (SubscriberInfo otherInfo in otherChannel.Subscribers)
        {
            otherIds.Add(otherInfo.Id);
        }

        foreach (SubscriberInfo subscriber in Subscribers)
        {
            subscriberIds.Add(subscriber.Id);
        }
        bool setsAreEqual = new HashSet<Guid>(otherIds).SetEquals(subscriberIds);

        if (setsAreEqual)
        {
            return 0;
        }

        return 1;
    } 
}
