using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager
{
	public class StatisticsManagerTests : SourceProviderTemplate
	{
		private TestCaseSettings _testCaseSettings;

		private static IEnumerable<IStatisticsTestCase> _testCases = new IStatisticsTestCase[]
		{
			new GetDocumentsTotalForSavedSearch(),
			new GetImagesFileSizeForSavedSearch(),
			new GetImagesTotalForSavedSearch(),
			new GetNativesFileSizeForSavedSearch(),
			new GetNativesTotalForSavedSearch(),
			new GetDocumentsTotalForFolder(),
			new GetImagesTotalForFolder(),
			new GetNativesTotalForFolder(),
			new GetDocumentsTotalForProduction(),
			new GetNativesTotalForProduction(),
			new GetImagesFileSizeForProduction(),
			new GetImagesTotalForProduction(),
			new GetNativesFileSizeForProduction(),
			new GetImagesFileSizeForFolder(),
			new GetNativesFileSizeForFolder()
		};

		public StatisticsManagerTests() : base($"Statistics_{Utils.FormattedDateTimeNow}")
		{
		}
		
		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var workspaceService = new WorkspaceService(new ImportHelper());
			FolderWithDocumentsIdRetriever folderWithDocumentsIdRetriever = Container.Resolve<FolderWithDocumentsIdRetriever>();
			_testCaseSettings = new TestCaseSettings();

			_testCaseSettings.DocumentsTestData = DocumentTestDataBuilder.BuildTestData();

			workspaceService.TryImportData(WorkspaceArtifactId, _testCaseSettings.DocumentsTestData);

			folderWithDocumentsIdRetriever.UpdateFolderIdsAsync(WorkspaceArtifactId, _testCaseSettings.DocumentsTestData.Documents)
				.GetAwaiter()
				.GetResult();

			_testCaseSettings.SavedSearchId = SavedSearch.CreateSavedSearch(WorkspaceArtifactId, "All documents");
			_testCaseSettings.ViewId = workspaceService.GetView(WorkspaceArtifactId, "Documents");
			_testCaseSettings.FolderId = _testCaseSettings.DocumentsTestData.Documents.Last().FolderId.GetValueOrDefault();

			_testCaseSettings.ProductionId =
				workspaceService.CreateAndRunProduction(WorkspaceArtifactId, _testCaseSettings.SavedSearchId, "Production");
		}

		[TestCaseSource(nameof(_testCases))]
		public void ItShouldGetDocumentTotal(IStatisticsTestCase statisticsTestCase)
		{
			statisticsTestCase.Execute(Helper, WorkspaceArtifactId, _testCaseSettings);
		}
	}
}