using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SoftLayer.Messaging
{
    public sealed class TagList : List<string>
    {
        public const int MaxTags = 32;
        public const int MaxTagLength = 32;

        public TagList()
            : base()
        {
        }

        public TagList(IEnumerable<string> stringCollection)
            : base(stringCollection)
        {
        }

        public new void Add(string tag)
        {
            if (!Regex.IsMatch(tag, @"^\w+$")) {
                throw new BadValueException("Tags can only contain letters, numbers, and underscores.");
            }

            if (Count >= MaxTags) {
                throw new TooManyItemsException("Cannot exceed " + MaxTags + "tags.");
            }

            if (tag.Length > MaxTagLength) {
                throw new TooManyItemsException("Tags cannot exceed " + MaxTagLength + " characters.");
            }

            base.Add(tag);
        }

        public override string ToString()
        {
            return ToString(", ");
        }

        public string ToString(string delimiter = ", ")
        {
            string tags = string.Empty;
            foreach (string tmpTag in this) {
                if (tags.Length > 0) {
                    tags += delimiter;
                }

                tags += tmpTag;
            }

            return tags;
        }
    }
}
