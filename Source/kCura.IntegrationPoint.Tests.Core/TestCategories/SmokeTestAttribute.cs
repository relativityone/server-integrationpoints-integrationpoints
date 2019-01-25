using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories
{
	public class SmokeTestAttribute : TestCaseAttribute
	{
		public SmokeTestAttribute()
		{
			Category = TestCategories.SMOKE_TEST;
		}
	}
}
