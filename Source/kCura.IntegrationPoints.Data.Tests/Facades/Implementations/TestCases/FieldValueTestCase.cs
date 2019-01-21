using System.Collections;

namespace kCura.IntegrationPoints.Data.Tests.Facades.Implementations.TestCases
{
	public class FieldValueTestCase
	{
		public ICollection Value { get; }
		public string Name { get; }

		public FieldValueTestCase(ICollection value, string name)
		{
			Value = value;
			Name = name;
		}
	}
}
