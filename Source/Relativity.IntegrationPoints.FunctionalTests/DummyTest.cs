using FluentAssertions;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.FunctionalTests
{
	[TestFixture]
	public class DummyTest
	{
		[Test]
		public DummyTest()
		{
			var flag = true;
			flag.Should().BeTrue();
		}
	}
}
