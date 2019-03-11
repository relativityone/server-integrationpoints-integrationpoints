using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
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
			new GetNativesTotalForProduction()
		};

		private static IEnumerable<IStatisticsTestCase> _quarantinedBrokenTestCases = new IStatisticsTestCase[]
		{
			new GetImagesFileSizeForFolder(), //TODO: Correct this test case. It's checking non-existent folder with ID = 0
			new GetNativesFileSizeForFolder(), //TODO: Correct this test case. It's checking non-existent folder with ID = 0
		};

		private static IEnumerable<IStatisticsTestCase> _quarantinedDueToREL301226 = new IStatisticsTestCase[]
		{
			new GetImagesFileSizeForProduction(),
			new GetImagesTotalForProduction(),
			new GetNativesFileSizeForProduction()
		};
		
		public StatisticsManagerTests() : base($"Statistics_{Utils.FormattedDateTimeNow}")
		{
		}
		
		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var workspaceService = new WorkspaceService(new ImportHelper());
			_testCaseSettings = new TestCaseSettings();

			_testCaseSettings.DocumentsTestData = DocumentTestDataBuilder.BuildTestData();

			workspaceService.ImportData(WorkspaceArtifactId, _testCaseSettings.DocumentsTestData);

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

		[TestCaseSource(nameof(_quarantinedBrokenTestCases))]
		[TestInQuarantine(TestQuarantineState.FailsContinuously)]
		public void ItShouldGetDocumentTotalQuarantinedBrokentests(IStatisticsTestCase statisticsTestCase)
		{
			statisticsTestCase.Execute(Helper, WorkspaceArtifactId, _testCaseSettings);
		}

		[TestCaseSource(nameof(_quarantinedDueToREL301226))]
		[TestInQuarantine(TestQuarantineState.DetectsDefectInExternalDependency, "REL-301226")]
		public void ItShouldGetDocumentTotalQuarantinedDueToREL301226(IStatisticsTestCase statisticsTestCase)
		{
			statisticsTestCase.Execute(Helper, WorkspaceArtifactId, _testCaseSettings);
		}
	}
}