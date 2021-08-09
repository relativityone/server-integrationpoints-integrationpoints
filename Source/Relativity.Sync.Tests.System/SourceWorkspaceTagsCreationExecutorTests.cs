using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class SourceWorkspaceTagsCreationExecutorTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private WorkspaceRef _sourceWorkspace;

		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_FIELD_GUID = Guid.Parse("323458DB-8A06-464B-9402-AF2516CF47E0");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_FIELD_GUID = Guid.Parse("909ADC7C-2BB9-46CA-9F85-DA32901D6554");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_FIELD_GUID = Guid.Parse("207E6836-2961-466B-A0D2-29974A4FAD36");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_FIELD_GUID = Guid.Parse("348D7394-2658-4DA4-87D0-8183824ADF98");
		private static readonly Guid _DESTINATION_WORKSPACE_GUID = Guid.Parse("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private static readonly Guid _DESTINATION_WORKSPACE_JOB_HISTORY_FIELD_GUID = Guid.Parse("07B8A468-DEC8-45BD-B50A-989A35150BE2");
		
		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;
		}

		[IdentifiedTest("c74612fc-2216-4ecb-af58-23e28826bdc0")]
		public async Task ItShouldCreateTagIfItDoesNotExist()
		{
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				JobHistoryArtifactId = jobHistoryArtifactId
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<ISourceWorkspaceTagsCreationConfiguration>(configuration);

			// ACT
			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			RelativityObject tag = await QueryForCreatedTagAsync(configuration.DestinationWorkspaceTagArtifactId)
				.ConfigureAwait(false);

			Assert.AreEqual(_destinationWorkspace.ArtifactID, tag.FieldValues.First(x => x.Field.Guids.Contains(_DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_FIELD_GUID)).Value);
			Assert.AreEqual(_destinationWorkspace.Name, tag.FieldValues.First(x => x.Field.Guids.Contains(_DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_FIELD_GUID)).Value);

			var relativityObjectValues = (List<RelativityObjectValue>) tag.FieldValues
				.First(x => x.Field.Guids.Contains(_DESTINATION_WORKSPACE_JOB_HISTORY_FIELD_GUID)).Value;
			Assert.AreEqual(1, relativityObjectValues.Count);
			Assert.AreEqual(jobHistoryArtifactId, relativityObjectValues.First().ArtifactID);
		}

		[IdentifiedTest("c70961bc-dbdf-4dce-a59d-a017c92b3282")]
		public async Task ItShouldUpdateTagIfItDoesExist()
		{
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				JobHistoryArtifactId = jobHistoryArtifactId
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<ISourceWorkspaceTagsCreationConfiguration>(configuration);

			int destinationWorkspaceTagArtifactId =
				await CreateDestinationWorkspaceTagAsync(_sourceWorkspace.ArtifactID, "whatever", "Wrong Workspace Name", _destinationWorkspace.ArtifactID).ConfigureAwait(false);

			// ACT
			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(destinationWorkspaceTagArtifactId, configuration.DestinationWorkspaceTagArtifactId);

			RelativityObject tag = await QueryForCreatedTagAsync(configuration.DestinationWorkspaceTagArtifactId)
				.ConfigureAwait(false);

			Assert.AreEqual(_destinationWorkspace.ArtifactID, tag.FieldValues.First(x => x.Field.Guids.Contains(_DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_FIELD_GUID)).Value);
			Assert.AreEqual(_destinationWorkspace.Name, tag.FieldValues.First(x => x.Field.Guids.Contains(_DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_FIELD_GUID)).Value);

			var relativityObjectValues = (List<RelativityObjectValue>) tag.FieldValues
				.First(x => x.Field.Guids.Contains(_DESTINATION_WORKSPACE_JOB_HISTORY_FIELD_GUID)).Value;
			Assert.AreEqual(1, relativityObjectValues.Count);
			Assert.AreEqual(jobHistoryArtifactId, relativityObjectValues.First().ArtifactID);
		}

		private async Task<RelativityObject> QueryForCreatedTagAsync(int destinationWorkspaceTagArtifactId)
		{
			RelativityObject tag;
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					Condition = $"'ArtifactId' == {destinationWorkspaceTagArtifactId}",
					Fields = new[]
					{
						new FieldRef
						{
							Guid = _DESTINATION_WORKSPACE_JOB_HISTORY_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_FIELD_GUID
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = _DESTINATION_WORKSPACE_GUID
					}
				};
				QueryResult result = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, request, 0, 1)
					.ConfigureAwait(false);

				tag = result.Objects.First();
			}

			return tag;
		}

		private async Task<int> CreateDestinationWorkspaceTagAsync(int workspaceId, string tagName, string destinationWorkspaceName, int destinationWorkspaceArtifactId)
		{
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = _DESTINATION_WORKSPACE_GUID
					},
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Name = "Name"
							},
							Value = tagName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_FIELD_GUID
							},
							Value = destinationWorkspaceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_FIELD_GUID
							},
							Value = destinationWorkspaceArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_FIELD_GUID
							},
							Value = "This Instance"
						},
						new FieldRefValuePair
						{
							Field = new FieldRef
							{
								Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_FIELD_GUID
							},
							Value = -1
						}
					}
				};

				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}
	}
}