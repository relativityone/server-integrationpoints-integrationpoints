﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Objects.Exceptions;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	class SyncConfigurationBuilderTests : SystemTest
	{
		private RdoOptions _rdoOptions;

		private ISyncServiceManager _syncServicesMgr;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();
			
			_rdoOptions = DefaultGuids.DefaultRdoOptions;
			_syncServicesMgr = new ServicesManagerStub();
		}

		[IdentifiedTest("08889EA2-DFFB-4F21-8723-5D2C4F23646C")]
		public async Task SyncConfigurationBuilder_ShouldSaveConfiguration()
		{
			// Arrange
			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			
			Task<WorkspaceRef> destinationWorkspaceTask = Environment.CreateWorkspaceAsync();

			int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, sourceWorkspace.ArtifactID).ConfigureAwait(false);
			
			int savedSearchId = await Rdos.GetSavedSearchInstance(ServiceFactory, sourceWorkspace.ArtifactID).ConfigureAwait(false);

			WorkspaceRef destinationWorkspace = await destinationWorkspaceTask.ConfigureAwait(false);
			
			int destinationFolderId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			
			ISyncContext syncContext =
				new SyncContext(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID, jobHistoryId);

			DocumentSyncOptions options = new DocumentSyncOptions(savedSearchId, destinationFolderId);
			
			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, _syncServicesMgr)
				.ConfigureRdos(_rdoOptions)
				.ConfigureDocumentSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			RelativityObject createdSyncConfiguration = await ReadSyncConfiguration(sourceWorkspace.ArtifactID, createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.ArtifactID.Should().Be(createdConfigurationId);
		}
		
		private async Task<RelativityObject> ReadSyncConfiguration(int workspaceId, int configurationId)
		{
			using (IObjectManager objectManager = _syncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid(SyncRdoGuids.SyncConfigurationGuid)
					},
					Condition = $"'ArtifactID' == {configurationId}",
				};

				var result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

				return result.Objects.Single();
			}
		}
	}
}
