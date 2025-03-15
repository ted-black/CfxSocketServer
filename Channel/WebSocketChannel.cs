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
    public ConcurrentDictionary<Guid, T> Subscribers { get; private set; } = [];

    /// <inheritdoc cref="IWebSocketChannel{T}.Orphans"/>
    public ConcurrentDictionary<Guid, string> Orphans { get; private set; }

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
        bool isAdded =  Subscribers.TryAdd(subscriber.Id, subscriber);
        Guid orphanIdToRemove = Guid.Empty;

        if (isAdded)
        {
            subscriber.ChannelId = Id;
            foreach (Guid orphan in Orphans.Keys)
            {
                if (Orphans.TryGetValue(orphan, out string orphanSuscriber))
                {
                    if(orphanSuscriber == subscriber.Name)
                    {
                        orphanIdToRemove = orphan;
                    }
                }
            }

            if (orphanIdToRemove != Guid.Empty)
            {
                Orphans.TryRemove(orphanIdToRemove, out string _);
            }
        }

        return isAdded;
    }

    /// <inheritdoc cref="IWebSocketChannel{T}.UnSubscribe(T)"/>
    public void UnSubscribe(T subscriberToRemove)
    {
        if(Subscribers.TryRemove(subscriberToRemove.Id, out _))
        {
            Orphans.TryAdd(subscriberToRemove.Id, subscriberToRemove.Name);
        }
    }

    /// <summary>
    /// <inheritdoc cref="IWebSocketChannel{T}.CanReSubscribe(List{string}"/>
    /// </summary>
    /// <param name="subscriberNames"></param>
    /// <returns></returns>
    public bool CanReSubscribe(List<string> subscriberNames)
    {
        List<string> currentSubscriberNames = [];
        foreach (T subscriber in Subscribers.Values)
        {
            currentSubscriberNames.Add(subscriber.Name);
        }
        foreach (string orphanedSubscriber in Orphans.Values)
        {
            currentSubscriberNames.Add(orphanedSubscriber);
        }

        return new HashSet<string>(subscriberNames).SetEquals(currentSubscriberNames);
    }

    /// <inheritdoc cref="IComparable.CompareTo(object?)"/>
    public int CompareTo(IWebSocketChannel<T> otherChannel)
    {
        List<Guid> otherIds = [];
        List<Guid> subscriberIds = [];

        foreach (Guid otherSubscriberId in otherChannel.Subscribers.Keys)
        {
            otherIds.Add(otherSubscriberId);
        }

        foreach (Guid subscriberId in Subscribers.Keys)
        {
            subscriberIds.Add(subscriberId);
        }
        bool setsAreEqual = new HashSet<Guid>(otherIds).SetEquals(subscriberIds);

        if (setsAreEqual)
        {
            return 0;
        }

        return 1;
    } 
}
