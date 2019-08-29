using FluentAssertions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.DummyUiTests
{
	//This test fixture is needed to execute FunctionalTestsSetupFixture
	//since this is the only test in kCura.IntegrationPoints.UITests
	//namespace. Remove it when any of UI test will be migrated to
	//Relativity.IntegrationPoints.FunctionalTests project
	[TestFixture]
	public class DummyUiTests
	{
		[Test]
		public void DummyTest()
		{
			true.Should().BeTrue();
		}
	}
}
