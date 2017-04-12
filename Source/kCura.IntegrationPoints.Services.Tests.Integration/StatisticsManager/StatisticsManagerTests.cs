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
		public StatisticsManagerTests() : base($"Statistics_{Utils.FormatedDateTimeNow}")
		{
		}

		private TestCaseSettings _testCaseSettings;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var workspaceService = new WorkspaceService(new ImportHelper());
			_testCaseSettings = new TestCaseSettings();

			_testCaseSettings.DocumentsTestData = DocumentTestDataBuilder.BuildTestData();

			workspaceService.ImportData(WorkspaceArtifactId, _testCaseSettings.DocumentsTestData);

			_testCaseSettings.SavedSearchId = SavedSearch.CreateSavedSearch(WorkspaceArtifactId, "All documents");
			_testCaseSettings.ViewId = workspaceService.GetView(WorkspaceArtifactId, "Documents");
			_testCaseSettings.FolderId = _testCaseSettings.DocumentsTestData.Documents.Last().FolderId.Value;

			_testCaseSettings.ProductionId = workspaceService.CreateProduction(WorkspaceArtifactId, _testCaseSettings.SavedSearchId, "Production");
		}

		[TestCaseSource(nameof(_testCases))]
		public void ItShouldGetDocumentTotal(IStatisticsTestCase statisticsTestCase)
		{
			statisticsTestCase.Execute(Helper, WorkspaceArtifactId, _testCaseSettings);
		}

		private static IEnumerable<IStatisticsTestCase> _testCases = new IStatisticsTestCase[]
		{
			new GetDocumentsTotalForSavedSearch(),
			new GetImagesFileSizeForSavedSearch(),
			new GetImagesTotalForSavedSearch(),
			new GetNativesFileSizeForSavedSearch(),
			new GetNativesTotalForSavedSearch(),
			new GetDocumentsTotalForFolder(),
			new GetImagesFileSizeForFolder(),
			new GetImagesTotalForFolder(),
			new GetNativesFileSizeForFolder(),
			new GetNativesTotalForFolder(),
			new GetDocumentsTotalForProduction(),
			new GetImagesFileSizeForProduction(),
			new GetImagesTotalForProduction(),
			new GetNativesFileSizeForProduction(),
			new GetNativesTotalForProduction()
		};
	}
}