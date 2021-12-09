using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Configuration;
using Relativity.Testing.Identification;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using NUnit.Framework;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Toggles;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	[TestFixture]
	internal sealed class DocumentGoldFlowTests : SystemTest
	{
		private GoldFlowTestSuite _goldFlowTestSuite;
		private readonly Dataset _dataset;

		public DocumentGoldFlowTests()
		{
			_dataset = Dataset.NativesAndExtractedText;
		}

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);
			await _goldFlowTestSuite.ImportDocumentsAsync(DataTableFactory.CreateImportDataTable(_dataset, true)).ConfigureAwait(false);
		}

		[IdentifiedTest("25b723da-82fe-4f56-ae9f-4a8b2a4d60f4")]
		[TestType.MainFlow]
		[TestCase(true)]
		[TestCase(false)]
		public async Task SyncJob_Should_SyncDocuments(bool enableKeplerizedImportAPIToggle)
		{
			// Arrange
			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);
			await goldFlowTestRun.ToggleProvider.SetAsync<EnableKeplerizedImportAPIToggle>(enableKeplerizedImportAPIToggle);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalDocumentCount).ConfigureAwait(false);
		}
		
		[IdentifiedTest("4be91e77-327d-41d6-afcc-a9d1090f0b04")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_SyncDocuments_WithCustomRDOs()
		{
			// Arrange
			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(
				async (sourceWorkspace, destinationWorkspace, configuration) =>
				{
					await Environment.InstallCustomHelperAppAsync(sourceWorkspace.ArtifactID).ConfigureAwait(false);
					await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

					configuration.JobHistory = CustomAppGuids.JobHistory;
					configuration.JobHistoryError = CustomAppGuids.JobHistoryError;
					configuration.DestinationWorkspace = CustomAppGuids.DestinationWorkspace;

					configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory,
							sourceWorkspace.ArtifactID, jobHistoryTypeGuid: CustomAppGuids.JobHistory.TypeGuid)
						.ConfigureAwait(false);

				}).ConfigureAwait(false);
		
			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);
		
			// Assert
			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalDocumentCount, CustomAppGuids.JobHistory.TypeGuid).ConfigureAwait(false);
		}

		[IdentifiedTest("e4451454-ea17-4d0e-b45a-a2c43ad35add")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_RetryDocuments()
		{
			// Arrange
			int jobHistoryToRetryId = -1, destinationWorkspaceId = -1;
			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(async (sourceWorkspace, destinationWorkspace, configuration) =>
			{
					destinationWorkspaceId = destinationWorkspace.ArtifactID;
					await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

					jobHistoryToRetryId = await Rdos.CreateJobHistoryInstanceAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID)
						.ConfigureAwait(false);
					configuration.JobHistoryToRetryId = jobHistoryToRetryId;

				}).ConfigureAwait(false);

			const int numberOfTaggedDocuments = 2;
			await Rdos.TagDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistoryToRetryId, numberOfTaggedDocuments).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _dataset.TotalDocumentCount - numberOfTaggedDocuments, _dataset.TotalDocumentCount - numberOfTaggedDocuments).ConfigureAwait(false);

			IList<string> sourceWorkspaceDocuments = await Rdos
				.QueryDocumentNamesAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, $"NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{jobHistoryToRetryId}])")
				.ConfigureAwait(false);

			IList<string> destinationWorkspaceDocuments = await Rdos
				.QueryDocumentNamesAsync(ServiceFactory, destinationWorkspaceId, "")
				.ConfigureAwait(false);

			goldFlowTestRun.AssertDocuments(sourceWorkspaceDocuments.ToArray(), destinationWorkspaceDocuments.ToArray());
		}

		private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
		{
			configuration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None;
			configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
			configuration.FolderPathSourceFieldName = "Document Folder Path";
			configuration.ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles;
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			configuration.MoveExistingDocuments = false;

			IEnumerable<FieldMap> identifierMapping = await GetDocumentIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			IEnumerable<FieldMap> extractedTextMapping = await GetExtractedTextMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);

			configuration.SetFieldMappings(identifierMapping.Concat(extractedTextMapping).ToList());
		}
	}
}
