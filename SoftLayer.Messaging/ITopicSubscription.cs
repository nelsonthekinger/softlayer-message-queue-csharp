
namespace SoftLayer.Messaging
{
    public interface ITopicSubscription
    {
        string GetID();
        string GetEndpointTarget();
    }
}
