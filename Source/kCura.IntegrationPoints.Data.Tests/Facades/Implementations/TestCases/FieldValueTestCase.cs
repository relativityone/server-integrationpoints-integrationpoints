using System.Collections;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations.TestCases
{
	public class FieldValueTestCase
	{
		public object Value { get; }
		public string Name { get; }
		public int? Count { get; }

		public FieldValueTestCase(ICollection value, string name)
		{
			Value = value;
			Name = name;
			Count = value?.Count;
		}
	}
}
