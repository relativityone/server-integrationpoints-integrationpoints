using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync.SavedSearch]
	[Category(TestCategory.RIP_SYNC)]
	public class NativesSavedSearchToFolderTest : RelativityProviderTestsBase
	{
		private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title"),
			new Tuple<string, string>("Document Folder Path", "Document Folder Path"),
		};

		private RelativityProviderModel CreateRelativityProviderModelWithNatives()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				SavedSearch = "All documents",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.PhysicalFiles,
				FieldMapping = DefaultFieldsMapping
			};

			return model;
		}

		[IdentifiedTest("b9cf5d44-9597-4566-9f87-32b1c601df1d")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public void RelativityProvider_TC_RTR_NF_01()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRoot();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("ac605017-d041-441e-8349-4b283c1aca2c")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_02()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act

			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("4e91b8f4-1560-4580-928b-1fda7f25a497")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_03()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("ed9dd678-45f0-426f-900b-f94177f46e77")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_04()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("34c420f0-2ce4-4d76-aabc-7431897fd02d")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_05()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("ee1f11dd-afa0-45bf-beeb-aeafa489766f")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_06()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("a0f1718e-a016-4312-827f-ce84967a4ad4")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_07()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("9166f5c5-a66f-496a-9bac-6263b4d1934d")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_08()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("cb25ba1e-7301-45dc-87e8-b2ccd2f126fe")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_09()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
			ValidateDocumentsFolderPathField();
		}


		[IdentifiedTest("151412cc-8843-4637-8e87-781dda3fadfa")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_10()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("39383c5d-79c1-44c1-9391-9e433de88574")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_11()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
			ValidateDocumentsFolderPathField();
		}

		[IdentifiedTest("d0dcf3c8-cf82-4644-8871-a6e5b7d67af0")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_12()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
			ValidateDocumentsFolderPathField();
		}


		[IdentifiedTest("38032017-5701-4fd3-9294-f1507589c20f")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_NF_13()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			RelativityProviderModel model = CreateRelativityProviderModelWithNatives();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true; 

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
			ValidateDocumentsFolderPathField();
		}

		private void ValidateDocumentsForField()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFieldValidator()
				.ValidateWith(CreateDocumentHasNativesAndInRepositoryValidator());

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForFolderTree()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFolderTreeValidator()
				.ValidateWith(CreateDocumentHasNativesAndInRepositoryValidator());

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForRootWithFolderName()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootWithFolderNameValidator()
				.ValidateWith(CreateDocumentHasNativesAndInRepositoryValidator());

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForRoot()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootValidator()
				.ValidateWith(CreateDocumentHasNativesAndInRepositoryValidator());

			documentsValidator.Validate();
		}

		private void ValidateDocumentsFolderPathField()
		{
			DocumentsValidator validator = new PushDocumentsValidator(SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId())
				.ValidateWith(new DocumentFieldsValidator("Document Folder Path"));

			validator.Validate();
		}

		private DocumentHasNativesAndInRepositoryValidator CreateDocumentHasNativesAndInRepositoryValidator()
		{
			return new DocumentHasNativesAndInRepositoryValidator(NativesService, SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, true);
		}

	}
}
