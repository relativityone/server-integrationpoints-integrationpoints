using System.Threading;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Category(TestCategory.MISCELLANEOUS)]
    public class SelectWithSavedSearchTest : UiTest
	{
		private const int _MILLISECONDSTIMEOUT = 1000;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
		}

		[Test, Order(10)]
		public void ChangesValueWhenSavedSearchIsChosenInDialog()
		{
			// Arrange
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

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
