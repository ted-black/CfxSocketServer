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
public class WebSocketChannel<T>() : IWebSocketChannel<T> where T : IChannelWriter, IComparable<IWebSocketChannel<T>>
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

            foreach (T name in Subscribers.Values)
            {
                nameString += $"{name.Name}, ";
            }

            return nameString[..^2];
        } 
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribers"/>
    public ConcurrentDictionary<string, T> Subscribers { get; private set; } = [];

    /// <inheritdoc cref="IWebSocketChannel{T}.Content"/>
    public ConcurrentQueue<string> Content {  get; private set; }

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(ISubscriberInfo)"/>
    public async Task WriteTextAllAsync(string payload)
    {
        List<Task> taskList = [];

        foreach (T subscriber in Subscribers.Values)
        {
            taskList.Add(subscriber.WriteTextAsync(payload));
        }

        await Task.WhenAll([.. taskList]);
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.WriteBinaryAllAsync(List{byte})"/>
    public async Task WriteBinaryAllAsync(List<byte> bytes)
    {
        List<Task> taskList = [];

        foreach (T subscriber in Subscribers.Values)
        {
            taskList.Add(subscriber.WriteBinaryAsync(bytes));
        }

        await Task.WhenAll([.. taskList]);
    }
    

    /// <inheritdoc cref="IWebSocketChannel{T}.Subscribe(T)"/>
    public bool Subscribe(T subscriber)
    {
        bool isAdded =  Subscribers.TryAdd(subscriber.Name, subscriber);
        Guid orphanIdToRemove = Guid.Empty;

        return isAdded;
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.UnSubscribe(T)"/>
    public bool UnSubscribe(T subscriberToRemove)
    {
        return Subscribers.TryRemove(subscriberToRemove.Name, out _);
    }

    /// <inheritdoc cref="IComparable.CompareTo(object?)"/>
    public int CompareTo(IWebSocketChannel<T> otherChannel)
    {


        if (otherChannel.Name == Name)
        {
            return 0;
        }

        return 1;
    } 
}
