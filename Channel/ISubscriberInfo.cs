namespace CfxSocketServer.Channel;

public interface ISubscriberInfo
{
    Guid Id { get; set; }

    string Name { get; set; }
}
