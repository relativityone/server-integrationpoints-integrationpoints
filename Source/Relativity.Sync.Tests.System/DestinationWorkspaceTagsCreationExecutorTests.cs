using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.Services.ApplicationInstallManager;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Integration;
using Relativity.Sync.Tests.Integration.Stubs;
using Relativity.Sync.Tests.System.Stubs;
using Field = kCura.Relativity.Client.DTOs.Field;
using FieldType = kCura.Relativity.Client.FieldType;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class DestinationWorkspaceTagsCreationExecutorTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private WorkspaceRef _sourceWorkspace;

		private const int _DEFAULT_NAME_FIELD_LENGTH = 255;
		private const string _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_NAME = "Relativity Source Case";
		private const string _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";
		private const string _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_NAME = "Job History Artifact ID";
		private const string _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_NAME = "Job History Name";

		private const string _RELATIVITY_SOURCE_CASE_CASE_ID_FIELD_NAME = "Source Workspace Artifact ID";
		private const string _RELATIVITY_SOURCE_CASE_NAME_FIELD_NAME = "Source Workspace Name";
		private const string _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_NAME = "Source Instance Name";
		private const string _JOB_HISTORY_NAME = "Test Job Name";
		private const string _LOCAL_INSTANCE_NAME = "This Instance";
		private const int _USER_ID = 9;
		private static readonly Guid _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
		private static readonly Guid _INTEGRATION_POINT_GUID = Guid.Parse("DCF6E9D1-22B6-4DA3-98F6-41381E93C30C");

		private static readonly Guid _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
		private static readonly Guid _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
		private static readonly Guid _JOB_HISTORY_GUID = Guid.Parse("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9");


		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = CreateWorkspaceWithFields();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = CreateWorkspaceWithFields();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;
		}

		[Test]
		public async Task ItShouldCreateTagsIfTheyDoesNotExist()
		{
			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			string expectedSourceWorkspaceName = _sourceWorkspace.Name;
			string expectedSourceCaseTagName = $"{_LOCAL_INSTANCE_NAME} - {expectedSourceWorkspaceName} - {expectedSourceWorkspaceArtifactId}";
			int expectedJobHistoryArtifactId = await CreateJobHistoryInstance(expectedSourceWorkspaceArtifactId).ConfigureAwait(false);
			string expectedSourceJobTagName = $"{_JOB_HISTORY_NAME} - {expectedJobHistoryArtifactId}";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobArtifactId = expectedJobHistoryArtifactId,
				ExecutingUserId = _USER_ID
			};

			// ACT
			ISyncJob syncJob = CreateSyncJob(configuration);


			// ASSERT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			RelativityObject sourceCaseTag = await QueryForCreatedSourceCaseTag(configuration.SourceWorkspaceTagArtifactId).ConfigureAwait(false);
			RelativityObject sourceJobTag = await QueryForCreatedSourceJobTag(configuration.SourceJobTagArtifactId).ConfigureAwait(false);

			Assert.AreEqual(expectedSourceWorkspaceArtifactId, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_ID_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceWorkspaceName, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(_LOCAL_INSTANCE_NAME, sourceCaseTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceCaseTagName, sourceCaseTag.Name);

			Assert.AreEqual(sourceCaseTag.ArtifactID, sourceJobTag.ParentObject.ArtifactID);
			Assert.AreEqual(expectedJobHistoryArtifactId, sourceJobTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID)).Value);
			Assert.AreEqual(_JOB_HISTORY_NAME, sourceJobTag.FieldValues.First(x => x.Field.Guids.Contains(_RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID)).Value);
			Assert.AreEqual(expectedSourceJobTagName, sourceJobTag.Name);
		}

		[Test]
		public async Task ItShouldUpdateSourceCaseTagAndCreateJobTag()
		{
			string wrongSourceTagName = "Definitely not a correct name";
			string wrongSourceWorkspaceName = "Wrong source workspace name";

			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			string expectedSourceWorkspaceName = _sourceWorkspace.Name;
			string expectedSourceCaseTagName = $"{_LOCAL_INSTANCE_NAME} - {expectedSourceWorkspaceName} - {expectedSourceWorkspaceArtifactId}";

			int expectedJobHistoryArtifactId = await CreateJobHistoryInstance(expectedSourceWorkspaceArtifactId).ConfigureAwait(false);
			int expectedSourceCaseTagArtifactId =
				await CreateRelativitySourceCaseInstance(_destinationWorkspace.ArtifactID, wrongSourceTagName, expectedSourceWorkspaceArtifactId, wrongSourceWorkspaceName, _LOCAL_INSTANCE_NAME)
					.ConfigureAwait(false);
			string expectedSourceJobTagName = $"{_JOB_HISTORY_NAME} - {expectedJobHistoryArtifactId}";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobArtifactId = expectedJobHistoryArtifactId,
				ExecutingUserId = _USER_ID
			};

			// ACT
			ISyncJob syncJob = CreateSyncJob(configuration);


			// ASSERT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			RelativityObject sourceCaseTag = await QueryForCreatedSourceCaseTag(configuration.SourceWorkspaceTagArtifactId).ConfigureAwait(false);
			RelativityObject sourceJobTag = await QueryForCreatedSourceJobTag(configuration.SourceJobTagArtifactId).ConfigureAwait(false);

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

		private async Task InstallIntegrationPoints(int workspaceArtifactId)
		{
			using (var applicationInstallManager = ServiceFactory.CreateProxy<IApplicationInstallManager>())
			{
				await applicationInstallManager.InstallLibraryApplicationByGuid(workspaceArtifactId, _INTEGRATION_POINT_GUID).ConfigureAwait(false);
			}
		}

		private async Task CreateRelativitySourceJobObjectType(int workspaceId, int relativitySourceCaseArtifactTypeId)
		{
			var objectType = new ObjectType(_RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID)
			{
				Name = _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_NAME,
				ParentArtifactTypeID = relativitySourceCaseArtifactTypeId,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false
			};

			var objectTypeFields = new List<Field>
			{
				new Field
				{
					Name = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_NAME,
					Guids = new List<Guid> {_RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID},
					FieldTypeID = FieldType.WholeNumber,
					ObjectType = objectType,
					IsRequired = true,
					Linked = false,
					OpenToAssociations = false,
					AllowSortTally = false,
					AllowGroupBy = false,
					AllowPivot = false,
					Width = "100",
					Wrapping = false
				},
				new Field
				{
					Name = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_NAME,
					Guids = new List<Guid> {_RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID},
					FieldTypeID = FieldType.FixedLengthText,
					ObjectType = objectType,
					IsRequired = true,
					IncludeInTextIndex = false,
					Linked = false,
					AllowHTML = false,
					AllowSortTally = false,
					AllowGroupBy = false,
					AllowPivot = false,
					OpenToAssociations = false,
					Width = "100",
					Wrapping = false,
					Unicode = false,
					Length = _DEFAULT_NAME_FIELD_LENGTH
				}

			};

			await CreateObjectTypeWithFields(workspaceId, _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID, objectType, objectTypeFields).ConfigureAwait(false);

		}

		private async Task<int> CreateRelativitySourceCaseObjectType(int workspaceId)
		{
			var objectType = new ObjectType(_RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID)
			{
				Name = _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_NAME,
				ParentArtifactTypeID = (int)ArtifactType.Case,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false
			};

			var objectTypeFields = new List<Field>
			{
				{
					new Field
					{
						Name = _RELATIVITY_SOURCE_CASE_CASE_ID_FIELD_NAME,
						Guids = new List<Guid> {_RELATIVITY_SOURCE_CASE_ID_FIELD_GUID},
						FieldTypeID = FieldType.WholeNumber,
						IsRequired = true,
						Linked = false,
						OpenToAssociations = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						Width = "100",
						Wrapping = false
					}
				},
				{
					new Field
					{
						Name = _RELATIVITY_SOURCE_CASE_NAME_FIELD_NAME,
						Guids = new List<Guid> {_RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID},
						FieldTypeID = FieldType.FixedLengthText,
						IsRequired = true,
						IncludeInTextIndex = false,
						Linked = false,
						AllowHTML = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						OpenToAssociations = false,
						Width = "100",
						Wrapping = false,
						Unicode = false,
						Length = _DEFAULT_NAME_FIELD_LENGTH
					}
				},
				{
					new Field
					{
						Name = _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_NAME,
						Guids = new List<Guid> {_RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID},
						FieldTypeID = FieldType.FixedLengthText,
						IsRequired = true,
						IncludeInTextIndex = false,
						Linked = false,
						AllowHTML = false,
						AllowSortTally = false,
						AllowGroupBy = false,
						AllowPivot = false,
						OpenToAssociations = false,
						Width = "100",
						Wrapping = false,
						Unicode = false,
						Length = _DEFAULT_NAME_FIELD_LENGTH
					}
				}
			};

			return await CreateObjectTypeWithFields(workspaceId, _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID, objectType, objectTypeFields).ConfigureAwait(false);
		}

		private async Task<int> CreateObjectTypeWithFields(int workspaceArtifactId, Guid objectTypeGuid, ObjectType objectType, List<Field> objectTypeFields)
		{
			int tmpWorkspaceArtifactId = Client.APIOptions.WorkspaceID;
			try
			{
				Client.APIOptions.WorkspaceID = workspaceArtifactId;
				int objectArtifactId = Client.Repositories.ObjectType.CreateSingle(objectType);
				using (var guidManager = ServiceFactory.CreateProxy<IArtifactGuidManager>())
				{
					await guidManager.CreateSingleAsync(workspaceArtifactId, objectArtifactId, new List<Guid> { objectTypeGuid }).ConfigureAwait(false);



					ObjectType objectTypeWithDescriptorArtifactTypeIdSet = Client.Repositories.ObjectType.ReadSingle(objectTypeGuid);

					foreach (var field in objectTypeFields)
					{
						field.ObjectType = objectTypeWithDescriptorArtifactTypeIdSet;
						int fieldArtifactId = Client.Repositories.Field.CreateSingle(field);
						await guidManager.CreateSingleAsync(workspaceArtifactId, fieldArtifactId, new List<Guid> { field.Guids.First() }).ConfigureAwait(false);
					}

					return objectTypeWithDescriptorArtifactTypeIdSet.DescriptorArtifactTypeID.GetValueOrDefault(-1);
				}
			}
			finally
			{
				Client.APIOptions.WorkspaceID = tmpWorkspaceArtifactId;
			}
		}

		private async Task<int> CreateJobHistoryInstance(int workspaceId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Value = _JOB_HISTORY_NAME,
							Field = new FieldRef
							{
								Name = "Name"
							}
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = _JOB_HISTORY_GUID
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		private async Task<int> CreateRelativitySourceCaseInstance(int workspaceId, string sourceTagName, int sourceWorkspaceArtifactId, string sourceWorkspaceName, string instanceName)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef { Name = "Name"},
							Value = sourceTagName
						},new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID},
							Value = sourceWorkspaceArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID },
							Value = sourceWorkspaceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID },
							Value = instanceName
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		private ISyncJob CreateSyncJob(ConfigurationStub configuration)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			ContainerFactory factory = new ContainerFactory();
			SyncJobParameters syncParameters = new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId);
			factory.RegisterSyncDependencies(containerBuilder, syncParameters, new SyncConfiguration(), new EmptyLogger());

			new SystemTestsInstaller().Install(containerBuilder);

			IntegrationTestsContainerBuilder.RegisterExternalDependenciesAsMocks(containerBuilder);
			IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceTagsCreationConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			return containerBuilder.Build().Resolve<ISyncJob>();
		}

		private async Task<RelativityObject> QueryForCreatedSourceCaseTag(int sourceWorkspaceTagArtifactId)
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

		private async Task<RelativityObject> QueryForCreatedSourceJobTag(int sourceJobTagArtifactId)
		{
			RelativityObject tag;
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
				QueryResult result = await objectManager.QueryAsync(_destinationWorkspace.ArtifactID, request, 0, 1)
					.ConfigureAwait(false);

				tag = result.Objects.First();
			}

			return tag;
		}

		private async Task<WorkspaceRef> CreateWorkspaceWithFields()
		{
			WorkspaceRef workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
			await Environment.CreateFieldsInWorkspace(workspace.ArtifactID).ConfigureAwait(false);
			return workspace;
		}
	}
}