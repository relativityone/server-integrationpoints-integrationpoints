using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync.ProductionPush]
	[Category(TestCategory.EXPORT_TO_RELATIVITY)]
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
		//RelativityProvider_TC_RTR_PTP_01
		[IdentifiedTestCase("23c8242c-20cb-4c89-ad24-1f50bf84cb0c", RelativityProviderModel.OverwriteModeEnum.AppendOnly, false)]
		//RelativityProvider_TC_RTR_PTP_02
		[IdentifiedTestCase("38e8e0fc-5315-4102-9964-5c901c564f3d", RelativityProviderModel.OverwriteModeEnum.AppendOnly, true)]
		//RelativityProvider_TC_RTR_PTP_03
		[IdentifiedTestCase("8aa51998-dbff-4c5f-a2af-88de58b287a6", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, false)]
		//RelativityProvider_TC_RTR_PTP_04
		[IdentifiedTestCase("a313623e-0c79-4caf-bfb0-7029682d9d1d", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, true)]
		//RelativityProvider_TC_RTR_PTP_05
		[IdentifiedTestCase("185de8df-cf4f-4286-975f-ed929c387eea", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, true)]
		//RelativityProvider_TC_RTR_PTP_06
		[IdentifiedTestCase("b3e2c155-ac41-43ee-b45d-69d43d6af685", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, false)]
		[RetryOnError]
		public void ItShouldPushImagesFromProductionToProduction(RelativityProviderModel.OverwriteModeEnum overwrite, bool copyFilesToRepository)
		{
			//Arrange
			ImagesProductionToProductionSetValidator validator = new ImagesProductionToProductionSetValidator();
			RelativityProviderModel model = CreateModel();
			model.Overwrite = overwrite;
			model.CopyFilesToRepository = copyFilesToRepository;

			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			if (overwrite.Equals(RelativityProviderModel.OverwriteModeEnum.OverlayOnly))
			{
				DestinationContext.ImportDocuments();
				DestinationContext.CreateAndRunProduction(model.DestinationProductionName);
			}
			else
			{
				DestinationContext.CreateProductionSet(model.DestinationProductionName);
			}

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			
			if (overwrite.Equals(RelativityProviderModel.OverwriteModeEnum.OverlayOnly))
			{
				ValidateOverlayProductionImages(copyFilesToRepository, model);
			}
			else
			{
				ValidateProductionImages(copyFilesToRepository, model);
			}
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