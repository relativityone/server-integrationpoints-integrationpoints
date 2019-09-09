using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.NUnitExtensions;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Tests.RelativityProvider;
using kCura.IntegrationPoints.UITests.Validation.RelativityProviderValidation;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.UITests.Tests.PerformanceBaseline
{
	[TestFixture, Explicit]
	[Category(TestCategory.PERFORMANCE_BASELINE)]
	public class PerformanceBaselineImagesTest : RelativityProviderTestsBase
	{
		private const string _WORKSPACE_NAME = "[Do Not Delete] [RIP Sync] Performance Images";
		private const string _SAVEDSEARCH = "All documents";

		[IdentifiedTest("0e76772e-03cb-4918-ac55-8fcdf1034655")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Images_Links_01() // we want to run each test two times to have more data; as long as we need to read result manually we use different name of each test
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
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, expectErrors: false);
		}

		[IdentifiedTest("c1543a0d-0630-4a23-8403-98f825d00fa4")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Images_Links_02() // we want to run each test two times to have more data; as long as we need to read result manually we use different name of each test
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
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, expectErrors: false);
		}

		[IdentifiedTest("512b6951-132f-4795-b8d0-792a632f4e82")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Images_Copy_01() // we want to run each test two times to have more data; as long as we need to read result manually we use different name of each test
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
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, expectErrors: false);
		}

		[IdentifiedTest("bf5afb96-4f52-4f42-9c12-7d0d4523425e")]
		[RetryOnError]
		public void PerfromanceBaseline_100k_Images_Copy_02() // we want to run each test two times to have more data; as long as we need to read result manually we use different name of each test
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
			PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();
			validator.ValidateSummaryPage(generalProperties, model, SourceContext, DestinationContext, expectErrors: false);
		}

		private RelativityProviderModel CreateRelativityProviderModelWithImages()
		{
			SourceContext.WorkspaceName = _WORKSPACE_NAME;

			var model = new RelativityProviderModel(TestContext.CurrentContext.Test.Name)
			{
				Source = RelativityProviderModel.SourceTypeEnum.SavedSearch,
				RelativityInstance = "This Instance",
				DestinationWorkspace = $"{DestinationContext.WorkspaceName} - {DestinationContext.WorkspaceId}",
				SavedSearch = _SAVEDSEARCH,
				CopyImages = true,
				CreateSavedSearch = false
			};
			return model;
		}
	}
}
