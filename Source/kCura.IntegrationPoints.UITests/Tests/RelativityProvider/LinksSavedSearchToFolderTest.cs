using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
	public class LinksSavedSearchToFolderTest: RelativityProviderTestsBase
	{
		private static readonly List<Tuple<string, string>> DefaultFieldsMapping = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("Control Number", "Control Number"),
			new Tuple<string, string>("Extracted Text", "Extracted Text"),
			new Tuple<string, string>("Title", "Title")
		};

		private RelativityProviderModel CreateRelativityProviderModelWithLinks()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				SavedSearch = "All documents",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly,
				FieldMapping = DefaultFieldsMapping
			};

			return model;
		}

		[IdentifiedTest("5e6097ba-76ae-488a-a60a-47d2eaa12f31")]
		[RetryOnError]
		[Category(TestCategory.SMOKE)]
		public void RelativityProvider_TC_RTR_LO_01()
		{
			//Arrange
			LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRoot();
		}

		[IdentifiedTest("4b2768a9-b44b-431e-bd26-cdb6d604bb47")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_02()
		{
			//Arrange
			LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[IdentifiedTest("09a3fe30-360b-40d1-867d-c56e5751c452")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_03()
		{
			//Arrange
			LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[IdentifiedTest("32e2afff-cef3-4592-a079-f7f8d49090e9")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_04()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[IdentifiedTest("9137f4ad-8f73-4a7c-9099-35c386dc11c9")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_05()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[IdentifiedTest("f9d043f0-c826-4f22-83b1-bae372fb1c51")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_06()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
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
		}

		[IdentifiedTest("29fa8085-768d-4e0b-a862-e66d5b30cc89")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_07()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[IdentifiedTest("cf864914-f6ff-437a-99a3-c2db2658851a")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_08()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			//validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[IdentifiedTest("b688c8fb-9924-42de-8429-f6b1855e9efb")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_09()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.No;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			//validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[IdentifiedTest("f1e1a5ad-8365-4f4d-b010-e94e5d0a031d")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_10()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			//validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[IdentifiedTest("8fab3365-e97d-42ad-be82-892db2774f32")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_11()
		{
			// Arrange
			DestinationContext.ImportDocumentsToRoot();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			//validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[IdentifiedTest("00b7c027-df10-4918-8ee1-a07f45634540")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_12()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromField;
			model.MoveExistingDocuments = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			//validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[IdentifiedTest("49213ab2-54ce-4a91-b1a6-71acd963684e")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_LO_13()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			//LinksSavedSearchToFolderValidator validator = new LinksSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithLinks();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.UseFolderPathInformation = RelativityProviderModel.UseFolderPathInformationEnum.ReadFromFolderTree;
			model.MoveExistingDocuments = true; 

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			//validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		private void ValidateDocumentsForField()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFieldValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForFolderTree()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFolderTreeValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForRootWithFolderName()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootWithFolderNameValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForRoot()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, SourceContext.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}
	}
}
