using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;
using Relativity.Services.ApplicationInstallManager;
using Relativity.Services.LibraryApplicationsManager;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Executors;
using Relativity.Sync.Tests.Integration.Stubs;
using Relativity.Sync.Tests.System.Stubs;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;
using Field = kCura.Relativity.Client.DTOs.Field;
using FieldType = kCura.Relativity.Client.FieldType;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class SourceWorkspaceTagsCreationExecutorTests : SystemTest
	{
		private Workspace _sourceWorkspace;
		private Workspace _destinationWorkspace;

		private readonly Guid _jobHistoryObjectTypeGuid = Guid.Parse("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9");
		private readonly Guid _destinationWorkspaceObjectTypeGuid = Guid.Parse("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");

		private readonly Guid _jobHistoryFieldInDestinationWorkspaceGuid = Guid.Parse("07B8A468-DEC8-45BD-B50A-989A35150BE2");
		private readonly Guid _destinationWorkspaceArtifactIdFieldInDestinationWorkspaceGuid = Guid.Parse("207E6836-2961-466B-A0D2-29974A4FAD36");
		private readonly Guid _destinationWorkspaceNameFieldInDestinationWorkspaceGuid = Guid.Parse("348D7394-2658-4DA4-87D0-8183824ADF98");
		private readonly Guid _destinationInstanceNameFieldInDestinationWorkspaceGuid = Guid.Parse("909ADC7C-2BB9-46CA-9F85-DA32901D6554");
		private readonly Guid _destinationInstanceArtifactIdFieldInDestinationWorkspaceGuid = Guid.Parse("323458DB-8A06-464B-9402-AF2516CF47E0");

		private readonly Guid _integrationPointsGuid = Guid.Parse("DCF6E9D1-22B6-4DA3-98F6-41381E93C30C");

		[SetUp]
		public async Task SetUp()
		{
			_sourceWorkspace = CreateWorkspaceAsync().Result;
			_destinationWorkspace = CreateWorkspaceAsync().Result;
			await InstallIntegrationPoints(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
		}

		private async Task InstallIntegrationPoints(int workspaceArtifactId)
		{
			using (var applicationInstallManager = ServiceFactory.CreateProxy<IApplicationInstallManager>())
			{
				await applicationInstallManager.InstallLibraryApplicationByGuid(workspaceArtifactId, _integrationPointsGuid).ConfigureAwait(false);
			}
		}

		public async Task<int> CreateJobHistoryInstance(int workspaceId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
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
					},
					ObjectType = new ObjectTypeRef()
					{
						Guid = _jobHistoryObjectTypeGuid
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

			containerBuilder.RegisterInstance(configuration).AsImplementedInterfaces();

			IContainer container = containerBuilder.Build();

			var syncJobFactory = new SyncJobFactory();
			return syncJobFactory.Create(container, installers, new SyncJobParameters(configuration.JobArtifactId, configuration.SourceWorkspaceArtifactId));
		}

		private async Task<RelativityObject> QueryForCreatedTag(int destinationWorkspaceTagArtifactId)
		{
			RelativityObject tag;
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"'ArtifactId' == {destinationWorkspaceTagArtifactId}",
					Fields = new[]
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
					},
					ObjectType = new ObjectTypeRef()
					{
						Guid = _destinationWorkspaceObjectTypeGuid
					}
				};
				QueryResult result = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, request, 0, 1)
					.ConfigureAwait(false);

				tag = result.Objects.First();
			}

			return tag;
		}

		[Test]
		public async Task ItShouldCreateTagIfItDoesNotExist()
		{
			int jobHistoryArtifactId = await CreateJobHistoryInstance(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
			const int userId = 9;

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				JobArtifactId = jobHistoryArtifactId,
				ExecutingUserId = userId
			};

			ISyncJob syncJob = CreateSyncJob(configuration);

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			RelativityObject tag = await QueryForCreatedTag(configuration.DestinationWorkspaceTagArtifactId)
				.ConfigureAwait(false);

			Assert.AreEqual(_destinationWorkspace.ArtifactID, tag.FieldValues.First(x => x.Field.Guids.Contains(_destinationWorkspaceArtifactIdFieldInDestinationWorkspaceGuid)).Value);
			Assert.AreEqual(_destinationWorkspace.Name, tag.FieldValues.First(x => x.Field.Guids.Contains(_destinationWorkspaceNameFieldInDestinationWorkspaceGuid)).Value);

			var relativityObjectValues = (List<RelativityObjectValue>)tag.FieldValues
				.First(x => x.Field.Guids.Contains(_jobHistoryFieldInDestinationWorkspaceGuid)).Value;
			Assert.AreEqual(1, relativityObjectValues.Count);
			Assert.AreEqual(jobHistoryArtifactId, relativityObjectValues.First().ArtifactID);
		}

		[Test]
		public void ItShouldUpdateTagIfItDoesExist()
		{
		}
	}
}
