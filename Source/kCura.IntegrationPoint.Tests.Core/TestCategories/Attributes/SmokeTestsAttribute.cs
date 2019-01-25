using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes
{
	public class SmokeTestsAttribute : TestFixtureAttribute
	{
		public SmokeTestsAttribute()
		{
			Category = TestCategories.SMOKE_TEST;
		}
	}
}
