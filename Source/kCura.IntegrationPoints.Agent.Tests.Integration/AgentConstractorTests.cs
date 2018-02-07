using NUnit.Framework;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class AgentConstractorTests
	{
		[Test]
		public void Run_Agent()
		{
			var agent = new Agent();
		}
	}
}