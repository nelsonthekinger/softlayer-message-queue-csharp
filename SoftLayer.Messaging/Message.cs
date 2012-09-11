using System;
using System.Collections.Generic;

namespace SoftLayer.Messaging
{
    public class Message
    {
        public const long DefaultVisibilityDelay = 0;
        public const long DefaultVisibilityInterval = 30;

        private string id = string.Empty;
        private string body = string.Empty;
        private FieldList fields = new FieldList();
        private long visibilityDelay = DefaultVisibilityDelay;
        private long visibilityInterval = DefaultVisibilityInterval;
        private decimal initialEntryTimestamp;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        public FieldList Fields
        {
            get { return fields; }
            set
            {
                fields.Clear();
                foreach (KeyValuePair<string, string> tmpEntry in value) {
                    fields[tmpEntry.Key] = tmpEntry.Value;
                }
            }
        }

        public long VisibilityDelay
        {
            get { return visibilityDelay; }
            set { visibilityDelay = value; }
        }

        public long VisibilityInterval
        {
            get { return visibilityInterval; }
            set { visibilityInterval = value; }
        }

        public DateTime InitialEntryTime
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds((long) initialEntryTimestamp);
            }
        }


        public Message()
        {
            this.fields = new FieldList();
        }

        public Message(string Body)
        {
            this.Body = Body;
            this.fields = new FieldList();
        }

        public void SetInitialEntryTime(long timestamp)
        {
            this.initialEntryTimestamp = (decimal) timestamp;
        }

        public void SetInitialEntryTime(decimal timestamp)
        {
            this.initialEntryTimestamp = timestamp;
        }
    }
}
