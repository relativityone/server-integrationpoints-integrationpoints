using FluentAssertions;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.FunctionalTests
{
	[TestFixture]
	public class DummyTest
	{
		[IdentifiedTest("08efc703-ef29-44d4-9b85-5f5893bf84ee")]
		public DummyTest()
		{
			var flag = true;
			flag.Should().BeTrue();
		}
	}
}
