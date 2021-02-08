using System;
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

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	[TestFixture]
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

		[Test]
		[Ignore("This test needs separate installation of JobHistory and Sync Configuration")]
		public async Task SyncConfigurationBuilder_ShouldBuildConfigurationWithCorrectParentObject_WhenSyncConfigurationRdoDoesNotExist()
		{
			// Arrange
			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			int sourceWorkspaceId = sourceWorkspace.ArtifactID;

			(int parentObjectTypeId, _) = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, sourceWorkspace.ArtifactID, $"{Guid.NewGuid()}",
				new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Case }).ConfigureAwait(false);

			RelativityObject parentObject = await Rdos
				.CreateBasicRdoAsync(ServiceFactory, sourceWorkspace.ArtifactID, parentObjectTypeId).ConfigureAwait(false);

			int savedSearchId = await Rdos.GetSavedSearchInstance(ServiceFactory, sourceWorkspaceId).ConfigureAwait(false);

			WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			int destinationWorkspaceId = destinationWorkspace.ArtifactID;

			int destinationFolderId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspaceId).ConfigureAwait(false);

			ISyncContext syncContext =
				new SyncContext(sourceWorkspaceId, destinationWorkspaceId, parentObject.ArtifactID);

			DocumentSyncOptions options = new DocumentSyncOptions(savedSearchId, destinationFolderId);

			var guids = await GetGuids(sourceWorkspace.ArtifactID).ConfigureAwait(false);

			var guidsString = string.Join(global::System.Environment.NewLine, guids.Select(x => $"[{x.Name}, {x.Guids.First()}]"));
			
			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, _syncServicesMgr)
				.ConfigureRdos(_rdoOptions)
				.ConfigureDocumentSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			var createdSyncConfiguration = await ReadSyncConfiguration(sourceWorkspaceId, createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.ParentObject.ArtifactID.Should().Be(parentObject.ArtifactID);
		}

		[Test]
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
			var createdSyncConfiguration = await ReadSyncConfiguration(sourceWorkspace.ArtifactID, createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.ParentObject.ArtifactID.Should().Be(jobHistoryId);
		}
		
		[Test]
		public async Task SyncConfigurationBuilder_ShouldThrow_WhenSyncConfigurationRdoExistsAndParentObjectIsInvalid()
		{
			// Arrange
			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			int sourceWorkspaceId = sourceWorkspace.ArtifactID;

			(int parentObjectTypeId, _) = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, sourceWorkspace.ArtifactID, $"{Guid.NewGuid()}",
				new ObjectTypeIdentifier { ArtifactTypeID = (int)ArtifactType.Case }).ConfigureAwait(false);

			RelativityObject parentObject = await Rdos
				.CreateBasicRdoAsync(ServiceFactory, sourceWorkspace.ArtifactID, parentObjectTypeId).ConfigureAwait(false);

			int savedSearchId = await Rdos.GetSavedSearchInstance(ServiceFactory, sourceWorkspaceId).ConfigureAwait(false);

			WorkspaceRef destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			int destinationWorkspaceId = destinationWorkspace.ArtifactID;

			int destinationFolderId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspaceId).ConfigureAwait(false);

			ISyncContext syncContext =
				new SyncContext(sourceWorkspaceId, destinationWorkspaceId, parentObject.ArtifactID);

			DocumentSyncOptions options = new DocumentSyncOptions(savedSearchId, destinationFolderId);

			
			// Act
			Func<Task> action = async () => await new SyncConfigurationBuilder(syncContext, _syncServicesMgr)
				.ConfigureRdos(_rdoOptions)
				.ConfigureDocumentSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			action.Should().Throw<InvalidObjectTypeException>();
		}

		private async Task<RelativityObject> ReadSyncConfiguration(int workspaceId, int configurationId)
		{
			using (IObjectManager objectManager = _syncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = SyncConfigurationRdo.SyncConfigurationGuid
					},
					Condition = $"'ArtifactID' == {configurationId}",
				};

				var result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

				return result.Objects.Single();
			}
		}
		
		private async Task<List<RelativityObject>> GetGuids(int workspaceId)
		{
			using (IObjectManager objectManager =
				ServiceFactory.CreateProxy<IObjectManager>())
			{
				// bool exists = await artifactGuidManager.ReadSingleGuidsAsync(workspaceId, SyncConfigurationGuid)
				//     .ConfigureAwait(false);
				// if (exists)
				// {
				var syncConfigurationObjectId = await objectManager
					.QueryAsync(workspaceId, new QueryRequest
					{
						Condition = "'Name' == 'Job History'",
						Fields = new[] {new FieldRef {Name = "Artifact Type ID"}},
						ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.ObjectType},
						IncludeNameInQueryResult = true
					}, 0, 1).ConfigureAwait(false);

				var response = await objectManager.QueryAsync(workspaceId, new QueryRequest()
				{
					Fields = new FieldRef[0],
					Condition =
						$"'FieldArtifactTypeID' == {syncConfigurationObjectId.Objects.First().FieldValues.First().Value}",
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int) ArtifactType.Field
					},
					IncludeNameInQueryResult = true,
				}, 0, 100).ConfigureAwait(false);

				return response.Objects;
			}
		}
	}
}
