using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class DestinationWorkspaceTagsCreationExecutorTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private WorkspaceRef _sourceWorkspace;

		private const string _JOB_HISTORY_NAME = "Test Job Name";
		private const string _LOCAL_INSTANCE_NAME = "This Instance";
		private static readonly Guid _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

		private static readonly Guid _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
		private static readonly Guid _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");


		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.GetAwaiter().GetResult();
			_destinationWorkspace = destinationWorkspaceCreationTask.GetAwaiter().GetResult();
		}

		[IdentifiedTest("98d0dd99-85cf-40df-8cfb-b037a9089a1f")]
		public async Task ItShouldCreateTagsIfTheyDoesNotExist()
		{
			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			string expectedSourceWorkspaceName = _sourceWorkspace.Name;
			string expectedSourceCaseTagName = $"{_LOCAL_INSTANCE_NAME} - {expectedSourceWorkspaceName} - {expectedSourceWorkspaceArtifactId}";
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, expectedSourceWorkspaceArtifactId, _JOB_HISTORY_NAME).ConfigureAwait(false);
			string expectedSourceJobTagName = $"{_JOB_HISTORY_NAME} - {expectedJobHistoryArtifactId}";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobHistoryArtifactId = expectedJobHistoryArtifactId
			};

			// ACT
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDestinationWorkspaceTagsCreationConfiguration>(configuration);

			// ASSERT
			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			RelativityObject sourceCaseTag = await QueryForCreatedSourceCaseTagAsync(configuration.SourceWorkspaceTagArtifactId).ConfigureAwait(false);
			RelativityObject sourceJobTag = (await QueryForCreatedSourceJobTagAsync(configuration.SourceJobTagArtifactId).ConfigureAwait(false)).First();

			Assert.AreEqual(expectedSourceWorkspaceArtifactId, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_ID_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceWorkspaceName, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(_LOCAL_INSTANCE_NAME, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceCaseTagName, sourceCaseTag.Name);

			Assert.AreEqual(sourceCaseTag.ArtifactID, sourceJobTag.ParentObject.ArtifactID);
			Assert.AreEqual(expectedJobHistoryArtifactId, sourceJobTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID)).Value);
			Assert.AreEqual(_JOB_HISTORY_NAME, sourceJobTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceJobTagName, sourceJobTag.Name);
		}

		[IdentifiedTest("bd967cff-fa7f-4810-a4a8-9e4a90cca593")]
		public async Task ItShouldUpdateSourceCaseTagAndCreateJobTag()
		{
			string wrongSourceTagName = "Definitely not a correct name";
			string wrongSourceWorkspaceName = "Wrong source workspace name";

			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			string expectedSourceWorkspaceName = _sourceWorkspace.Name;
			string expectedSourceCaseTagName = $"{_LOCAL_INSTANCE_NAME} - {expectedSourceWorkspaceName} - {expectedSourceWorkspaceArtifactId}";

			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, expectedSourceWorkspaceArtifactId, _JOB_HISTORY_NAME).ConfigureAwait(false);

			RelativitySourceCaseTag wrongSourceCaseTag = new RelativitySourceCaseTag
			{
				Name = wrongSourceTagName,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				SourceWorkspaceName = wrongSourceWorkspaceName,
				SourceInstanceName = _LOCAL_INSTANCE_NAME
			};

			int expectedSourceCaseTagArtifactId = await Rdos.CreateRelativitySourceCaseInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID, wrongSourceCaseTag).ConfigureAwait(false);
			string expectedSourceJobTagName = $"{_JOB_HISTORY_NAME} - {expectedJobHistoryArtifactId}";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobHistoryArtifactId = expectedJobHistoryArtifactId
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDestinationWorkspaceTagsCreationConfiguration>(configuration);

			// ACT
			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);
			
			// ASSERT
			RelativityObject sourceCaseTag = await QueryForCreatedSourceCaseTagAsync(configuration.SourceWorkspaceTagArtifactId).ConfigureAwait(false);
			RelativityObject sourceJobTag = (await QueryForCreatedSourceJobTagAsync(configuration.SourceJobTagArtifactId).ConfigureAwait(false)).First();

			Assert.AreEqual(expectedSourceCaseTagArtifactId, sourceCaseTag.ArtifactID);
			Assert.AreEqual(expectedSourceWorkspaceArtifactId, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_ID_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceWorkspaceName, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(_LOCAL_INSTANCE_NAME, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceCaseTagName, sourceCaseTag.Name);

			Assert.AreEqual(expectedJobHistoryArtifactId, sourceJobTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID)).Value);
			Assert.AreEqual(_JOB_HISTORY_NAME, sourceJobTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceJobTagName, sourceJobTag.Name);
			Assert.AreEqual(sourceCaseTag.ArtifactID, sourceJobTag.ParentObject.ArtifactID);
		}

		[IdentifiedTest("d5b513a7-cb15-444a-a9ce-2f34ddce3112")]
		public async Task ExecuteAsync_ShouldNotCreateSourceJobTag_WhenExistsForRelatedJobHistoryId()
		{
			// Arrange
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID, _JOB_HISTORY_NAME).ConfigureAwait(false);

			RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag
			{
				Name = _sourceWorkspace.Name,
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				SourceWorkspaceName = _sourceWorkspace.Name,
				SourceInstanceName = _LOCAL_INSTANCE_NAME
			};

			int sourceCaseTagArtifactId = await Rdos.CreateRelativitySourceCaseInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID, sourceCaseTag).ConfigureAwait(false);

			RelativitySourceJobTag expectedSourceJobTag = new RelativitySourceJobTag
			{
				Name = $"{_JOB_HISTORY_NAME} - {expectedJobHistoryArtifactId}",
				JobHistoryArtifactId = expectedJobHistoryArtifactId,
				JobHistoryName = _JOB_HISTORY_NAME,
				SourceCaseTagArtifactId = sourceCaseTagArtifactId
			};

			int expectedSourceJobTagArtifactId = await Rdos.CreateRelativitySourceJobInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID, expectedSourceJobTag).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				JobHistoryArtifactId = expectedJobHistoryArtifactId
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IDestinationWorkspaceTagsCreationConfiguration>(configuration);

			// Act
			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			configuration.SourceJobTagArtifactId.Should().Be(expectedSourceJobTagArtifactId);

			List<RelativityObject> sourceJobTags = await QueryForCreatedSourceJobTagAsync(configuration.SourceJobTagArtifactId).ConfigureAwait(false);
			
			sourceJobTags.Should().ContainSingle(x => x.ArtifactID == expectedSourceJobTagArtifactId);
		}

		private async Task<RelativityObject> QueryForCreatedSourceCaseTagAsync(int sourceWorkspaceTagArtifactId)
		{
			RelativityObject tag;
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					Condition = $"'ArtifactId' == {sourceWorkspaceTagArtifactId}",
					Fields = new[]
					{
						new FieldRef
						{
							Guid = _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID
						},
					},
					IncludeNameInQueryResult = true,
					ObjectType = new ObjectTypeRef
					{
						Guid = _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID
					}
				};
				QueryResult result = await objectManager.QueryAsync(_destinationWorkspace.ArtifactID, request, 0, 1)
					.ConfigureAwait(false);

				tag = result.Objects.First();
			}

			return tag;
		}

		private async Task<List<RelativityObject>> QueryForCreatedSourceJobTagAsync(int sourceJobTagArtifactId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					Condition = $"'ArtifactId' == {sourceJobTagArtifactId}",
					Fields = new[]
					{
						new FieldRef
						{
							Guid = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID
						},
						new FieldRef
						{
							Guid = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID
						}
					},
					IncludeNameInQueryResult = true,
					ObjectType = new ObjectTypeRef
					{
						Guid = _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID
					}
				};
				QueryResult result = await objectManager.QueryAsync(_destinationWorkspace.ArtifactID, request, 0, int.MaxValue)
					.ConfigureAwait(false);

				return result.Objects;
			}
		}
	}
}