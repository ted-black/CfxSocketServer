namespace CfxSocketServer.Channel;

public class ChannelInfo : IChannelInfo
{
    public Guid Id { get; set; }

    public List<SubscriberInfo> Subscribers { get; set; }

    public string Name { 
        get 
        {
            string name = "";
            foreach (SubscriberInfo subscriber in Subscribers)
            {
                name += subscriber.Name + ", ";
            }
            return name[..^2];
        } 
    }
}
