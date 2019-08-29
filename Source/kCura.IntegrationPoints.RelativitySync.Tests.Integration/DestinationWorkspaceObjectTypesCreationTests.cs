using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.RelativitySync.Adapters;
using kCura.IntegrationPoints.RelativitySync.Tests.Integration.Stubs;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class DestinationWorkspaceObjectTypesCreationTests : IntegrationTestBase
	{
		private DestinationWorkspaceObjectTypesCreation _instance;
		private DestinationWorkspaceObjectTypesCreationConfigurationStub _configuration;

		private int _workspaceId;

		[SetUp]
		public void SetUp()
		{
			_workspaceId = Workspace.CreateWorkspace(Guid.NewGuid().ToString());

			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IHelper>().Instance(Helper));
			_instance = new DestinationWorkspaceObjectTypesCreation(container);

			_configuration = new DestinationWorkspaceObjectTypesCreationConfigurationStub
			{
				DestinationWorkspaceArtifactId = _workspaceId
			};
		}

		[TearDown]
		public void TearDown()
		{
			if (_workspaceId != 0)
			{
				Workspace.DeleteWorkspace(_workspaceId);
			}
		}

		[IdentifiedTest("141b0223-1c4a-4dd8-a261-918e695f7f28")]
		public async Task ItShouldCreateObjectTypes()
		{
			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT

			await AssertSourceWorkspaceFields().ConfigureAwait(false);

			await AssertSourceJobFields().ConfigureAwait(false);

			await AssertDocumentFields().ConfigureAwait(false);
		}

		[IdentifiedTest("1b73724f-a09b-4b6a-8d90-13cb0192bd7e")]
		public async Task ItShouldBeAbleToExecuteMultipleTimesOnTheSameWorkspace()
		{
			// ACT
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);
			await _instance.ExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.Pass();
		}

		// NOTE: We aren't using [TestCase] here b/c that would require creating a workspace for each test case.
		// These tests are otherwise very quick and we don't really expect them to fail. Change these to
		// TestCases if they get any more complicated.
		[IdentifiedTest("c804cbf0-0aee-4ed2-aab9-4f5239c9d3e7")]
		public async Task ItShouldAlwaysExecute()
		{
			foreach (bool isSourceJobArtifactTypeIdSet in TrueFalse())
			{
				foreach (bool isSourceWorkspaceArtifactTypeIdSet in TrueFalse())
				{
					_configuration.IsSourceJobArtifactTypeIdSet = isSourceJobArtifactTypeIdSet;
					_configuration.IsSourceWorkspaceArtifactTypeIdSet = isSourceWorkspaceArtifactTypeIdSet;

					// ACT
					bool result = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

					// ASSERT
					Assert.IsTrue(result,
						"CanExecuteAsync was not true for (IsSourceJobArtifactTypeIdSet={0}, IsSourceWorkspaceArtifactTypeIdSet={1})",
						isSourceJobArtifactTypeIdSet,
						isSourceWorkspaceArtifactTypeIdSet);
				}
			}
		}

		private async Task AssertSourceWorkspaceFields()
		{
			List<RelativityObject> sourceWorkspaceObjectFields = await GetObjectFieldsAsync(Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME).ConfigureAwait(false);

			//System Fields (6) + Relativity Source Case, Source Instance Name, Source Workspace Artifact ID, Source Workspace Name, Relativity Source Case::Relativity Image Count
			Assert.AreEqual(11, sourceWorkspaceObjectFields.Count);

			Assert.IsTrue(sourceWorkspaceObjectFields.Any(x => x.Guids.Any(y => y == SourceWorkspaceDTO.Fields.CaseIdFieldNameGuid)));
			Assert.IsTrue(sourceWorkspaceObjectFields.Any(x => x.Guids.Any(y => y == SourceWorkspaceDTO.Fields.CaseNameFieldNameGuid)));
			Assert.IsTrue(sourceWorkspaceObjectFields.Any(x => x.Guids.Any(y => y == SourceWorkspaceDTO.Fields.InstanceNameFieldGuid)));
		}

		private async Task AssertSourceJobFields()
		{
			List<RelativityObject> sourceJobObjectFields = await GetObjectFieldsAsync(Constants.SPECIAL_SOURCEJOB_FIELD_NAME).ConfigureAwait(false);

			//System Fields (6) + Job History Artifact ID, Job History Name, Relativity Source Job, RelativitySourceCase, Relativity Source Job::Relativity Image Count
			Assert.AreEqual(11, sourceJobObjectFields.Count);

			Assert.IsTrue(sourceJobObjectFields.Any(x => x.Guids.Any(y => y == SourceJobDTO.Fields.JobHistoryIdFieldGuid)));
			Assert.IsTrue(sourceJobObjectFields.Any(x => x.Guids.Any(y => y == SourceJobDTO.Fields.JobHistoryNameFieldGuid)));
		}

		private async Task AssertDocumentFields()
		{
			List<RelativityObject> documentFields = await GetObjectFieldsAsync("Document").ConfigureAwait(false);

			Assert.IsTrue(documentFields.Any(x => x.Guids.Any(y => y == SourceJobDTO.Fields.JobHistoryFieldOnDocumentGuid)));
			Assert.IsTrue(documentFields.Any(x => x.Guids.Any(y => y == SourceWorkspaceDTO.Fields.SourceWorkspaceFieldOnDocumentGuid)));
		}

		private async Task<List<RelativityObject>> GetObjectFieldsAsync(string objectName)
		{
			using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Name = "Field"
					},
					Fields = new[]
					{
						new FieldRef
						{
							Name = "Name"
						},
						new FieldRef
						{
							Name = "Guid"
						}
					},
					Condition = $"('Object Type' IN ['{objectName}'])"
				};
				QueryResult result = await objectManager.QueryAsync(_workspaceId, request, 0, 200).ConfigureAwait(false);
				return result.Objects;
			}
		}

		private IEnumerable<bool> TrueFalse()
		{
			yield return true;
			yield return false;
		}
	}
}