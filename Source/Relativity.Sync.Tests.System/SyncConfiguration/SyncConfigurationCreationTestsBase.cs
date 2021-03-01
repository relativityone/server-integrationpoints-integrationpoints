﻿using System;
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
				DataDestinationType = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.DataDestinationTypeGuid)),
				DataSourceArtifactId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.DataSourceArtifactIdGuid)),
				DataSourceType = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.DataSourceTypeGuid)),
				DestinationFolderStructureBehavior = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.DestinationFolderStructureBehaviorGuid)),
				DestinationWorkspaceArtifactId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.DestinationWorkspaceArtifactIdGuid)),
				EmailNotificationRecipients = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.EmailNotificationRecipientsGuid)),
				FieldsMapping = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.FieldMappingsGuid)),
				FieldOverlayBehavior = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.FieldOverlayBehaviorGuid)),
				FolderPathSourceFieldName = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.FolderPathSourceFieldNameGuid)),
				ImportOverwriteMode = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.ImportOverwriteModeGuid)),
				MoveExistingDocuments = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.MoveExistingDocumentsGuid)),
				NativesBehavior = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.NativesBehaviorGuid)),
				RdoArtifactTypeId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.RdoArtifactTypeIdGuid)),
				JobHistoryToRetryId = ReadSyncConfigurationValue<int?>(configuration, new Guid(SyncRdoGuids.JobHistoryToRetryIdGuid)),
				JobHistoryId = ReadSyncConfigurationValue<int>(configuration, new Guid(SyncRdoGuids.JobHistoryIdGuid)),
				ImageImport = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.ImageImportGuid)),
				IncludeOriginalImages = ReadSyncConfigurationValue<bool>(configuration, new Guid(SyncRdoGuids.IncludeOriginalImagesGuid)),
				ProductionImagePrecedence = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.ProductionImagePrecedenceGuid)),
				ImageFileCopyMode = ReadSyncConfigurationValue<string>(configuration, new Guid(SyncRdoGuids.ImageFileCopyModeGuid))
			};
		}
	}
}
