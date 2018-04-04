﻿using System;
using System.Net;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
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
			model.ProductionName = $"Production {DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")}";
			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_1()
		{
			//Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_2()
		{
			//Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_3()
		{
			//Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}


		[Test]
		public void RelativityProvider_TC_RTR_IMG_4()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_5()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_6()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			validator.ValidateSummaryPage(generalProperties, model, Context, DestinationContext, false);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository));
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_7()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateProduction(model.ProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);

			ValidateProductionImages(false);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_8()
		{
			// Arrange

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);

			ValidateProductionImages(true);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_9()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateProduction(model.ProductionName);
			DestinationContext.CreateProduction(model.ProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(false);
		}


		[Test]
		public void RelativityProvider_TC_RTR_IMG_10()
		{
			// Arrange
			DestinationContext.ImportDocuments();

			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);
			DestinationContext.CreateProduction(model.ProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(true);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_11()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateProduction(model.ProductionName);
			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(false);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IMG_12()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);

			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			Assert.AreEqual("Saved Search: All Documents", generalProperties.Properties["Source Details:"]);
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(true);
		}


		[Test]
		public void RelativityProvider_TC_RTR_IMG_13()
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = true;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);

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
