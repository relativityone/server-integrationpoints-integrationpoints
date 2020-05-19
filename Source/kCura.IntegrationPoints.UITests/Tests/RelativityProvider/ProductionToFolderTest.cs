using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
	public class ProductionToFolderTest : RelativityProviderTestsBase
	{
		private RelativityProviderModel CreateRelativityProviderModelWithProduction()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.Production,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				CreateSavedSearch = false,
				CopyImages = true,
				SourceProductionName = $"Prod_{DateTime.Now:yyyy-MM-dd_HH:mm:ss}"
			};

			return model;
		}
		
		[Category(TestCategory.SMOKE)]
		[IdentifiedTest("d76ebfc3-cfc6-4afa-be16-bf3154775bb5")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_PTF_1()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.CopyFilesToRepository = false;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false, model);
		}

		[IdentifiedTest("c932c915-890e-4b32-b859-46415a56bb2e")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_PTF_2()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOnly;
			model.CopyFilesToRepository = true;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(true, model);
		}

		[IdentifiedTest("1fbc8f0c-39d3-437c-b4b0-433add7f6a0d")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_PTF_3()
		{
			//Not all documents have images, so production would add missing ones
			DestinationContext.ImportDocuments();

			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = false;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false, model);
		}

		[IdentifiedTest("fbb5be58-1c53-4cac-aa0a-69e9e2f4222a")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_PTF_4()
		{
			//Not all documents have images, so production would add missing ones
			DestinationContext.ImportDocuments();

			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.OverlayOnly;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = true;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(true, model);
		}

		[IdentifiedTest("9cf1416f-8698-4f1d-b127-f119722a8877")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_PTF_5()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = false;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false, model);
		}

		[IdentifiedTest("4fca11e2-dd5c-442c-b890-3db3204c06ca")]
		[RetryOnError]
		public void RelativityProvider_TC_RTR_PTF_6()
		{
			//Arrange
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = RelativityProviderModel.OverwriteModeEnum.AppendOverlay;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.CopyFilesToRepository = true;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(true, model);
		}

		private void ValidateProductionImagesAndDocumentSource(bool expectInRepository, RelativityProviderModel model)
		{
			IRelativityObjectManager objectManager = ObjectManagerFactory.CreateRelativityObjectManager(DestinationContext.GetWorkspaceId());

			DocumentsValidator validator = CreateDocumentsEmptyValidator()
				.ValidateWith(new DocumentFieldsValidator())
				.ValidateWith(new DocumentHasImagesValidator(true))
				.ValidateWith(new DocumentImagesValidator(ImageService, DestinationContext.GetWorkspaceId(), expectInRepository))
				.ValidateWith(new DocumentSourceJobNameValidator(objectManager, model.Name));

			validator.Validate();
		}
	}
}
