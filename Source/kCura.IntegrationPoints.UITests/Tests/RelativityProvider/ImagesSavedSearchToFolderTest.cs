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
	[Feature.DataTransfer.IntegrationPoints.Sync.SavedSearch]
	[Category(TestCategory.EXPORT_IMAGES)]
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
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.SourceProductionName = $"SrcProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
			SourceContext.CreateAndRunProduction(model.SourceProductionName);

			model.DestinationProductionName = $"DestProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
			return model;
		}

		//RelativityProvider_TC_RTR_IMG_01
		//RelativityProvider_TC_RTR_IMG_02
		//RelativityProvider_TC_RTR_IMG_03
		//RelativityProvider_TC_RTR_IMG_04
		//RelativityProvider_TC_RTR_IMG_05
		//RelativityProvider_TC_RTR_IMG_06
		[IdentifiedTestCase("3c74b1de-c6ca-41e8-81a8-944bf856c0ff", RelativityProviderModel.OverwriteModeEnum.AppendOnly, false)]
		[IdentifiedTestCase("b2764d2b-d107-4b7a-98e4-238e8ff9f12a", RelativityProviderModel.OverwriteModeEnum.AppendOnly, true)]
		[IdentifiedTestCase("1f72fd1f-1109-4733-8859-4d431619a6f6", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, false)]
		[IdentifiedTestCase("661e5706-e658-4483-bf1b-4924e471c260", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, true)]
		[IdentifiedTestCase("9b2f2880-d329-4b7c-a1c1-5a09cf888072", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, false)]
		[IdentifiedTestCase("8ae832f6-830f-4380-b2f9-32974003dac3", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, true)]
		[RetryOnError]
		public void ShouldPushImages_WhenImagePrecedenceIsOriginalImages(RelativityProviderModel.OverwriteModeEnum overwrite, bool copyFilesToRepository)
		{
			//Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithImages();
			model.Overwrite = overwrite;
			model.ImagePrecedence = ImagePrecedence.OriginalImages;
			model.CopyFilesToRepository = copyFilesToRepository;

			if (overwrite.Equals(RelativityProviderModel.OverwriteModeEnum.OverlayOnly))
			{
				DestinationContext.ImportDocuments();
			}

			//Act
				IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			ValidateOriginalImages(copyFilesToRepository);
		}
		
		//RelativityProvider_TC_RTR_IMG_07
		//RelativityProvider_TC_RTR_IMG_08
		//RelativityProvider_TC_RTR_IMG_09
		//RelativityProvider_TC_RTR_IMG_10
		//RelativityProvider_TC_RTR_IMG_11
		//RelativityProvider_TC_RTR_IMG_12
		//RelativityProvider_TC_RTR_IMG_13
		[IdentifiedTestCase("83c8d913-3943-4821-94f6-6d3ad07117e0", RelativityProviderModel.OverwriteModeEnum.AppendOnly, false, false)]
		[IdentifiedTestCase("2214a1fb-9654-4982-af54-654c4d1350d4", RelativityProviderModel.OverwriteModeEnum.AppendOnly, true, false)]
		[IdentifiedTestCase("d07d4d84-291f-4896-ae31-3cdab325a111", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, false, false)]
		[IdentifiedTestCase("839d516a-5dab-4ff0-92de-af53fc9ac414", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, true, false)]
		[IdentifiedTestCase("1a6075b6-7949-4bbc-a14c-f1acbadb3b4c", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, false, false)]
		[IdentifiedTestCase("68331cd1-a2a6-4dca-a86b-45889f3ae8e8", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, true, false)]
		[IdentifiedTestCase("89d4dcae-bec8-4fe1-85a2-04d356326b30", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, true, true)]
		[RetryOnError]
		public void ShouldPushImages_WhenImagePrecedenceIsProducedImages(RelativityProviderModel.OverwriteModeEnum overwrite, bool copyFilesToRepository, bool includeOriginalImagesIfNotProduced )
		{
			// Arrange
			ImagesSavedSearchToFolderValidator validator = new ImagesSavedSearchToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = overwrite;
			model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			model.ImagePrecedence = ImagePrecedence.ProducedImages;
			model.IncludeOriginalImagesIfNotProduced = includeOriginalImagesIfNotProduced;
			model.CopyFilesToRepository = copyFilesToRepository;
			
			if (overwrite.Equals(RelativityProviderModel.OverwriteModeEnum.OverlayOnly))
			{
				DestinationContext.ImportDocuments();
				DestinationContext.CreateAndRunProduction(model.DestinationProductionName);
			}
			
			// Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			detailsPage.RunIntegrationPoint();
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
			ValidateProductionImages(copyFilesToRepository);
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
