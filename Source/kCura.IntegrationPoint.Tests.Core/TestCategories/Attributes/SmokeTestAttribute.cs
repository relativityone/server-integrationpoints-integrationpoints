using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
	public class SmokeTestAttribute : TestCaseAttribute
	{
		public SmokeTestAttribute()
		{
			Category = TestCategories.SMOKE_TEST;
		}
	}
}
