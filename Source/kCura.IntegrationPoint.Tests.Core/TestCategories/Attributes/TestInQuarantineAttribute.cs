using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
	public class TestInQuarantineAttribute : TestCaseAttribute
	{
		public TestInQuarantineAttribute(TestQuarantineState state, string reason = null)
		{
			Category = TestCategories.IN_QUARANTINE;
		}
	}
}
