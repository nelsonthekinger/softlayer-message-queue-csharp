using System.Collections.Generic;

namespace SoftLayer.Messaging
{
    public class HttpTopicSubscription : ITopicSubscription
    {
        public string ID = string.Empty;
        public string URL = string.Empty;
        public HttpTopicSubscriptionMethod HttpMethod = HttpTopicSubscriptionMethod.POST;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
        public Dictionary<string, string> Headers = new Dictionary<string, string>();
        public string Body = string.Empty;

        public HttpTopicSubscription()
        {
        }

        public HttpTopicSubscription(string url)
        {
            this.URL = url;
        }

        public string GetID()
        {
            return this.ID;
        }

        public string GetEndpointTarget()
        {
            return this.URL;
        }
    }
}
