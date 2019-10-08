using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.StatisticsManager
{
	[Feature.DataTransfer.IntegrationPoints]
	public class StatisticsManagerTests : SourceProviderTemplate
	{
		private TestCaseSettings _testCaseSettings;

		private static IEnumerable<TestCaseData> _testCases = new TestCaseData[]
		{
			new TestCaseData (new GetDocumentsTotalForSavedSearch()).WithId("C5AB2A48-23E4-468F-8E48-D918CFA23020"),
			new TestCaseData (new GetImagesFileSizeForSavedSearch()).WithId("AA40AEA0-C481-425C-B7F7-AE0D22DA820F"),
			new TestCaseData (new GetImagesTotalForSavedSearch()).WithId("945BCC45-56F3-4D76-9C1F-38B59512CEE4"),
			new TestCaseData (new GetNativesFileSizeForSavedSearch()).WithId("9EDD9E10-A90A-473B-B9BD-CB49BE83CB45"),
			new TestCaseData (new GetNativesTotalForSavedSearch()).WithId("56EDBF46-40F6-43BD-9E36-3DF1A78FBA1A"),
			new TestCaseData (new GetDocumentsTotalForFolder()).WithId("E6F4DA03-1E86-4A2A-9BAC-F6E7223AE149"),
			new TestCaseData (new GetImagesTotalForFolder()).WithId("8C52F86F-C3E4-4DC2-A821-6E2E0347F30F"),
			new TestCaseData (new GetNativesTotalForFolder()).WithId("5E63B48C-FD48-40F1-AB79-B9422468EA3C"),
			new TestCaseData (new GetDocumentsTotalForProduction()).WithId("B13EDEEE-0F5D-4CEB-977E-26F1B0CFDA51"),
			new TestCaseData (new GetNativesTotalForProduction()).WithId("F0AA5856-9F3C-40AC-9C21-A529F7970173"),
			new TestCaseData (new GetImagesFileSizeForProduction()).WithId("255249E8-879B-4C57-AC85-8A53ABFD50A8"),
			new TestCaseData (new GetImagesTotalForProduction()).WithId("F4F3E233-CA27-4988-A206-51ED11158E86"),
			new TestCaseData (new GetNativesFileSizeForProduction()).WithId("7B2A7F99-8B0C-41F2-A654-E01C01BA3BB7"),
			new TestCaseData (new GetImagesFileSizeForFolder()).WithId("9E3424FC-6ADA-4E26-9246-87E3AAC5A5DC"),
			new TestCaseData (new GetNativesFileSizeForFolder()).WithId("0FBBDC5E-3BD0-40EF-A60C-A2A5D254F058")
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

			_testCaseSettings.ProductionId = workspaceService
				.CreateAndRunProduction(WorkspaceArtifactId, _testCaseSettings.SavedSearchId, "Production")
				.ProductionArtifactID;
		}

		[TestCaseSource(nameof(_testCases))]
		public void ItShouldGetDocumentTotal(IStatisticsTestCase statisticsTestCase)
		{
			statisticsTestCase.Execute(Helper, WorkspaceArtifactId, _testCaseSettings);
		}
	}
}