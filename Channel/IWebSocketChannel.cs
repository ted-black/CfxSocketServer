﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfxSocketServer.Channel;

public interface IWebSocketChannel<T> where T : IChannelWriter
{
    /// <summary>
    /// Channel Id
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Channel subscribers
    /// </summary>
    ConcurrentDictionary<Guid, T> Subscribers { get; }

    /// <summary>
    /// Content
    /// </summary>
    ConcurrentQueue<string> Content { get; }

    /// <summary>
    /// Subscribe to channel
    /// </summary>
    /// <param name="subscriber"></param>
    bool Subscribe(T subscriber);

    /// <summary>
    /// Unsubscribe from the channel
    /// </summary>
    /// <param name="subscriberToRemove"></param>
    bool UnSubscribe(T subscriberToRemove);

    /// <summary>
    /// Write utf8 to all subscriptions
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    Task WriteTextAllAsync(string payload);

    /// <summary>
    /// Write binary to all subscriptions
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    Task WriteBinaryAllAsync(List<byte> bytes);

    /// <summary>
    /// Compare to
    /// </summary>
    /// <param name="otherChannel"></param>
    /// <returns></returns>
    bool IsExist(IWebSocketChannel<T> otherChannel);
}
