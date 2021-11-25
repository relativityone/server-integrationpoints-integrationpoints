﻿using Autofac;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Helpers.APIHelper;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	[TestFixture]
	internal class NonDocumentGoldFlowTests : SystemTest
	{
		private readonly int _entityArtifactTypeId = 1000061; // TODO query with OM
		private readonly Guid _entityGuid = new Guid("d216472d-a1aa-4965-8b36-367d43d4e64c"); // TODO use artifact type id
		private const string ViewName = "Entities - Legal Hold View";

		private WorkspaceRef _sourceWorkspace;
		private WorkspaceRef _destinationWorkspace;
		private int _viewArtifactId;

		protected override async Task ChildSuiteSetup()
		{
			const int sourceWorkspaceId = 1018642;
			const int destinationWorkspaceId = 1018645;

			_sourceWorkspace = await Environment.GetWorkspaceAsync(sourceWorkspaceId).ConfigureAwait(false);
			_destinationWorkspace = await Environment.GetWorkspaceAsync(destinationWorkspaceId).ConfigureAwait(false);

			await Environment.InstallCustomHelperAppAsync(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
			await Environment.InstallLegalHoldToWorkspaceAsync(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
			await Environment.InstallLegalHoldToWorkspaceAsync(_destinationWorkspace.ArtifactID).ConfigureAwait(false);
			await CreateEntitiesAsync(3).ConfigureAwait(false);

			_viewArtifactId = await GetViewArtifactIdAsync(ViewName).ConfigureAwait(false);
		}

		[IdentifiedTest("C721DA78-1D27-4463-B49C-9A9E9E65F700")]
		public async Task SyncJob_Should_SyncEntities()
		{
			// Arrange

			int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now:yyyy MMMM dd HH.mm.ss.fff}", CustomAppGuids.JobHistory.TypeGuid).ConfigureAwait(false);

			SyncConfigurationBuilder builder = new SyncConfigurationBuilder(
				new SyncContext(_sourceWorkspace.ArtifactID, _destinationWorkspace.ArtifactID, jobHistoryId),
				new ServicesManagerStub());

			int syncConfigurationId = await builder
				.ConfigureRdos(CustomAppGuids.Guids)
				.ConfigureNonDocumentSync(new NonDocumentSyncOptions(_viewArtifactId, _entityArtifactTypeId, _entityArtifactTypeId))
				.WithFieldsMapping(mappingBuilder => mappingBuilder.WithIdentifier())
				.SaveAsync()
				.ConfigureAwait(false);

			ISyncJobFactory syncJobFactory = new SyncJobFactory();

			ContainerBuilder containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(new SyncDataAndUserConfiguration(executingUserId: 9)).As<IUserContextConfiguration>();

			SyncJobParameters syncJobParameters = new SyncJobParameters(syncConfigurationId, _sourceWorkspace.ArtifactID, Guid.NewGuid());
			IRelativityServices relativityServices = new RelativityServices(new NullAPM(), new ServicesManagerStub(), AppSettings.RelativityUrl, new TestHelper());

			ISyncJob syncJob = syncJobFactory.Create(
				containerBuilder.Build(),
				syncJobParameters,
				relativityServices,
				Logger);

			// Act

			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

			// Assert
			// TODO
		}

		private async Task CreateEntitiesAsync(int entitiesCount)
		{
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				ObjectTypeRef entityObjectType = new ObjectTypeRef()
				{
					Guid = _entityGuid
				};

				// Create Manager Entity

				CreateResult managerCreateResult = await objectManager.CreateAsync(_sourceWorkspace.ArtifactID, new CreateRequest()
				{
					ObjectType = entityObjectType,
					FieldValues = new[]
					{
						new FieldRefValuePair()
						{
							Field = new FieldRef()
							{
								Name = "Full Name"
							},
							Value = "Some Manager"
						}
					}
				}).ConfigureAwait(false);

				// Create Entities linked to Manager

				FieldRef[] fields = new[]
				{
					new FieldRef()
					{
						Name = "Full Name"
					},
					new FieldRef()
					{
						Name = "Manager"
					}
				};

				IReadOnlyList<IReadOnlyList<object>> values = Enumerable
					.Range(0, entitiesCount)
					.Select(i => new List<object>()
					{
						$"User {i}",
						new RelativityObjectRef()
						{
							ArtifactID = managerCreateResult.Object.ArtifactID
						}
					})
					.ToList();

				MassCreateResult massCreateResult = await objectManager.CreateAsync(_sourceWorkspace.ArtifactID, new MassCreateRequest()
				{
					ObjectType = entityObjectType,
					Fields = fields,
					ValueLists = values
				}, CancellationToken.None).ConfigureAwait(false);
			}
		}

		private async Task<int> GetViewArtifactIdAsync(string viewName)
		{
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryResult queryResult = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, new QueryRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = (int)ArtifactType.View
					},
					Condition = $"'Name' == '{viewName}'"
				}, 0, 1).ConfigureAwait(false);

				if (queryResult.Objects.Count == 0)
				{
					throw new Exception($"Can't find view: {viewName}");
				}

				return queryResult.Objects[0].ArtifactID;
			}
		}

	}
}
