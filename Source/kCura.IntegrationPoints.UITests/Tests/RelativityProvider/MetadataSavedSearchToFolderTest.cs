using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	using IntegrationPoint.Tests.Core;

	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class MetadataSavedSearchToFolderTest: RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModel()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name)
			{
				Source = "Saved Search",
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.No,
				FieldMapping = DefaultFieldsMapping
			};

			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();

			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		

		[Test]
		public void RelativityProvider_TC_RTR_MDO_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForField();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		

		[Test]
		public void RelativityProvider_TC_RTR_MDO_03()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForFolderTree();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_04()
		{
			// Arrange
			DestinationContext.ImportDocuments(testDataType:DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_05()
		{
			// Arrange
			DestinationContext.ImportDocuments(testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_06()
		{
			// Arrange
			DestinationContext.ImportDocuments(testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_07()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForField();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]); 
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_08()
		{
			// Arrange
			DestinationContext.ImportDocuments();
			
			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForFolderTree();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_09()
		{
			// Arrange
			DestinationContext.ImportDocuments(testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_10()
		{
			// Arrange
			DestinationContext.ImportDocuments(testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_11()
		{
			// Arrange
			DestinationContext.ImportDocuments(testDataType: DocumentTestDataBuilder.TestDataType.ModerateWithoutFoldersStructure);

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForRoot();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		[Test]
		public void RelativityProvider_TC_RTR_MDO_12()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForField();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}


		[Test]
		public void RelativityProvider_TC_RTR_MDO_13()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModel();

			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true; 

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();
			
			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			ValidateDocumentsForFolderTree();
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
		}

		private void ValidateDocumentsForRoot()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootValidator()
					.ValidateWith(new DocumentNativesValidator(false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForField()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFieldValidator()
					.ValidateWith(new DocumentNativesValidator(false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForFolderTree()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFolderTreeValidator()
					.ValidateWith(new DocumentNativesValidator(false));

			documentsValidator.Validate();
		}
	}

}
