using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core.Jobs
{
	[TestFixture]
	public class SampleTest : IntegrationTestBase
	{
		[Test]
		[Explicit]
		public void Test()
		{
			bool createdUser = User.CreateUserRest("first", "last", "flast@kcura.com");
			bool createdIntegrationPoint = User.CreateUserRest("first", "last", "flast@kcura.com");
		}
	}
}