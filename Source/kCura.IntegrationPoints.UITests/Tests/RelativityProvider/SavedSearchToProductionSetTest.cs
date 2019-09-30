using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;
using System;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
	public class SavedSearchToProductionSetTest : RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateModel()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CopyImages = true,
				Location = RelativityProviderModel.LocationEnum.ProductionSet,
				SourceProductionName = $"SrcProd_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}",
				DestinationProductionName = $"DestProd_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}",
				CreateSavedSearch = false
			};
			return model;
		}

		[IdentifiedTest("e5125237-605b-4519-9ba0-3fbfcb1e2ab7")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_01()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("d781e140-c939-4d1d-a421-24263d7de424")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_02()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("4d2fe595-6924-46c7-aa64-e9ffa593765f")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_03()
		{
			//Arrange
			DestinationContext.ImportDocuments();

			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("4d9b57e7-1a1e-4029-a04e-4dc651227211")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_IPS_04()
		{
			//Arrange
			DestinationContext.ImportDocuments();

			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("aa778fbc-17fd-4b35-a12e-5818dc650623")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_05()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("2c710c86-2341-4107-9dae-431989faa52c")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_06()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("80ae784e-7a5c-436e-9e61-735e39a61e2b")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_07()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("8aac9cf3-022e-4343-a822-9d3b4f033f68")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_08()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("95ad301b-d50b-4760-8152-a4ed63a52bc4")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_09()
		{
			//Arrange
			DestinationContext.ImportDocuments();
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateAndRunProduction(model.DestinationProductionName);
			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("01b5cadb-eb8d-4abb-8398-f69d8779a7aa")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_10()
		{
			//Arrange
			DestinationContext.ImportDocuments();
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateAndRunProduction(model.DestinationProductionName);
			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("f73d2ddd-b8a8-4e14-b596-e8174d3b6b01")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_11()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);
			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("0ed74f14-e9a1-4f2b-a231-8b0a33124ab2")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_12()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);
			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[IdentifiedTest("7e9533e9-c6b9-4c02-8bac-c49fceb70515")]
		[RetryOnError]
		[Ignore("REL-291041")]
		public void RelativityProvider_TC_RTR_IPS_13()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = true;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);
			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}


		private void ValidateOriginalImages(bool expectInRepository, RelativityProviderModel model)
		{
			Validate(CreateOnlyDocumentsWithImagesValidator(), expectInRepository, model);
		}

		private void ValidateProductionImages(bool expectInRepository, RelativityProviderModel model)
		{
			Validate(CreateDocumentsEmptyValidator(), expectInRepository, model);
		}

		private void Validate(DocumentsValidator documentsValidator, bool expectInRepository, RelativityProviderModel model)
		{
			documentsValidator
				.ValidateWith(new DocumentFieldsValidator())
				.ValidateWith(new DocumentHasImagesValidator(true))
				.ValidateWith(new DocumentImagesValidator(ImageService, DestinationContext.GetWorkspaceId(), expectInRepository))
				.ValidateWith(CreateDocumentSourceJobNameValidator(model))
				.Validate();
		}

		private IDocumentValidator CreateDocumentSourceJobNameValidator(RelativityProviderModel model)
		{
			IRelativityObjectManager objectManager = ObjectManagerFactory.CreateRelativityObjectManager(DestinationContext.GetWorkspaceId());
			return new DocumentSourceJobNameValidator(objectManager, model.Name);
		}
	}
}
