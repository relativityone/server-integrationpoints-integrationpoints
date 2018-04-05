using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
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

		[Test]
		public void RelativityProvider_TC_RTR_IPS_1()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_2()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_3()
		{
			//Arrange
			DestinationContext.ImportDocuments();

			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_4()
		{
			//Arrange
			DestinationContext.ImportDocuments();

			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_5()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_6()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.OriginalImages;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateOriginalImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_7()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_8()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_9()
		{
			//Arrange
			DestinationContext.ImportDocuments();
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateAndRunProduction(model.DestinationProductionName);
			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_10()
		{
			//Arrange
			DestinationContext.ImportDocuments();
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateAndRunProduction(model.DestinationProductionName);
			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_11()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = false;

			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);
			Context.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_12()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = false;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);
			Context.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			validator.ValidateJobStatus(detailsPage, JobStatusChoices.JobHistoryCompleted);
			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_IPS_13()
		{
			//Arrange
			var validator = new SavedSearchToProductionSetValidator();
			var model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedenceEnum.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = true;
			model.CopyFilesToRepository = true;

			DestinationContext.CreateProductionSet("Import" + model.DestinationProductionName);
			Context.CreateAndRunProduction(model.SourceProductionName);

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
