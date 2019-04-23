using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class SnapshotPartitionExecutorTests : SystemTest
	{
		private int _workspaceId;
		private int _syncConfigurationId;
		private SnapshotPartitionExecutor _instance;
		private IBatchRepository _batchRepository;

		private static readonly Guid SyncConfigurationRelationGuid = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid _SYNC_BATCH_OBJECT_TYPE = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");

		private static ObjectTypeRef SyncBatchObjectTypeRef => new ObjectTypeRef { Guid = _SYNC_BATCH_OBJECT_TYPE };

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync()
				.ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			int jobHistoryId = await Rdos.CreateJobHistoryInstance(ServiceFactory, _workspaceId).ConfigureAwait(false);
			_syncConfigurationId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, _workspaceId, jobHistoryId).ConfigureAwait(false);

			_batchRepository = new BatchRepository(new SourceServiceFactoryForAdminStub(ServiceFactory));
			_instance = new SnapshotPartitionExecutor(_batchRepository, Mock.Of<ISyncLog>());
		}

		[SetUp]
		public async Task SetUp()
		{
			// Clean up all existing batches
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				var request = new MassDeleteByCriteriaRequest
				{
					ObjectIdentificationCriteria = new ObjectIdentificationCriteria
					{
						ObjectType = SyncBatchObjectTypeRef
					}
				};
				await objectManager.DeleteAsync(_workspaceId, request).ConfigureAwait(false);
			}
		}

		[Test]
		public async Task ItShouldCreateAllBatches()
		{
			// Arrange
			const int totalRecordsCount = 1000;
			const int batchSize = 100;
			ConfigurationStub configuration = new ConfigurationStub()
			{
				TotalRecordsCount = totalRecordsCount,
				BatchSize = batchSize,
				ExportRunId = Guid.Empty,
				SourceWorkspaceArtifactId = _workspaceId,
				SyncConfigurationArtifactId = _syncConfigurationId
			};

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			List<RelativityObject> batches = await GetBatchesAsync(ServiceFactory, _workspaceId, _syncConfigurationId).ConfigureAwait(false);

			const int expectedNumBatches = 10;
			batches.Count.Should().Be(expectedNumBatches);

			AssertBatchesInOrder(batches);
		}

		[Test]
		public async Task ItShouldCreateRemainingBatches()
		{
			// Arrange
			const int batchSize = 100;
			const int firstStartingIndex = 0;
			const int secondStartingIndex = firstStartingIndex + batchSize;
			const int thirdStartingIndex = secondStartingIndex + batchSize;
			await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, batchSize, firstStartingIndex).ConfigureAwait(false);
			await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, batchSize, secondStartingIndex).ConfigureAwait(false);
			await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, batchSize, thirdStartingIndex).ConfigureAwait(false);

			const int totalRecordsCount = 1000;
			ConfigurationStub configuration = new ConfigurationStub()
			{
				TotalRecordsCount = totalRecordsCount,
				BatchSize = batchSize,
				ExportRunId = Guid.Empty,
				SourceWorkspaceArtifactId = _workspaceId,
				SyncConfigurationArtifactId = _syncConfigurationId
			};

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			List<RelativityObject> batches = await GetBatchesAsync(ServiceFactory, _workspaceId, _syncConfigurationId).ConfigureAwait(false);

			const int expectedNumBatches = 10;
			batches.Count.Should().Be(expectedNumBatches);

			AssertBatchesInOrder(batches);
		}

		[Test]
		public async Task ItShouldCreateRemainingBatchesWithNewBatchSize()
		{
			// Arrange
			const int originalBatchSize = 100;
			const int firstStartingIndex = 0;
			const int secondStartingIndex = firstStartingIndex + originalBatchSize;
			const int thirdStartingIndex = secondStartingIndex + originalBatchSize;
			await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, originalBatchSize, firstStartingIndex).ConfigureAwait(false);
			await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, originalBatchSize, secondStartingIndex).ConfigureAwait(false);
			await _batchRepository.CreateAsync(_workspaceId, _syncConfigurationId, originalBatchSize, thirdStartingIndex).ConfigureAwait(false);

			const int totalRecordsCount = 1000;
			const int newBatchSize = 350;
			ConfigurationStub configuration = new ConfigurationStub()
			{
				TotalRecordsCount = totalRecordsCount,
				BatchSize = newBatchSize,
				ExportRunId = Guid.Empty,
				SourceWorkspaceArtifactId = _workspaceId,
				SyncConfigurationArtifactId = _syncConfigurationId
			};

			// Act
			ExecutionResult result = await _instance.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);

			List<RelativityObject> batches = await GetBatchesAsync(ServiceFactory, _workspaceId, _syncConfigurationId).ConfigureAwait(false);

			const int expectedNumBatches = 5;
			batches.Count.Should().Be(expectedNumBatches);

			AssertBatchesInOrder(batches);
		}

		private static async Task<List<RelativityObject>> GetBatchesAsync(IServiceFactory serviceFactory, int workspaceId, int syncConfigurationId, int length = 100)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				var request = new QueryRequest
				{
					ObjectType = SyncBatchObjectTypeRef,
					Condition = $"'{SyncConfigurationRelationGuid}' == OBJECT {syncConfigurationId}",
					Sorts = new[]
					{
						new Sort
						{
							FieldIdentifier = new FieldRef { Guid = StartingIndexGuid },
							Direction = SortEnum.Ascending
						}
					},
					Fields = new []
					{
						new FieldRef { Guid = StartingIndexGuid },
						new FieldRef { Guid = TotalItemsCountGuid }
					}
				};

				QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, length).ConfigureAwait(false);
				return result.Objects;
			}
		}

		private static void AssertBatchesInOrder(IEnumerable<RelativityObject> batches)
		{
			int expectedStartIndex = 0;
			foreach (RelativityObject batch in batches)
			{
				batch[StartingIndexGuid].Value.Should().Be(expectedStartIndex);
				expectedStartIndex += (int)batch[TotalItemsCountGuid].Value;
			}
		}

	}
}