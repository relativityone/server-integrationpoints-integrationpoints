using System.Threading.Tasks;
using System.Collections.Generic;
using FluentAssertions;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Configuration;
using Relativity.Testing.Identification;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using NUnit.Framework;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	[TestFixture]
	public class ImageGoldFlowTests : SystemTest
	{
		private const int _HAS_IMAGES_YES_CHOICE = 1034243;

		private GoldFlowTestSuite _goldFlowTestSuite;

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory, DataTableFactory.CreateImageImportDataTable(Dataset.Images))
				.ConfigureAwait(false);
		}

		[IdentifiedTest("1af24688-54e2-44eb-86f4-60fb28d37df4")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_SyncDocuments()
		{
			// Arrange
			var goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			int documentsWithImagesInDestinationWorkspaceCount = (await Rdos.QueryDocumentIdentifiersAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false))
				.Count;

			await goldFlowTestRun.AssertAsync(result, _goldFlowTestSuite.DataSetItemsCount).ConfigureAwait(false);
			documentsWithImagesInDestinationWorkspaceCount.Should().Be(_goldFlowTestSuite.DataSetItemsCount);
		}

		[IdentifiedTest("e4451454-ea17-4d0e-b45a-a2c43ad35add")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_RetryDocuments()
		{
			// Arrange
			int jobHistoryToRetryId = -1;
			var goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(async (sourceWorkspace, destinationWorkspace, configuration) =>
			{
				await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration);

				jobHistoryToRetryId = await Rdos.CreateJobHistoryInstanceAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID)
					.ConfigureAwait(false);
				configuration.JobHistoryToRetryId = jobHistoryToRetryId;

			}).ConfigureAwait(false);

			const int numberOfTaggedDocuments = 2;
			await Rdos.TagDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistoryToRetryId, numberOfTaggedDocuments).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _goldFlowTestSuite.DataSetItemsCount - numberOfTaggedDocuments).ConfigureAwait(false);
		}

		private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
		{
			configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.CopyFiles;
			configuration.ImageImport = true;
			configuration.IncludeOriginalImages = true;

			IList<FieldMap> identifierMapping = await GetIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			configuration.SetFieldMappings(identifierMapping);
		}
	}
}
