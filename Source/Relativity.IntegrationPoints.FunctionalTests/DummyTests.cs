using FluentAssertions;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.FunctionalTests
{
	[TestFixture]
	public class DummyTests
	{
		[IdentifiedTest("08efc703-ef29-44d4-9b85-5f5893bf84ee")]
		public void DummyTest()
		{
			var flag = true;
			flag.Should().BeTrue();
		}
	}
}
