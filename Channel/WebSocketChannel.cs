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
public class WebSocketChannel<T>() : IWebSocketChannel<T> where T : IChannelWriter
{
    /// <inheritdoc cref="IWebSocketChannel{T}.Id"/>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// <inheritdoc cref="IWebSocketChannel{T}.Name"/>
    /// </summary>
    public string Name 
    { 
        get
        {
            string nameString = string.Empty;
            SortedSet<string> names = new();

            foreach (T subscriber in Subscribers.Values)
            {
                if (!names.Contains(nameString))
                {
                    names.Add(subscriber.Name);
                }
            }

            foreach (string name in names)
            {
                nameString += $"{name}, ";
            }

            return nameString[..^2];
        } 
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribers"/>
    public ConcurrentDictionary<Guid, T> Subscribers { get; private set; } = [];

    /// <inheritdoc cref="IWebSocketChannel{T}.Content"/>
    public ConcurrentQueue<string> Content {  get; private set; } = [];

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(ISubscriberInfo)"/>
    public async Task WriteTextAllAsync(string payload)
    {
        List<Task> taskList = [];

        Content.Enqueue(payload);

        foreach (T subscriber in Subscribers.Values)
        {
            if (subscriber.IsListening)
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

        foreach (T subscriber in Subscribers.Values)
        {
            if (subscriber.IsListening) { 
                taskList.Add(subscriber.WriteBinaryAsync(bytes));
            }
        }

        await Task.WhenAll([.. taskList]);
    }
    

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(T)"/>
    public bool Subscribe(T subscriber)
    {
        subscriber.ChannelId = Id;

        T channelSubscriber = Subscribers.FirstOrDefault(Subscribers => Subscribers.Value.Name == subscriber.Name).Value;

        if (channelSubscriber is null)
        {
            return Subscribers.TryAdd(subscriber.Id, subscriber);
        }

        if (!channelSubscriber.IsListening)
        {
            Subscribers.TryRemove(channelSubscriber.Id, out _);
            return Subscribers.TryAdd(subscriber.Id, subscriber);
        }

        return false;
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.UnSubscribe(T)"/>
    public bool UnSubscribe(T subscriberToRemove)
    {
        return Subscribers.TryRemove(subscriberToRemove.Id, out _);
    }

    /// <inheritdoc cref="IComparable.CompareTo(object?)"/>
    public bool IsExist(IWebSocketChannel<T> otherChannel)
    {
        if (otherChannel.Name == Name)
        {
            return true;
        }

        return false;
    } 
}
