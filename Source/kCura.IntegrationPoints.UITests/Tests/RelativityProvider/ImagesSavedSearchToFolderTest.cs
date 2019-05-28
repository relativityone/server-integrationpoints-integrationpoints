using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;
using System;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
	public class ImagesSavedSearchToFolderTest : RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModelWithImages()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyImages = true,
				CreateSavedSearch = false
			};
			return model;
		}

		private RelativityProviderModel CreateRelativityProviderModelWithProduction()
		{
			var model = CreateRelativityProviderModelWithImages();
			model.SourceProductionName = $"Production {DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}";
			return model;
		}

		[IdentifiedTest("3c74b1de-c6ca-41e8-81a8-944bf856c0ff")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_01()
		{
			//Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = false;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[IdentifiedTest("b2764d2b-d107-4b7a-98e4-238e8ff9f12a")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_02()
		{
			//Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			
			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[IdentifiedTest("1f72fd1f-1109-4733-8859-4d431619a6f6")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_03()
		{
			//Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}


		[IdentifiedTest("661e5706-e658-4483-bf1b-4924e471c260")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_04()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[IdentifiedTest("9b2f2880-d329-4b7c-a1c1-5a09cf888072")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_05()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[IdentifiedTest("8ae832f6-830f-4380-b2f9-32974003dac3")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_06()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[IdentifiedTest("83c8d913-3943-4821-94f6-6d3ad07117e0")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_07()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);

			ValidateProductionImages(false);
		}

		[IdentifiedTest("2214a1fb-9654-4982-af54-654c4d1350d4")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_08()
		{
			// Arrange

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);

			ValidateProductionImages(true);
		}

		[IdentifiedTest("d07d4d84-291f-4896-ae31-3cdab325a111")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_09()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateAndRunProduction(model.SourceProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(false);
		}


		[IdentifiedTest("839d516a-5dab-4ff0-92de-af53fc9ac414")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_10()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateAndRunProduction(model.SourceProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(true);
		}

		[IdentifiedTest("1a6075b6-7949-4bbc-a14c-f1acbadb3b4c")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_11()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);
			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(false);
		}

		[IdentifiedTest("68331cd1-a2a6-4dca-a86b-45889f3ae8e8")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_12()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(true);
		}


		[IdentifiedTest("89d4dcae-bec8-4fe1-85a2-04d356326b30")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IMG_13()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = true;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(true);
		}

		private void ValidateOriginalImages(bool expectInRepository)
		{
			DocumentsValidator documentsValidator = CreateOnlyDocumentsWithImagesValidator()
				.ValidateWith(new DocumentImagesValidator(ImageService, DestinationContext.GetWorkspaceId(), expectInRepository));

			documentsValidator.Validate();
		}

		private void ValidateProductionImages(bool expectInRepository)
		{
			DocumentsValidator documentsValidator = CreateDocumentsEmptyValidator()
				.ValidateWith(new DocumentImagesValidator(ImageService, DestinationContext.GetWorkspaceId(), expectInRepository));

			documentsValidator.Validate();
		}
	}

}
