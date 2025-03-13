using System;
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
    /// Channel subscribers
    /// </summary>
    List<SubscriberInfo> Subscribers { get; }

    /// <summary>
    /// Subscribe to channel
    /// </summary>
    /// <param name="subscriber"></param>
    void Subscribe(SubscriberInfo subscriber);

    /// <summary>
    /// Unsubscribe from the channel
    /// </summary>
    /// <param name="subscriber"></param>
    void UnSubscribe(ISubscriberInfo subscriberToRemove);

    /// <summary>
    /// Get channel info
    /// </summary>
    /// <returns></returns>
    ChannelInfo GetChannelInfo();

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
}
