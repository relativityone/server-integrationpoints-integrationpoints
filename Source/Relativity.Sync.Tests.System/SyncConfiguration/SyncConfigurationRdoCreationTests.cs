using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
	[TestFixture]
	internal class SyncConfigurationRdoCreationTests : SystemTest
	{
		public ISyncServiceManager SyncServicesMgr;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();

			SyncServicesMgr = new ServicesManagerStub();
		}

		[Test]
		public async Task Exists_ShouldReturnTrue_WhenSyncConfigurationRdoExistsInWorkspace()
		{
			// Arrange
			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			// Act
			bool exists = await SyncConfigurationRdo.Exists(workspace.ArtifactID, SyncServicesMgr).ConfigureAwait(false);

			// Assert
			exists.Should().BeTrue();
		}

		[Test]
		public async Task Exists_ShouldReturnFalse_WhenSyncConfigurationRdoDoesNotExistInWorkspace()
		{
			// Arrange
			WorkspaceRef workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);

			// Act
			bool exists = await SyncConfigurationRdo.Exists(workspace.ArtifactID, SyncServicesMgr).ConfigureAwait(false);

			// Assert
			exists.Should().BeFalse();
		}

		[Test]
		public async Task CreateType_ShouldHandleSyncConfigurationCreation()
		{
			// Arrange
			WorkspaceRef refWorkspace = new WorkspaceRef(1019372); //await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			int refWorkspaceId = refWorkspace.ArtifactID;
			
			var refSyncConfigurationTypeId =
				await ReadRefSyncConfigurationTypeId(refWorkspace.ArtifactID, SyncConfigurationRdo.SyncConfigurationGuid).ConfigureAwait(false);

			WorkspaceRef testWorkspace = new WorkspaceRef(1019374); //await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			int testWorkspaceId = testWorkspace.ArtifactID;

			int parentObjectTypeId = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, testWorkspace.ArtifactID, $"{Guid.NewGuid()}",
				new ObjectTypeIdentifier {ArtifactTypeID = (int)ArtifactType.Case}).ConfigureAwait(false);

			RelativityObject parentObject = await Rdos
				.CreateBasicRdoAsync(ServiceFactory, testWorkspace.ArtifactID, parentObjectTypeId).ConfigureAwait(false);

			// Act
			int createdConfigurationTypeId = await SyncConfigurationRdo.CreateType(testWorkspace.ArtifactID, parentObject.ArtifactID, SyncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertConfigurationType(
				refWorkspaceId, refSyncConfigurationTypeId,
				testWorkspaceId, createdConfigurationTypeId).ConfigureAwait(false);

		}

		private async Task AssertConfigurationType(
			int refWorkspaceId, int refConfigurationTypeId,
			int testWorkspaceId, int createdConfigurationTypeId)
		{
			var expectedSyncConfigurationType =
				await ReadSyncConfigurationType(refWorkspaceId, refConfigurationTypeId)
					.ConfigureAwait(false);

			var createdSyncConfigurationType =
				await ReadSyncConfigurationType(testWorkspaceId, createdConfigurationTypeId)
					.ConfigureAwait(false);

			createdSyncConfigurationType.Should().BeEquivalentTo(expectedSyncConfigurationType,
				config =>
				{
					config.Excluding(x => x.ArtifactID);
					config.Excluding(x => x.ArtifactTypeID);
					config.Excluding(x => x.CreatedBy);
					config.Excluding(x => x.CreatedOn);
					config.Excluding(x => x.FieldByteUsage);
					config.Excluding(x => x.Guids);
					config.Excluding(x => x.LastModifiedBy);
					config.Excluding(x => x.LastModifiedOn);
					config.Excluding(x => x.Name);
					config.Excluding(x => x.ParentObjectType);
					config.Excluding(x => x.RelativityApplications);

					return config;
				});

			var expectedSyncConfigurationFieldTypes =
				await ReadSyncConfigurationTypeFields(refWorkspaceId, expectedSyncConfigurationType.ArtifactTypeID).ConfigureAwait(false);

			var createdSyncConfigurationFieldTypes =
				await ReadSyncConfigurationTypeFields(testWorkspaceId, createdSyncConfigurationType.ArtifactTypeID).ConfigureAwait(false);

			createdSyncConfigurationFieldTypes.Should().BeEquivalentTo(expectedSyncConfigurationFieldTypes);
		}

		private async Task<int> ReadRefSyncConfigurationTypeId(int workspaceId, Guid guid)
		{
			using (IObjectManager objectManager = SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			using (IObjectTypeManager objectTypeManager = SyncServicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
			{
				ReadRequest request = new ReadRequest()
				{
					Object = new RelativityObjectRef()
					{
						Guid = guid
					}
				};
				ReadResult result = await objectManager.ReadAsync(workspaceId, request).ConfigureAwait(false);

				var objectType = await objectTypeManager.ReadAsync(workspaceId, result.Object.ArtifactID)
					.ConfigureAwait(false);

				return objectType.ArtifactID;
			}
		}

		private async Task<ObjectTypeResponse> ReadSyncConfigurationType(int workspaceId, int syncConfigurationTypeId)
		{
			using (IObjectTypeManager objectTypeManager = SyncServicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
			{
				return await objectTypeManager.ReadAsync(workspaceId, syncConfigurationTypeId)
					.ConfigureAwait(false);
			}
		}

		private async Task<List<RelativityObject>> ReadSyncConfigurationTypeFields(int workspaceId, int configurationTypeId)
		{
			using (IObjectManager objectManager = SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						ArtifactTypeID = (int) ArtifactType.Field
					},
					Fields = new List<FieldRef>
					{
						new FieldRef
						{
							Name = "*"
						}
					},
					Condition = $"'FieldArtifactTypeID' == {configurationTypeId}" +
					            $"AND 'DisplayName' IN [{string.Join(",", SyncConfigurationRdo.GetFieldsDefinition(0).Values.Select(x => x.Name))}]"
				};

				var result = await objectManager.QueryAsync(workspaceId, request, 1, 100).ConfigureAwait(false);

				return result.Objects;
			}
		}
	}
}
