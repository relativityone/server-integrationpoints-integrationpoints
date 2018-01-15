using System;
using System.Threading;
using kCura.Injection.Behavior;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class RelativityProviderTest : UiTest
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
		}

		[Test, Order(10)]
		public void RelativityProvider()
		{
			// Arrange
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test Push IP";
			first.Destination = "Relativity";

			PushToRelativitySecondPage second = first.GoToNextPagePush();
			second.SourceSelect = "Saved Search";
			second.SelectAllDocuments();
			second.RelativityInstance = "This Instance";
			second.DestinationWorkspace = "Test Workspace 2017-10-31_15-07-43 - 9846545";
			second.SelectFolderLocation();
			second.FolderLocationSelect.ChooseRootElement();

			PushToRelativityThirdPage third = second.GoToNextPage();
			third.MapAllFields();
			third.SelectOverwrite = "Overlay Only";
			third.SelectCopyNativeFiles("Physical Files");
			third.SelectFolderPathInfo = "No";

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

	}
}
