using System.Collections.Generic;

namespace SoftLayer.Messaging
{
    public sealed class FieldList : Dictionary<string, string>
    {
        public const int MaxFields = 64;
        public const int MinFieldKeyLength = 1;
        public const int MaxFieldKeyLength = 32;
        public const int MinFieldValueLength = 0;
        public const int MaxFieldValueLength = 1024;

        public FieldList()
            : base()
        {
        }

        public FieldList(IDictionary<string, string> entryCollection)
            : base(entryCollection)
        {
        }

        public new void Add(string key, string value)
        {
            if (Count >= MaxFields) {
                throw new TooManyItemsException("Number of fields cannot exceed " + MaxFields + ".");
            }

            if (key.Length > MaxFieldKeyLength) {
                throw new BadValueException("Field key cannot exceed " + MaxFieldKeyLength + " characters.");
            }
            else if (key.Length < MinFieldValueLength) {
                throw new BadValueException("Field key must be at least " + MinFieldKeyLength + " characters.");
            }

            if (value.Length > MaxFieldValueLength) {
                throw new BadValueException("Field value cannot exceed " + MaxFieldValueLength + " characters.");
            }
            else if (key.Length < MinFieldValueLength) {
                throw new BadValueException("Field value must be at least " + MinFieldValueLength + " characters.");
            }

            base.Add(key, value);
        }
    }
}
