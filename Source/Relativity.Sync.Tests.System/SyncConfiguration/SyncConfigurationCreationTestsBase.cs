using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	internal abstract class SyncConfigurationCreationTestsBase : SystemTest
	{
		protected ISyncServiceManager SyncServicesMgr;

		protected int SourceWorkspaceId;
		protected int DestinationWorkspaceId;

		protected RelativityObject JobHistory;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();

			SyncServicesMgr = new ServicesManagerStub();

			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			SourceWorkspaceId = sourceWorkspace.ArtifactID;

			WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			DestinationWorkspaceId = destinationWorkspace.ArtifactID;

			JobHistory = await Rdos.CreateJobHistoryRelativityObjectInstanceAsync(ServiceFactory, SourceWorkspaceId).ConfigureAwait(false);
		}

		protected async Task AssertCreatedConfigurationAsync(int createdConfigurationId, SyncConfigurationRdo expectedConfiguration)
		{
			var createdSyncConfiguration = await ReadSyncConfigurationAsync(createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.Should().BeEquivalentTo(expectedConfiguration);
		}

		private async Task<SyncConfigurationRdo> ReadSyncConfigurationAsync(int configurationId)
		{
			T ReadSyncConfigurationValue<T>(RelativityObject configurationRdo, Guid fieldGuid)
			{
				object val = configurationRdo.FieldValues.Single(x => x.Field.Guids.Contains(fieldGuid)).Value;

				return val == null ? default(T) : (T)val;
			}

			RelativityObject configuration;
			using (IObjectManager objectManager = SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = SyncRdoGuids.SyncConfigurationGuid
					},
					Fields = new List<FieldRef>()
					{
						new FieldRef { Name = "*" }
					},
					Condition = $"'ArtifactID' == {configurationId}",
				};

				var result = await objectManager.QueryAsync(SourceWorkspaceId, request, 0, 1).ConfigureAwait(false);

				configuration = result.Objects.Single();
			}

			int? jobHistoryToRetryValue =
				ReadSyncConfigurationValue<int?>(configuration,
					SyncRdoGuids.JobHistoryToRetryIdGuid);

			return new SyncConfigurationRdo
			{
				CreateSavedSearchInDestination = ReadSyncConfigurationValue<bool>(configuration, SyncRdoGuids.CreateSavedSearchInDestinationGuid),
				DataDestinationArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncRdoGuids.DataDestinationArtifactIdGuid),
				DataDestinationType = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.DataDestinationTypeGuid),
				DataSourceArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncRdoGuids.DataSourceArtifactIdGuid),
				DataSourceType = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.DataSourceTypeGuid),
				DestinationFolderStructureBehavior = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.DestinationFolderStructureBehaviorGuid),
				DestinationWorkspaceArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncRdoGuids.DestinationWorkspaceArtifactIdGuid),
				EmailNotificationRecipients = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.EmailNotificationRecipientsGuid),
				FieldsMapping = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.FieldMappingsGuid),
				FieldOverlayBehavior = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.FieldOverlayBehaviorGuid),
				FolderPathSourceFieldName = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.FolderPathSourceFieldNameGuid),
				ImportOverwriteMode = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.ImportOverwriteModeGuid),
				MoveExistingDocuments = ReadSyncConfigurationValue<bool>(configuration, SyncRdoGuids.MoveExistingDocumentsGuid),
				NativesBehavior = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.NativesBehaviorGuid),
				RdoArtifactTypeId = ReadSyncConfigurationValue<int>(configuration, SyncRdoGuids.RdoArtifactTypeIdGuid),
				JobHistoryToRetryId = jobHistoryToRetryValue,
				ImageImport = ReadSyncConfigurationValue<bool>(configuration, SyncRdoGuids.ImageImportGuid),
				IncludeOriginalImages = ReadSyncConfigurationValue<bool>(configuration, SyncRdoGuids.IncludeOriginalImagesGuid),
				ProductionImagePrecedence = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.ProductionImagePrecedenceGuid),
				ImageFileCopyMode = ReadSyncConfigurationValue<string>(configuration, SyncRdoGuids.ImageFileCopyModeGuid)
			};
		}
	}
}
