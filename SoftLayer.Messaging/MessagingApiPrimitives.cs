using System;
using System.Collections.Generic;

namespace SoftLayer.Messaging.Primitives
{
    public class ApiResponse
    {
        public string message { get; set; }
    }

    public class EntityListResponse<T> : ApiResponse
    {
        public List<T> items { get; set; }
        public long item_count;
    }

    public class AuthResponse : ApiResponse
    {
        public string status { get; set; }
        public string token { get; set; }
    }

    public class StatsResponse : ApiResponse
    {
        public List<StatsMeasurementResponse> notifications { get; set; }
        public List<StatsMeasurementResponse> requests { get; set; }
    }

    public class StatsMeasurementResponse : ApiResponse
    {
        public long value { get; set; }
        public List<int> key { get; set; }

        public static DateTime ConvertDateKey(List<int> dateKey)
        {
            return new DateTime(
                ((dateKey.Count >= 1) ? dateKey[0] : 0),
                ((dateKey.Count >= 2) ? dateKey[1] : 0),
                ((dateKey.Count >= 3) ? dateKey[2] : 0),
                ((dateKey.Count >= 4) ? dateKey[3] : 0),
                ((dateKey.Count >= 5) ? dateKey[4] : 0),
                0,
                DateTimeKind.Utc);
        }
    }

    public class QueueResponse : ApiResponse
    {
        private List<string> _tags = new List<string>();

        public long expiration { get; set; }
        public string name { get; set; }
        public List<string> tags { get { return _tags; } set { _tags = value; } }
        public long visibility_interval { get; set; }
    }

    public class MessageResponse : ApiResponse
    {
        public string id { get; set; }
        public string body { get; set; }
        public Dictionary<string, string> fields { get; set; }
        public decimal initial_entry_time { get; set; }
        public long visibility_delay { get; set; }
        public long visibility_interval { get; set; }
    }

    public class MessageListResponse : EntityListResponse<MessageResponse>
    {
    }

    public class TopicResponse : ApiResponse
    {
        public string name { get; set; }
        public List<string> tags { get; set; }
    }

    public class TopicSubscriptionListResponse : EntityListResponse<ITopicSubscription>
    {
    }

    public class SubscriptionResponse : ApiResponse
    {
        public string id { get; set; }
        public string endpoint_type { get; set; }
        public SubscriptionEndpointResponse endpoint { get; set; }
    }

    public class SubscriptionEndpointResponse : ApiResponse
    {
        public string queue_name { get; set; }
        public string method { get; set; }
        public string url { get; set; }
        // params is a C# keyword, so escape appropriately.
        public Dictionary<string, string> @params { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public string body { get; set; }
    }
}
