using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
    public class LinksSavedSearchToFolderTest: RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModelWithLinks()
		{
			var model = new RelativityProviderModel(NUnit.Framework.TestContext.CurrentContext.Test.Name);
			model.Source = RelativityProviderModel.SourceTypeEnum.SavedSearch;
			model.RelativityInstance = "This Instance";
			model.DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}";
			model.CopyNativeFiles = RelativityProviderModel.CopyNativeFilesEnum.LinksOnly;
			return model;
		}

		[Test]
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
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRoot();
		}

		[Test]
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
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
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
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[Test]
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
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[Test]
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
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[Test]
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

		[Test]
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
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
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
			//validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		[Test]
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
			//validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[Test]
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
			//validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[Test]
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
			//validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForRootWithFolderName();
		}

		[Test]
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
			//validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForField();
		}

		[Test]
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
			//validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);

			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateDocumentsForFolderTree();
		}

		private void ValidateDocumentsForField()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFieldValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForFolderTree()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForFolderTreeValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForRootWithFolderName()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootWithFolderNameValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}

		private void ValidateDocumentsForRoot()
		{
			DocumentsValidator documentsValidator = CreateDocumentsForRootValidator()
				.ValidateWith(new DocumentHasNativesAndInRepositoryValidator(NativesService, Context.GetWorkspaceId(), DestinationContext.GetWorkspaceId(), true, false));

			documentsValidator.Validate();
		}
	}
}
