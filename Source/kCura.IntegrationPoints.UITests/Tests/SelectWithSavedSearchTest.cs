using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System.Threading;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.WEB_IMPORT_EXPORT)]
	[Category(TestCategory.MISCELLANEOUS)]
	public class SelectWithSavedSearchTest : UiTest
	{
		private const int _MILLISECONDSTIMEOUT = 1000;

		[IdentifiedTest("36b70022-060d-4ae1-994b-6619b67f02a2")]
		[RetryOnError]
		[Order(10)]
		[Ignore("REL-299432")]
		public void ChangesValueWhenSavedSearchIsChosenInDialog()
		{
			// Arrange
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(SourceContext.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewExportIntegrationPoint();
			first.Name = "Test IP";
			first.Destination = "Load File";

			ExportToFileSecondPage second = first.GoToNextPage();
			SavedSearchDialog ssd = second.OpenSavedSearchSelectionDialog();
			ssd.ChooseSavedSearch("All Documents");

			// Assert
			Thread.Sleep(_MILLISECONDSTIMEOUT);
			Assert.AreEqual("All Documents", second.GetSelectedSavedSearch());
		}
	}
}
