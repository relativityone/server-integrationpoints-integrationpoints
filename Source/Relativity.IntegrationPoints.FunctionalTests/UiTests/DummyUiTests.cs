using FluentAssertions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.DummyUiTests
{
	//This test fixture is needed to execute FunctionalTestsSetupFixture
	//since this is the only test in kCura.IntegrationPoints.UITests
	//namespace. Remove it when any of UI test will be migrated to
	//Relativity.IntegrationPoints.FunctionalTests project
	[TestFixture]
	[Category(_WEB_IMPORT_EXPORT_TEST_CATEGORY)]
	[Category(_EXPORT_TO_RELATIVITY_TEST_CATEGORY)]
	[Category(_ONE_TIME_TESTS_SETUP)]
	public class DummyUiTests
    {
        private const string _EXPORT_TO_RELATIVITY_TEST_CATEGORY = "ExportToRelativity";
		private const string _WEB_IMPORT_EXPORT_TEST_CATEGORY = "WebImportExport";
		private const string _ONE_TIME_TESTS_SETUP = "OneTimeTestsSetup";

		[Test]
		public void DummyTest()
		{
			true.Should().BeTrue();
		}
	}
}
