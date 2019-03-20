using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Integration.Stubs;
using Relativity.Sync.Tests.System.Stub;
using UsernamePasswordCredentials = kCura.Relativity.Client.UsernamePasswordCredentials;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;
using Field = kCura.Relativity.Client.DTOs.Field;
using FieldType = kCura.Relativity.Client.FieldType;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class SourceWorkspaceTagsCreationExecutorTests : SystemTest
	{
		private Workspace _workspace;
		private Workspace _destinationWorkspace;
		private ServiceFactory _serviceFactory;

		private const int _WORKSPACE_OBJECT_TYPE_ARTIFACT_TYPE_ID = 8;

		private readonly Guid _documentObjectTypeGuid = Guid.Parse("15C36703-74EA-4FF8-9DFB-AD30ECE7530D");
		private readonly Guid _destinationWorkspaceObjectTypeGuid = Guid.Parse("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private readonly Guid _jobHistoryObjectTypeGuid = Guid.Parse("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9");

		private readonly Guid _documentsFieldInDestinationWorkspaceGuid = Guid.Parse("94EE2BD7-76D5-4D17-99E2-04768CCE05E6");
		private readonly Guid _jobHistoryFieldInDestinationWorkspaceGuid = Guid.Parse("07B8A468-DEC8-45BD-B50A-989A35150BE2");
		private readonly Guid _destinationWorkspaceArtifactIdFieldInDestinationWorkspaceGuid = Guid.Parse("207E6836-2961-466B-A0D2-29974A4FAD36");
		private readonly Guid _destinationWorkspaceNameFieldInDestinationWorkspaceGuid = Guid.Parse("348D7394-2658-4DA4-87D0-8183824ADF98");
		private readonly Guid _destinationInstanceNameFieldInDestinationWorkspaceGuid = Guid.Parse("909ADC7C-2BB9-46CA-9F85-DA32901D6554");
		private readonly Guid _destinationInstanceArtifactIdFieldInDestinationWorkspaceGuid = Guid.Parse("323458DB-8A06-464B-9402-AF2516CF47E0");

		private readonly Guid _documentsFieldInJobHistoryGuid = Guid.Parse("94EE2BD7-76D5-4D17-99E2-04768CCE05E6");

		[OneTimeSetUp]
		protected override void ChildSuiteSetup()
		{
			_serviceFactory = CreateServideFactory();
		}

		[SetUp]
		public void SetUp()
		{
			_workspace = CreateWorkspaceAsync().Result;
			_destinationWorkspace = CreateWorkspaceAsync().Result;
			AddObjectTypes(_workspace.ArtifactID);
		}

		public void AddObjectTypes(int workspaceId)
		{
			CreateObjectType(workspaceId, _destinationWorkspaceObjectTypeGuid, "Destination Workspace", _WORKSPACE_OBJECT_TYPE_ARTIFACT_TYPE_ID);

			CreateObjectType(workspaceId, _jobHistoryObjectTypeGuid, "Job History", _WORKSPACE_OBJECT_TYPE_ARTIFACT_TYPE_ID);

			CreateField(workspaceId, BuildField(_documentsFieldInDestinationWorkspaceGuid, _destinationWorkspaceObjectTypeGuid, "Documents", FieldType.MultipleObject, _documentObjectTypeGuid));
			CreateField(workspaceId, BuildField(_jobHistoryFieldInDestinationWorkspaceGuid, _destinationWorkspaceObjectTypeGuid, "Job History", FieldType.MultipleObject, _jobHistoryObjectTypeGuid));
			CreateField(workspaceId, BuildField(_destinationWorkspaceArtifactIdFieldInDestinationWorkspaceGuid, _destinationWorkspaceObjectTypeGuid, "Destination Workspace Artifact ID", FieldType.WholeNumber));
			CreateField(workspaceId, BuildField(_destinationWorkspaceNameFieldInDestinationWorkspaceGuid, _destinationWorkspaceObjectTypeGuid, "Destination Workspace Name", FieldType.FixedLengthText));
			CreateField(workspaceId, BuildField(_destinationInstanceNameFieldInDestinationWorkspaceGuid, _destinationWorkspaceObjectTypeGuid, "Destination Instance Name", FieldType.FixedLengthText));
			CreateField(workspaceId, BuildField(_destinationInstanceArtifactIdFieldInDestinationWorkspaceGuid, _destinationWorkspaceObjectTypeGuid, "Destination Instance Artifact ID", FieldType.WholeNumber));

			CreateField(workspaceId, BuildField(_documentsFieldInJobHistoryGuid, _destinationWorkspaceObjectTypeGuid, "Documents", FieldType.MultipleObject, _documentObjectTypeGuid));
		}

		public Field BuildField(Guid fieldTypeGuid, Guid objectTypeGuid, string fieldName, FieldType fieldType)
		{
			return BuildField(fieldTypeGuid, objectTypeGuid, fieldName, fieldType, null);
		}

		public Field BuildField(Guid fieldTypeGuid, Guid objectTypeGuid, string fieldName, FieldType fieldType, Guid? referencedObjectTypeGuid)
		{
			var field = new Field(fieldTypeGuid)
			{
				Name = fieldName,
				ArtifactTypeGuids = new List<Guid>() { objectTypeGuid },
				FieldTypeID = fieldType,
				AssociativeObjectType = (referencedObjectTypeGuid == null) ? null : new ObjectType(referencedObjectTypeGuid.Value)
			};

			return field;
		}

		public void CreateField(int workspaceId, Field field)
		{
			Client.APIOptions.WorkspaceID = workspaceId;

			Client.Repositories.Field.CreateSingle(field);
		}

		public void CreateObjectType(int workspaceId, Guid objectTypeGuid, string objectTypeName, int parentArtifactTypeId)
		{
			var objectType = new ObjectType(objectTypeGuid)
			{
				Name = objectTypeName,
				ParentArtifactTypeID = parentArtifactTypeId,
				CopyInstancesOnParentCopy = false,
				CopyInstancesOnWorkspaceCreation = false,
				SnapshotAuditingEnabledOnDelete = false,
				Pivot = true,
				Sampling = false,
				PersistentLists = false
			};

			Client.APIOptions.WorkspaceID = workspaceId;

			Client.Repositories.ObjectType.CreateSingle(objectType);
		}

		public async Task<int> CreateJobHistoryInstance(int workspaceId)
		{
			using (var objectManager = _serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest()
				{
					FieldValues = new[]
					{
						new FieldRefValuePair()
						{
							Value = Guid.NewGuid().ToString(),
							Field = new FieldRef()
							{
								Name = "Name"
							}
						}
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		private ISyncJob CreateSyncJob(ConfigurationStub configuration)
		{
			// The setup will be changed once the new Containerbuilder changes are merged in.
			ContainerBuilder containerBuilder = new ContainerBuilder();
			List<IInstaller> installers = Assembly.GetAssembly(typeof(IInstaller))
				.GetTypes()
				.Where(t => !t.IsAbstract && t.IsAssignableTo<IInstaller>())
				.Select(t => (IInstaller)Activator.CreateInstance(t))
				.ToList();
			installers.Add(new SystemTestsInstaller());

			containerBuilder.RegisterGeneric(typeof(ExecutorStub<>)).As(typeof(IExecutor<>));
			containerBuilder.RegisterGeneric(typeof(ExecutionConstrainsStub<>)).As(typeof(IExecutionConstrains<>));


			containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutionConstrains>()
				.As<IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>>();
			containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();

			Mock<ISyncLog> loggerMock = new Mock<ISyncLog>();
			containerBuilder.RegisterInstance(loggerMock.Object).As<ISyncLog>();
			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			IContainer container = containerBuilder.Build();

			var syncJobFactory = new SyncJobFactory();
			return syncJobFactory.Create(container, installers, new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId));
		}

		[Test]
		public async Task ItShouldCreateTagIfItDoesNotExist()
		{
			int jobHistoryArtifactId = await CreateJobHistoryInstance(_workspace.ArtifactID).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = _workspace.ArtifactID,
				JobArtifactId = jobHistoryArtifactId
			};

			ISyncJob syncJob = CreateSyncJob(configuration);
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			int destinationWorkspaceTagArtifactId = configuration.DestinationWorkspaceTagArtifactId;

			using (var objectManager = _serviceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"'ArtifactId' == {destinationWorkspaceTagArtifactId}",
					Fields = new []
					{
						new FieldRef()
						{
							Guid = _jobHistoryFieldInDestinationWorkspaceGuid
						},
						new FieldRef()
						{
							Guid = _destinationWorkspaceArtifactIdFieldInDestinationWorkspaceGuid
						},
						new FieldRef()
						{
							Guid = _destinationWorkspaceNameFieldInDestinationWorkspaceGuid
						},
						new FieldRef()
						{
							Guid = _destinationInstanceNameFieldInDestinationWorkspaceGuid
						},
						new FieldRef()
						{
							Guid = _destinationInstanceArtifactIdFieldInDestinationWorkspaceGuid
						}
					}
				};
				QueryResult result = await objectManager.QueryAsync(_workspace.ArtifactID, request, 0, 1).ConfigureAwait(false);

				RelativityObject tag = result.Objects.First();
				
				Assert.AreEqual(_destinationWorkspace.ArtifactID, tag.FieldValues.First(x => x.Field.Guids.Contains(_destinationWorkspaceArtifactIdFieldInDestinationWorkspaceGuid)).Value);
				Assert.AreEqual(_destinationWorkspace.Name, tag.FieldValues.First(x => x.Field.Guids.Contains(_destinationWorkspaceNameFieldInDestinationWorkspaceGuid)).Value);
				Assert.AreEqual(jobHistoryArtifactId, tag.FieldValues.First(x => x.Field.Guids.Contains(_jobHistoryFieldInDestinationWorkspaceGuid)).Value);
			}
		}

		[Test]
		public Task ItShouldUpdateTagIfItDoesExist()
		{

		}
	}
}
