namespace CfxSocketServer.Channel;

public interface IChannelInfo
{
    public Guid Id { get; set; }

    public List<SubscriberInfo> Subscribers { get; set; }

    public string Name { get; }
}
