using System;
using System.Collections.Generic;
using Relativity.Services;

namespace Relativity.Sync.WorkspaceGenerator
{
	public class RandomFieldsGenerator
	{
		private static readonly FieldType[] SupportedTypes = new[]
		{
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

		public IEnumerable<CustomField> GetRandomFields(int count)
		{
			for (int i = 0; i < count; i++)
			{
				FieldType randomType = SupportedTypes[_random.Next(0, SupportedTypes.Length)];

				CustomField field = new CustomField($"{i}-{randomType}", randomType);
				yield return field;
			}
		}
	}
}