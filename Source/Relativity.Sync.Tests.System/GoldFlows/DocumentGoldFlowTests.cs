using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

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
		public async Task SyncJob_Should_SyncDocuments()
		{
			// Arrange
			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalDocumentCount).ConfigureAwait(false);
		}
		
		[IdentifiedTest("215AD6CF-A79A-45A9-AEE2-22C4848F1F8B")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_SyncDocuments_And_NotCreateErrors_WhenDisabled()
		{
			// Arrange
			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(
				async (sourceWrokspace, destinationWorkspace, configuration) =>
				{
					await ConfigureTestRunAsync(sourceWrokspace, destinationWorkspace, configuration)
						.ConfigureAwait(false);

					configuration.LogItemLevelErrors = false;

					// to create item level errors
					configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOnly;
					await _goldFlowTestSuite.ImportDocumentsAsync(DataTableFactory.CreateImportDataTable(_dataset, true), destinationWorkspace)
						.ConfigureAwait(false);
					
				}).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			IEnumerable<RelativityObjectSlim> itemLevelErrors = await GetAllItemLevelErrors(goldFlowTestRun.SourceWorkspaceArtifactId).ConfigureAwait(false);
			itemLevelErrors.Should().BeEmpty();
		}

		private async Task<IEnumerable<RelativityObjectSlim>> GetAllItemLevelErrors(int workspaceId)
		{
			using (IObjectManager objectManager =
			       new ServiceFactoryFromAppConfig().CreateServiceFactory().CreateProxy<IObjectManager>())
			{
				var query = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = DefaultGuids.JobHistoryError.TypeGuid
					}
				};

				return (await objectManager.QuerySlimAsync(workspaceId, query, 0, Int32.MaxValue)).Objects;
			}
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
