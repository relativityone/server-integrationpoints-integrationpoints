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
	public class MetadataSavedSearchToFolderTest: UiTest
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

		private RelativityProviderModel CreateRelativityProviderModel()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = "Saved Search";
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{_destinationContext.WorkspaceName} - {_destinationContext.WorkspaceId}";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No;
			return model;
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_1()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		public void RelativityProvider_TC_RTR_MDO_2()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		public void RelativityProvider_TC_RTR_MDO_3()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable(); 

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_4()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_5()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_6()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_7()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_8()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_9()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = _integrationPointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test, Order(10)]
		public void RelativityProvider_TC_RTR_MDO_10()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_11()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_12()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
		public void RelativityProvider_TC_RTR_MDO_13()
		{
			// Arrange
			_destinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
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
