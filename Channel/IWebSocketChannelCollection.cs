namespace CfxSocketServer.Channel;

public interface IWebSocketChannelCollection
{
    /// <summary>
    /// Get a webSocket channel
    /// </summary>
    /// <returns></returns>
    IWebSocketChannel<IChannelWriter> Get(Guid id);

    /// <summary>
    /// Add a webSocket channel
    /// </summary>
    /// <param name="channel"></param>
    bool Add(IWebSocketChannel<IChannelWriter> channel);

    /// <summary>
    /// Remove a webSocket channel
    /// </summary>
    /// <param name="channel"></param>
    void Remove(IWebSocketChannel<IChannelWriter> channel);
}
