using System;
using Relativity.Services;

namespace Relativity.Sync.WorkspaceGenerator.Fields
{
    public class CustomField
    {
        public string Name { get; set; }

        public FieldType Type { get; set; }

        public CustomField(string name, FieldType type)
        {
            Name = name;
            Type = type;
        }

        public CustomField(string name, string type)
        {
            Name = name;
            Type = ParseFieldType(type);
        }

        private static FieldType ParseFieldType(string type)
        {
            switch (type)
            {
                case "Date":
                    return FieldType.Date;
                case "Decimal":
                    return FieldType.Decimal;
                case "Currency":
                    return FieldType.Currency;
                case "Fixed-Length Text":
                    return FieldType.FixedLengthText;
                case "Whole Number":
                    return FieldType.WholeNumber;
                case "Yes/No":
                    return FieldType.YesNo;
                default:
                    throw new ArgumentOutOfRangeException($"Not supported value for '{nameof(type)}' parameter: '{type}'");
            }
        }
    }
}