using System.Collections.Generic;
using System.Text.RegularExpressions;
using SubscriptionList = System.Collections.Generic.List<SoftLayer.Messaging.ITopicSubscription>;

namespace SoftLayer.Messaging
{
    public class Topic
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 128;

        private string name = string.Empty;
        private TagList tags = new TagList();
        private List<ITopicSubscription> subscriptions = new List<ITopicSubscription>();

        public string Name
        {
            get { return name; }
            set
            {
                if (value.Length < MinNameLength) {
                    throw new BadValueException("Topic name cannot be empty.");
                }

                if (value.Length > MaxNameLength) {
                    throw new BadValueException("Topic name cannot be more than " + MaxNameLength + " characters.");
                }

                if (!Regex.IsMatch(value, @"^\w+$")) {
                    throw new BadValueException("Topic name can only contain letters, numbers, and underscores.");
                }

                name = value;
            }
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

        public SubscriptionList Subscriptions
        {
            get { return subscriptions; }
            set
            {
                subscriptions.Clear();

                if (value == null) {
                    return;
                }

                foreach (var tmpSubscription in value) {
                    subscriptions.Add(tmpSubscription);
                }
            }
        }

        public Topic()
        {
        }

        public Topic(string name)
        {
            this.Name = name;
        }
        public Topic(string name, TagList tagList)
        {
            this.Name = name;
            this.Tags = tagList;
        }

        public Topic(string name, string[] tagList)
        {
            this.Name = name;

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
