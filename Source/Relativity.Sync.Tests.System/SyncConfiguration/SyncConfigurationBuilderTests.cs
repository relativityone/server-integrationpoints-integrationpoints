using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Relativity.API;
using Relativity.Services.Workspace;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	class SyncConfigurationBuilderTests : SystemTest
	{
		private RdoOptions _rdoOptions;
		
        protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();
			
			_rdoOptions = DefaultGuids.DefaultRdoOptions;
		}

		[IdentifiedTest("08889EA2-DFFB-4F21-8723-5D2C4F23646C")]
		public async Task SyncConfigurationBuilder_ShouldSaveConfigurationWithSyncStatistics()
		{
			// Arrange
			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			
			Task<WorkspaceRef> destinationWorkspaceTask = Environment.CreateWorkspaceAsync();

			int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, sourceWorkspace.ArtifactID).ConfigureAwait(false);
			
			int savedSearchId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, sourceWorkspace.ArtifactID).ConfigureAwait(false);

			WorkspaceRef destinationWorkspace = await destinationWorkspaceTask.ConfigureAwait(false);
			
			int destinationFolderId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			
			ISyncContext syncContext =
				new SyncContext(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID, jobHistoryId);

			DocumentSyncOptions options = new DocumentSyncOptions(savedSearchId, destinationFolderId);
			
			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, Mock.Of<IServicesMgr>(), new EmptyLogger())
				.ConfigureRdos(_rdoOptions)
				.ConfigureDocumentSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			SyncConfigurationRdo createdSyncConfiguration =
				await Rdos.ReadRdoAsync<SyncConfigurationRdo>(sourceWorkspace.ArtifactID, createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.ArtifactId.Should().Be(createdConfigurationId);

			SyncStatisticsRdo syncStatistics = 
				await Rdos.ReadRdoAsync<SyncStatisticsRdo>(sourceWorkspace.ArtifactID, createdSyncConfiguration.SyncStatisticsId)
					.ConfigureAwait(false);

			syncStatistics.Should().NotBeNull();
		}
	}
}
