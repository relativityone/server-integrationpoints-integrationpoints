using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	internal abstract class SyncConfigurationCreationTestsBase : SystemTest
	{
		protected ISourceServiceFactoryForAdmin ServiceFactoryForAdmin;
		protected IServicesMgr ServicesMgr;

		protected int SourceWorkspaceId;
		protected int DestinationWorkspaceId;

		protected RelativityObject JobHistory;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();

			ServiceFactoryForAdmin = new SourceServiceFactoryStub();
            ServicesMgr = new ServicesManagerStub();

			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			SourceWorkspaceId = sourceWorkspace.ArtifactID;

			WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			DestinationWorkspaceId = destinationWorkspace.ArtifactID;

			JobHistory = await Rdos.CreateJobHistoryRelativityObjectInstanceAsync(ServiceFactory, SourceWorkspaceId, new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9")).ConfigureAwait(false);
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
			using (IObjectManager objectManager = await ServiceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid(SyncRdoGuids.SyncConfigurationGuid)
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

			return new SyncConfigurationRdo
			{
				CreateSavedSearchInDestination = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.CreateSavedSearchInDestinationGuid)),
				DataDestinationArtifactId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.DataDestinationArtifactIdGuid)),
				DataDestinationType = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.DataDestinationTypeGuid)).GetEnumFromDescription<DestinationLocationType>(),
				DataSourceArtifactId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.DataSourceArtifactIdGuid)),
				DataSourceType = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.DataSourceTypeGuid)).GetEnumFromDescription<DataSourceType>(),
				DestinationFolderStructureBehavior = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.DestinationFolderStructureBehaviorGuid)).GetEnumFromDescription<DestinationFolderStructureBehavior>(),
				DestinationWorkspaceArtifactId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.DestinationWorkspaceArtifactIdGuid)),
				EmailNotificationRecipients = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.EmailNotificationRecipientsGuid)),
				FieldsMapping = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.FieldMappingsGuid)),
				FieldOverlayBehavior = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.FieldOverlayBehaviorGuid)).GetEnumFromDescription<FieldOverlayBehavior>(),
				FolderPathSourceFieldName = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.FolderPathSourceFieldNameGuid)),
				ImportOverwriteMode = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.ImportOverwriteModeGuid)).GetEnumFromDescription<ImportOverwriteMode>(),
				MoveExistingDocuments = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.MoveExistingDocumentsGuid)),
				NativesBehavior = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.NativesBehaviorGuid)).GetEnumFromDescription<ImportNativeFileCopyMode>(),
				RdoArtifactTypeId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.RdoArtifactTypeIdGuid)),
				DestinationRdoArtifactTypeId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.DestinationRdoArtifactTypeIdGuid)),
				JobHistoryToRetryId = ReadSyncConfigurationValue<int?>(configuration, new Guid(SyncRdoGuids.JobHistoryToRetryIdGuid)),
				JobHistoryId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.JobHistoryIdGuid)),
				ImageImport = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.ImageImportGuid)),
				IncludeOriginalImages = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.IncludeOriginalImagesGuid)),
				ProductionImagePrecedence = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.ProductionImagePrecedenceGuid)),
				ImageFileCopyMode = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.ImageFileCopyModeGuid)).GetEnumFromDescription<ImportImageFileCopyMode>()
			};
		}
	}
}
