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
		private const string _EXPORT_TO_RELATIVITY_TEST_CATEGORY = "ExportToRelativity";

		[Test]
		public void DummyTest()
		{
			true.Should().BeTrue();
		}

		[Test]
		[Category(_EXPORT_TO_RELATIVITY_TEST_CATEGORY)]
		public void DummyExportToRelativityTest()
		{
			true.Should().BeTrue();
		}
	}
}
