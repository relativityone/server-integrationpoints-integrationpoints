using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using TestContext = kCura.IntegrationPoints.UITests.Configuration.TestContext;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class LinksSavedSearchToFolderTest: UiTest
	{
		private TestContext _destinationContext = null;
		private IntegrationPointsAction _integrationPointsAction;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			EnsureGeneralPageIsOpened();
			_integrationPointsAction = new IntegrationPointsAction(Driver, Context);
		}


		[SetUp]
		public void SetUp()
		{
			_destinationContext = new TestContext()
			.CreateWorkspace(); 
		}

		[TearDown]
		public void TearDown()
		{ 
			_destinationContext?.TearDown();
		}

		private RelativityProviderModel CreateRelativityProviderModelWithLinks()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = "Saved Search";
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{_destinationContext.WorkspaceName} - {_destinationContext.WorkspaceId}";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;
			return model;
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_1()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		public void RelativityProvider_TC_RTR_LO_2()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		public void RelativityProvider_TC_RTR_LO_3()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_4()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_5()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_6()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_7()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]); 
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_8()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_9()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_10()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_11()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_12()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_LO_13()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true; 

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

	}

}
