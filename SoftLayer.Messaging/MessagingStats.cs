using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftLayer.Messaging
{
    public class MessagingStats
    {
        public Dictionary<DateTime, long> Requests { get; set; }
        public Dictionary<DateTime, long> Notifications { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public MessagingStats()
        {
            this.Notifications = new Dictionary<DateTime, long>();
            this.Requests = new Dictionary<DateTime, long>();
        }

        public long TotalRequests
        {
            get { return Requests.Values.Sum(); }
        }

        public long TotalNotifications
        {
            get { return Notifications.Values.Sum(); }
        }

    }
}
