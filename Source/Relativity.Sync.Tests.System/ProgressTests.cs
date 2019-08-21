using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Nodes;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	internal sealed class ProgressTests : SystemTest
	{
		private WorkspaceRef _sourceWorkspace;

		private static readonly Guid ProgressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

		private static readonly Guid OrderGuid = new Guid("610A1E44-7AAA-47FC-8FA0-92F8C8C8A94A");
		private static readonly Guid StatusGuid = new Guid("698E1BBE-13B7-445C-8A28-7D40FD232E1B");
		private static readonly Guid ExceptionGuid = new Guid("2F2CFC2B-C9C0-406D-BD90-FB0133BCB939");
		private static readonly Guid MessageGuid = new Guid("2E296F79-1B81-4BF6-98AD-68DA13F8DA44");
		private static readonly Guid ParentArtifactGuid = new Guid("E0188DD7-4B1B-454D-AFA4-3CCC7F9DC001");

		[SetUp]
		public async Task SetUp()
		{
			_sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
		}

		[IdentifiedTest("1be9ae3e-c78f-408c-a06e-1d9359114d41")]
		public async Task ItShouldLogProgressForEachStep()
		{
			int workspaceArtifactId = _sourceWorkspace.ArtifactID;
			string jobHistoryName = $"JobHistory.{Guid.NewGuid()}";

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, workspaceArtifactId, jobHistoryName).ConfigureAwait(false);
			int syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, _sourceWorkspace.ArtifactID, jobHistoryArtifactId).ConfigureAwait(false);
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = workspaceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				SyncConfigurationArtifactId = syncConfigurationArtifactId
			};
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedAllSteps(configuration);

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			List<RelativityObject> progressRdos = await QueryForProgressRdosAsync(workspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false);

			const int nonNodeProgressSteps = 1; // MultiNode
			int minimumExpectedProgressRdos = GetSyncNodes().Count + nonNodeProgressSteps;
			progressRdos.Count.Should().Be(minimumExpectedProgressRdos);
		}

		private async Task<List<RelativityObject>> QueryForProgressRdosAsync(int workspaceId, int syncConfigurationId)
		{
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = ProgressObjectTypeGuid
					},
					Condition = $"'{ParentArtifactGuid}' == {syncConfigurationId}",
					Fields = new[]
					{
						new FieldRef
						{
							Guid = OrderGuid
						},
						new FieldRef
						{
							Guid = StatusGuid
						},
						new FieldRef
						{
							Guid = ExceptionGuid
						},
						new FieldRef
						{
							Guid = MessageGuid
						}
					}
				};

				const int maxNumResults = 100;
				QueryResult result = await objectManager.QueryAsync(workspaceId, request, 1, maxNumResults).ConfigureAwait(false);
				return result.Objects;
			}
		}

		private static List<Type> GetSyncNodes()
		{
			return Assembly.GetAssembly(typeof(SyncNode<>))
				.GetTypes()
				.Where(t => t.BaseType?.IsConstructedGenericType ?? false)
				.Where(t => t.BaseType.GetGenericTypeDefinition() == typeof(SyncNode<>))
				.ToList();
		}
	}
}
