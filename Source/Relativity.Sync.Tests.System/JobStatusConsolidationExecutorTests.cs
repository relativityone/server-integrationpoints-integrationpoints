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
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public class JobStatusConsolidationExecutorTests : SystemTest
	{
		private WorkspaceRef _sourceWorkspace;
		private WorkspaceRef _destinationWorkspace;

		private IBatchRepository _batchRepository;

		private static readonly Guid CompletedItemsCountGuid = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");
		private static readonly Guid FailedItemsCountGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");
		private static readonly Guid TotalItemsCountGuid = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");

		protected override Task ChildSuiteSetup()
		{
			_batchRepository = new BatchRepository(new ServiceFactoryStub(ServiceFactory), new DateTimeWrapper());
			return Task.CompletedTask;
		}

		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();

			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask)
				.ConfigureAwait(false);

			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;
		}

		[Test]
		public async Task ItShouldDoJob()
		{
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, _sourceWorkspace.ArtifactID)
				.ConfigureAwait(false);
			int syncConfigurationArtifactId =
				await Rdos.CreateSyncConfigurationInstance(ServiceFactory, _sourceWorkspace.ArtifactID, jobHistoryArtifactId)
				.ConfigureAwait(false);

			var configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				JobHistoryArtifactId = jobHistoryArtifactId,
				SyncConfigurationArtifactId = syncConfigurationArtifactId
			};

			const int batchCount = 10;
			const int transferredItemsCountPerBatch = 10000;
			const int failedItemsCountPerBatch = 500;
			await CreateBatchesAsync(_sourceWorkspace.ArtifactID, syncConfigurationArtifactId, batchCount, transferredItemsCountPerBatch, failedItemsCountPerBatch)
				.ConfigureAwait(false);

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IJobStatusConsolidationConfiguration>(configuration);
			await syncJob.ExecuteAsync(CancellationToken.None)
				.ConfigureAwait(false);

			const int expectedTransferredItemsCount = transferredItemsCountPerBatch * batchCount;
			const int expectedFailedItemsCount = failedItemsCountPerBatch * batchCount;
			const int expectedTotalItemsCount = expectedTransferredItemsCount + expectedFailedItemsCount;

			var (actualTransferredItemsCount, actualFailedItemsCount, actualTotalItemsCount) = await ReadJobHistory(_sourceWorkspace.ArtifactID, jobHistoryArtifactId)
				.ConfigureAwait(false);

			actualTransferredItemsCount.Should().Be(expectedTransferredItemsCount);
			actualFailedItemsCount.Should().Be(expectedFailedItemsCount);
			actualTotalItemsCount.Should().Be(expectedTotalItemsCount);
		}

		private async Task<(int transferredItemsCount, int failedItemsCount, int totalItemsCount)> ReadJobHistory(int workspaceArtifactId, int jobHistoryArtifactId)
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				var readRequest = new ReadRequest
				{
					Object = new RelativityObjectRef
					{
						ArtifactID = jobHistoryArtifactId
					},
					Fields = new[]
					{
						new FieldRef
						{
							Guid = TotalItemsCountGuid
						},
						new FieldRef
						{
							Guid = CompletedItemsCountGuid
						},
						new FieldRef
						{
							Guid = FailedItemsCountGuid
						}
					}
				};
				ReadResult readResult = await objectManager.ReadAsync(workspaceArtifactId, readRequest)
					.ConfigureAwait(false);

				int completedItemsCount = (int)readResult.Object[CompletedItemsCountGuid].Value;
				int failedItemsCount = (int)readResult.Object[FailedItemsCountGuid].Value;
				int totalItemsCount = (int)readResult.Object[TotalItemsCountGuid].Value;

				return (completedItemsCount, failedItemsCount, totalItemsCount);
			}
		}

		private async Task CreateBatchesAsync(int workspaceArtifactId,
			int syncConfigurationArtifactId,
			int batchCount,
			int transferredItemsCountPerBatch,
			int failedItemsCountPerBatch)
		{
			int startingIndexCount = 0;
			int itemsCountPerBatch = transferredItemsCountPerBatch + failedItemsCountPerBatch;

			int GetStartingIndexCount()
			{
				int index = startingIndexCount;
				startingIndexCount += itemsCountPerBatch;
				return index;
			}

			IEnumerable<Task<IBatch>> batchCreationTasks = Enumerable
				.Range(0, batchCount)
				.Select(i => _batchRepository
					.CreateAsync(workspaceArtifactId,
						syncConfigurationArtifactId,
						itemsCountPerBatch,
						GetStartingIndexCount()));

			IBatch[] batches = await Task.WhenAll(batchCreationTasks).ConfigureAwait(false);

			IEnumerable<Task> setTransferredCountTasks = batches.Select(b => b.SetTransferredItemsCountAsync(transferredItemsCountPerBatch));
			IEnumerable<Task> setFailedCountTasks = batches.Select(b => b.SetFailedItemsCountAsync(failedItemsCountPerBatch));

			await Task.WhenAll(setTransferredCountTasks.Concat(setFailedCountTasks))
				.ConfigureAwait(false);
		}
	}
}