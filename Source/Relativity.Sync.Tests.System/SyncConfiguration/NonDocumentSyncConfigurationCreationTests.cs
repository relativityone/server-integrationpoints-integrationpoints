using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	internal class NonDocumentSyncConfigurationCreationTests : SyncConfigurationCreationTestsBase
	{
		private const int RdoArtifactTypeId = (int)ArtifactType.Production;
		private const int DestinationRdoArtifactTypeId = (int)ArtifactType.Production;
		private const string DefaultViewName = "All Productions";

		[IdentifiedTest("23DAD98E-10B3-4AD8-947F-3E314624600D")]
		public async Task Create_DefaultNonDocumentSyncConfiguration()
		{
			// Arrange
			int viewArtifactId = await GetDefaultViewAsync().ConfigureAwait(false);
			SyncConfigurationRdo expectedSyncConfiguration = CreateDefaultExpectedConfiguration(viewArtifactId);
			
			ISyncContext syncContext = new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			NonDocumentSyncOptions options = new NonDocumentSyncOptions(viewArtifactId, RdoArtifactTypeId, DestinationRdoArtifactTypeId);

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureNonDocumentSync(options)
				.SaveAsync()
				.ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		[IdentifiedTest("9AD684EC-652D-4528-A8D1-1DBB2174173E")]
		public void Create_DefaultNonDocumentSyncConfiguration_ShouldThrowWhenViewDoesntExist()
		{
			// Arrange
			int viewArtifactId = 1111;
			ISyncContext syncContext = new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
			NonDocumentSyncOptions options = new NonDocumentSyncOptions(viewArtifactId, RdoArtifactTypeId, DestinationRdoArtifactTypeId);

			// Act
			Func<Task<int>> action = async () => await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureNonDocumentSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			action.Should().Throw<InvalidSyncConfigurationException>();
		}

		private SyncConfigurationRdo CreateDefaultExpectedConfiguration(int viewArtifactId)
		{
			return new SyncConfigurationRdo
			{
				RdoArtifactTypeId = RdoArtifactTypeId,
				DestinationRdoArtifactTypeId = DestinationRdoArtifactTypeId,
				DataSourceType = DataSourceType.View,
				DataSourceArtifactId = viewArtifactId,
				DestinationWorkspaceArtifactId = DestinationWorkspaceId,
				DataDestinationType = DestinationLocationType.Folder,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				ImportOverwriteMode = ImportOverwriteMode.AppendOnly,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				NativesBehavior = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				JobHistoryId = JobHistory.ArtifactID
			};
		}

		private async Task<int> GetDefaultViewAsync()
		{
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.View
					},
					Condition = $"'Name' == '{DefaultViewName}'"
				};

				QueryResult queryResult = await objectManager.QueryAsync(SourceWorkspaceId, queryRequest, 0, 1).ConfigureAwait(false);

				if (queryResult.Objects.Any())
				{
					return queryResult.Objects.First().ArtifactID;
				}
				else
				{
					throw new Exception($"Cannot find view '{DefaultViewName}'.");
				}
			}
		}
	}
}