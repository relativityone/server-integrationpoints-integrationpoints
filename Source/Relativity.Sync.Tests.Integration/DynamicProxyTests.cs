using NUnit.Framework;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class DynamicProxyTests
	{
		[Test]
		public void ItShouldWrapKeplerServiceForUser()
		{
			//TODO we need changes to container initialization to be able to override registration
			Assert.Ignore("Changes to initialization required");
		}
	}
}