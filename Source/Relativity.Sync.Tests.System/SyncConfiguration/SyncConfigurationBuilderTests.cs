using System;
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
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;
using Action = System.Action;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	[TestFixture]
	class SyncConfigurationBuilderTests : SystemTest
	{
		public ISyncServiceManager SyncServicesMgr;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();

			SyncServicesMgr = new ServicesManagerStub();
		}

		[Test]
		public async Task SyncConfigurationBuilder_ShouldBuildConfigurationWithCorrectParentObject_WhenSyncConfigurationRdoDoesNotExist()
		{
			// Arrange
			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			int sourceWorkspaceId = sourceWorkspace.ArtifactID;

			int parentObjectTypeId = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, sourceWorkspace.ArtifactID, $"{Guid.NewGuid()}",
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
			int createdConfigurationId = new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureDocumentSync(options)
				.Build();

			// Assert
			var createdSyncConfiguration = await ReadSyncConfiguration(sourceWorkspaceId, createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.ParentObject.ArtifactID.Should().Be(parentObject.ArtifactID);
		}

		[Test]
		public async Task SyncConfigurationBuilder_ShouldThrow_WhenSyncConfigurationRdoExistsAndParentObjectIsInvalid()
		{
			// Arrange
			WorkspaceRef sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			int sourceWorkspaceId = sourceWorkspace.ArtifactID;

			int parentObjectTypeId = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, sourceWorkspace.ArtifactID, $"{Guid.NewGuid()}",
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
			Action action = () => new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureDocumentSync(options)
				.Build();

			// Assert
			action.Should().Throw<InvalidObjectTypeException>();
		}

		private async Task<RelativityObject> ReadSyncConfiguration(int workspaceId, int configurationId)
		{
			using (IObjectManager objectManager = SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
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
	}
}
