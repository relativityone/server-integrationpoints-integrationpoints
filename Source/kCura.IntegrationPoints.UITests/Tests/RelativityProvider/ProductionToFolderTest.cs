using System;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Category(TestCategory.SMOKE)]
	public class ProductionToFolderTest : RelativityProviderTestsBase
	{
		public override void TearDown()
		{
			DestinationContext?.TearDown();
		}

		private RelativityProviderModel CreateRelativityProviderModelWithProduction()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.Production,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CreateSavedSearch = false,
				CopyImages = true,
				ProductionName = $"Prod_{DateTime.Now:yyyy-MM-dd_HH:mm:ss}"
			};

			return model;
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTF_1()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.CopyFilesToRepository = false;

			Context.CreateProduction(model.ProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTF_2()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(true);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTF_3()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = false;

			Context.CreateProduction(model.ProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTF_4()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(true);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTF_5()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = false;

			Context.CreateProduction(model.ProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false);
		}

		[Test]
		public void RelativityProvider_TC_RTR_PTF_6()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = true;

			Context.CreateProduction(model.ProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(true);
		}

		private void ValidateProductionImagesAndDocumentSource(bool expectInRepository)
		{
			DocumentsValidator validator = CreateDocumentsEmptyValidator()
				.ValidateWith(new DocumentHasImagesValidator(true))
				.ValidateWith(new DocumentImagesValidator(ImageService, DestinationContext.GetWorkspaceId(), expectInRepository));

			validator.Validate();
		}
	}
}
