using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Fields
{
    public class RandomFieldsGenerator : IRandomFieldsGenerator
    {
        private static readonly FieldType[] SupportedTypes = new []
        {
            FieldType.Date,
            FieldType.Decimal,
            FieldType.Currency,
            FieldType.FixedLengthText,
            FieldType.WholeNumber,
            FieldType.YesNo
        };

        private readonly Random _random;

        public RandomFieldsGenerator()
        {
            _random = new Random();
        }

        public IEnumerable<CustomField> GetRandomFields(List<TestCase> testCases)
        {
            List<CustomField> fields = new List<CustomField>();

            for (int i = 0; i < testCases.Max(x => x.NumberOfFields); i++)
            {
                FieldType randomType = SupportedTypes[_random.Next(0, SupportedTypes.Length)];
                CustomField field = new CustomField($"{i:D3}-{randomType}", randomType);
                fields.Add(field);
            }

            return fields;
        }
    }
}