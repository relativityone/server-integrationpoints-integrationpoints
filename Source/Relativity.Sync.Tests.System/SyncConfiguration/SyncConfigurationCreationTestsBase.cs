using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
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

			//WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			SourceWorkspaceId = 1019353; //sourceWorkspace.ArtifactID;

			//WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			DestinationWorkspaceId = 1019356; //destinationWorkspace.ArtifactID;

			JobHistory = await Rdos.CreateJobHistoryRelativityObjectInstanceAsync(ServiceFactory, SourceWorkspaceId).ConfigureAwait(false);
		}

		protected static RelativityObject GetBasicRelativityObject(int artifactId)
		{
			return new RelativityObject() { ArtifactID = artifactId };
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
						Guid = SyncConfigurationRdo.SyncConfigurationGuid
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

			RelativityObjectValue jobHistoryToRetryValue =
				ReadSyncConfigurationValue<RelativityObjectValue>(configuration,
					SyncConfigurationRdo.JobHistoryToRetryGuid);

			return new SyncConfigurationRdo
			{
				CreateSavedSearchInDestination = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRdo.CreateSavedSearchInDestinationGuid),
				DataDestinationArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRdo.DataDestinationArtifactIdGuid),
				DataDestinationType = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.DataDestinationTypeGuid),
				DataSourceArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRdo.DataSourceArtifactIdGuid),
				DataSourceType = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.DataSourceTypeGuid),
				DestinationFolderStructureBehavior = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid),
				DestinationWorkspaceArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid),
				EmailNotificationRecipients = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.EmailNotificationRecipientsGuid),
				FieldsMapping = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.FieldMappingsGuid),
				FieldOverlayBehavior = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.FieldOverlayBehaviorGuid),
				FolderPathSourceFieldName = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.FolderPathSourceFieldNameGuid),
				ImportOverwriteMode = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.ImportOverwriteModeGuid),
				MoveExistingDocuments = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRdo.MoveExistingDocumentsGuid),
				NativesBehavior = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.NativesBehaviorGuid),
				RdoArtifactTypeId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRdo.RdoArtifactTypeIdGuid),
				JobHistoryToRetry = jobHistoryToRetryValue == null ? null : GetBasicRelativityObject(jobHistoryToRetryValue.ArtifactID),
				ImageImport = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRdo.ImageImportGuid),
				IncludeOriginalImages = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRdo.IncludeOriginalImagesGuid),
				ProductionImagePrecedence = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.ProductionImagePrecedenceGuid),
				ImageFileCopyMode = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRdo.ImageFileCopyModeGuid)
			};
		}
	}
}
