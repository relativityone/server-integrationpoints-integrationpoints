﻿namespace Relativity.Sync.Tests.Integration
{
	internal sealed class FieldValue
	{
		public string Field { get; }
		public object Value { get; }

		public FieldValue(string field, object value)
		{
			Field = field;
			Value = value;
		}
	}
}
