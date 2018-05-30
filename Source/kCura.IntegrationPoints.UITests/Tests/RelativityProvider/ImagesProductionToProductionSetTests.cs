using System;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	public class ImagesProductionToProductionSetTests : RelativityProviderTestsBase
	{

		private RelativityProviderModel CreateModel()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.Production,
				SourceProductionName = $"SrcProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",

				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				Location = RelativityProviderModel.LocationEnum.ProductionSet,
				DestinationProductionName = $"DestProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
				CreateSavedSearch = false,

				CopyImages = true,
			};
			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTP_01()
		{
			//Arrange
			RelativityProviderModel model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTP_02()
		{
			//Arrange
			RelativityProviderModel model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTP_03()
		{
			//Arrange
			RelativityProviderModel model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);

			DestinationContext.ImportDocuments();
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateOverlayProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTP_04()
		{
			//Arrange
			RelativityProviderModel model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);

			DestinationContext.ImportDocuments();
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateOverlayProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTP_05()
		{
			//Arrange
			RelativityProviderModel model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.CopyFilesToRepository = false;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTP_06()
		{
			//Arrange
			RelativityProviderModel model = CreateModel();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.CopyFilesToRepository = true;

			Context.CreateAndRunProduction(model.SourceProductionName);
			DestinationContext.CreateProductionSet(model.DestinationProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImages(model.GetValueOrDefault(x => x.CopyFilesToRepository), model);
		}

		private void ValidateProductionImages(bool expectInRepository, RelativityProviderModel model)
		{
			ValidateIncludingHasImages(CreateDocumentsEmptyValidator(), expectInRepository, model);
		}

		private void ValidateOverlayProductionImages(bool expectInRepository, RelativityProviderModel model)
		{
			ValidateWithoutHasImages(CreateDocumentsEmptyValidator(), expectInRepository, model);
		}

		private void ValidateIncludingHasImages(DocumentsValidator documentsValidator, bool expectInRepository, RelativityProviderModel model)
		{
			documentsValidator
				.ValidateWith(new DocumentHasImagesValidator(null))
				.ValidateWith(new DocumentProductionImagesValidator(ProductionImageService, DestinationContext.GetWorkspaceId(), expectInRepository))
				.ValidateWith(CreateDocumentSourceJobNameValidator(model))
				.Validate();
		}

		private void ValidateWithoutHasImages(DocumentsValidator documentsValidator, bool expectInRepository, RelativityProviderModel model)
		{
			documentsValidator
				.ValidateWith(new DocumentProductionImagesValidator(ProductionImageService, DestinationContext.GetWorkspaceId(), expectInRepository))
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