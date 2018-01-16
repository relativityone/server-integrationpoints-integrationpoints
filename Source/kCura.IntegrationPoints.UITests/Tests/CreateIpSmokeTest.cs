using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class CreateIpSmokeTest : UiTest
	{
		private IntegrationPointsAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}

		[Test, Order(10)]
		public void CreateIp()
		{
			// Arrange
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			// Act
			IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
			ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
			first.Name = "Test IP";
			first.Destination = "Load File";

			ExportToFileSecondPage second = first.GoToNextPage();
			second.SelectAllDocuments();

			ExportToFileThirdPage third = second.GoToNextPage();
			third.DestinationFolder.ChooseRootElement();

			IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Relativity (.dat); Unicode", generalProperties.Properties["Load file format:"]);
		}

		private ExportToLoadFileProviderModel CreateExportToLoadFileProviderModel()
		{
			var model = new ExportToLoadFileProviderModel("Test IP");

			model.DestinationProvider = IntegrationPointGeneralModel.INTEGRATION_POINT_PROVIDER_LOADFILE;

			return model;
		}

		[Test, Order(11)]
		public void CreateExportToLoadFileIp()
		{
			// Arrange
			ExportToLoadFileProviderModel model = CreateExportToLoadFileProviderModel();

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewExportToLoadfileIntegrationPointAction(model);

			// Assert
			Assert.AreEqual("Relativity (.dat); Unicode", detailsPage.SelectGeneralPropertiesTable().Properties["Load file format:"]);
		}
	}
}
