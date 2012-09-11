using System.Collections.Generic;

namespace SoftLayer.Messaging
{
    public class QueueTopicSubscription : ITopicSubscription
    {
        public string ID = string.Empty;
        public string QueueName = string.Empty;

        public QueueTopicSubscription()
        {
        }

        public QueueTopicSubscription(string queueName)
        {
            this.QueueName = queueName;
        }

        public string GetID()
        {
            return this.ID;
        }

        public string GetEndpointTarget()
        {
            return this.QueueName;
        }
    }
}
