using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Validators;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.RelativityProvider
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync.ProductionPush]
	[Category(TestCategory.RIP_OLD)]
	public class ProductionToFolderTest : RelativityProviderTestsBase
	{
		private readonly string _sourceProductionName = $"SrcProd_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

		protected override Task SuiteSpecificOneTimeSetup()
		{
			SourceContext.CreateProductionAndImportData(_sourceProductionName);
			return Task.CompletedTask;
		}

		protected override Task SuiteSpecificSetup() => Task.CompletedTask;
		protected override Task SuiteSpecificTearDown() => Task.CompletedTask;

		private RelativityProviderModel CreateRelativityProviderModelWithProduction()
		{
			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.Production,
				SourceProductionName = _sourceProductionName,
				
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				Location = RelativityProviderModel.LocationEnum.Folder,
				
				CreateSavedSearch = false,

				CopyImages = true
			};

			return model;
		}

		[Category(TestCategory.SMOKE)]
		[IdentifiedTestCase("4f0c899d-202d-4b37-b63c-94e0b6bbdcd8", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, false)]
		public void ShouldPushImages_WhenSourceProductionAndDestinationFolder(
			RelativityProviderModel.OverwriteModeEnum overwrite, bool copyFilesToRepository)
		{
			var validator = new ImagesProductionToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = overwrite;
			model.CopyFilesToRepository = copyFilesToRepository;
			if (!overwrite.Equals(RelativityProviderModel.OverwriteModeEnum.AppendOnly))
			{
				model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			}

			if (overwrite.Equals(RelativityProviderModel.OverwriteModeEnum.OverlayOnly))
			{
				DestinationContext.ImportDocuments();
			}

			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			detailsPage.RunIntegrationPoint();

			// Assert
			WaitForJobToFinishAndValidateCompletedStatus(detailsPage);

			ValidateProductionImagesAndDocumentSource(false, model);
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
		}

		//RelativityProvider_TC_RTR_PTF_1
		[IdentifiedTestCase("d76ebfc3-cfc6-4afa-be16-bf3154775bb5", RelativityProviderModel.OverwriteModeEnum.AppendOnly, false)]
		//RelativityProvider_TC_RTR_PTF_2
		[IdentifiedTestCase("c932c915-890e-4b32-b859-46415a56bb2e", RelativityProviderModel.OverwriteModeEnum.AppendOnly, true)]
		//RelativityProvider_TC_RTR_PTF_3
		[IdentifiedTestCase("1fbc8f0c-39d3-437c-b4b0-433add7f6a0d", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, false)]
		//RelativityProvider_TC_RTR_PTF_4
		[IdentifiedTestCase("fbb5be58-1c53-4cac-aa0a-69e9e2f4222a", RelativityProviderModel.OverwriteModeEnum.OverlayOnly, true)]
		//RelativityProvider_TC_RTR_PTF_5
		[IdentifiedTestCase("9cf1416f-8698-4f1d-b127-f119722a8877", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, false)]
		//RelativityProvider_TC_RTR_PTF_6
		[IdentifiedTestCase("4fca11e2-dd5c-442c-b890-3db3204c06ca", RelativityProviderModel.OverwriteModeEnum.AppendOverlay, true)]
		[RetryOnError]
		public void ShouldDisplayCorrectSummaryPage_WhenSourceProductionImagesAndDestinationFolder(
			RelativityProviderModel.OverwriteModeEnum overwrite, bool copyFilesToRepository)
		{
			//Arrange
			var validator = new ImagesProductionToFolderValidator();
			RelativityProviderModel model = CreateRelativityProviderModelWithProduction();
			model.Overwrite = overwrite;
			model.CopyFilesToRepository = copyFilesToRepository;
			
			if (!overwrite.Equals( RelativityProviderModel.OverwriteModeEnum.AppendOnly))
			{
				model.MultiSelectFieldOverlay = RelativityProviderModel.MultiSelectFieldOverlayBehaviorEnum.UseFieldSettings;
			}

			SourceContext.CreateProductionAndImportData(model.SourceProductionName);
			
			//Act
			IntegrationPointDetailsPage detailsPage = PointsAction.CreateNewRelativityProviderIntegrationPoint(model);
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

			// Assert
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, false);
		}


		#region Validators

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

		#endregion
	}
}
