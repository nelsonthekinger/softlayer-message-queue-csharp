using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SoftLayer.Messaging
{
    public class Queue
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 128;

        public const int DefaultExpiration = 604800;
        public const int MinExpiration = 600;
        public const int MaxExpiration = 2592000;

        public const int DefaultVisibilityInterval = 60;
        public const int MinVisibilityInterval = 10;
        public const int MaxVisibilityInterval = 86400;

        public const int MaxMessagesPerPop = 100;


        private string name = string.Empty;
        private long expiration = DefaultExpiration;
        private long visibilityInterval = DefaultVisibilityInterval;
        private TagList tags = new TagList();

        public string Name
        {
            get { return name; }
            set {
                if (value.Length < MinNameLength) {
                    throw new BadValueException("Queue name cannot be empty.");
                }

                if (value.Length > MaxNameLength) {
                    throw new BadValueException("Queue name cannot be more than " + MaxNameLength + " characters.");
                }

                if (!Regex.IsMatch(value, @"^\w+$")) {
                    throw new BadValueException("Queue name can only contain letters, numbers, and underscores.");
                }

                name = value; 
            }
        }

        public long Expiration
        {
            get { return expiration; }
            set { expiration = value; }
        }

        public long VisibilityInterval
        {
            get { return visibilityInterval; }
            set { visibilityInterval = value; }
        }

        public TagList Tags
        {
            get { return tags; }
            set
            {
                tags.Clear();
                
                if (value == null) {
                    return;
                }

                foreach (string tmpTag in value) {
                    tags.Add(tmpTag);
                }
            }
        }

        public Queue()
        {
        }

        public Queue(string name)
        {
            this.Name = name;
        }

        public Queue(string name, long expiration)
        {
            this.Name = name;
        }

        public Queue(string name, long expiration, long visibilityInterval)
        {
            this.Name = name;
        }

        public Queue(string name, long expiration, long visibilityInterval, TagList tagList)
        {
            this.Name = name;
            this.Expiration = expiration;
            this.VisibilityInterval = visibilityInterval;
            this.Tags = tagList;
        }

        public Queue(string name, long expiration, long visibilityInterval, string[] tagList)
        {
            this.Name = name;
            this.Expiration = expiration;
            this.VisibilityInterval = visibilityInterval;

            foreach (string tmpTag in tagList) {
                Tags.Add(tmpTag);
            }
        }

        public string GetTagList(string delimiter = ", ")
        {
            return Tags.ToString(delimiter);
        }
    }
}
