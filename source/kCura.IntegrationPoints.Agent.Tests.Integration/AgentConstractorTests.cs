using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Category(Constants.INTEGRATION_CATEGORY)]
	public class AgentConstractorTests
	{
		[Test]
		public void Run_Agent()
		{
			var agent = new Agent();
		}
	}
}