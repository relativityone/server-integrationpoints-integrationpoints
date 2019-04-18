using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class BatchRdoTests : SystemTest
	{
		private BatchRepository _repository;

		private int _syncConfigurationArtifactId;

		private int _workspaceId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_repository = new BatchRepository(new SourceServiceFactoryForAdminStub(ServiceFactory));

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, _workspaceId).ConfigureAwait(false);
			_syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, _workspaceId, jobHistoryArtifactId).ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldCreateAndReadBatch()
		{
			const int startingIndex = 55;
			const int totalRecords = 100;

			// ACT
			IBatch createdBatch = await _repository.CreateAsync(_workspaceId, _syncConfigurationArtifactId, totalRecords, startingIndex).ConfigureAwait(false);
			IBatch readBatch = await _repository.GetAsync(_workspaceId, createdBatch.ArtifactId).ConfigureAwait(false);

			// ASSERT
			readBatch.StartingIndex.Should().Be(startingIndex);
			readBatch.TotalItemsCount.Should().Be(totalRecords);
		}

		[Test]
		public async Task ItShouldUpdateValues()
		{
			const int startingIndex = 5;
			const int totalRecords = 10;

			const string status = "status 1";
			const int failedItemsCount = 8;
			const string lockedBy = "locked by";
			const double progress = 2.1;
			const int transferredItemsCount = 45;

			IBatch createdBatch = await _repository.CreateAsync(_workspaceId, _syncConfigurationArtifactId, totalRecords, startingIndex).ConfigureAwait(false);

			// ACT
			await createdBatch.SetStatusAsync(status).ConfigureAwait(false);
			await createdBatch.SetFailedItemsCountAsync(failedItemsCount).ConfigureAwait(false);
			await createdBatch.SetLockedByAsync(lockedBy).ConfigureAwait(false);
			await createdBatch.SetProgressAsync(progress).ConfigureAwait(false);
			await createdBatch.SetTransferredItemsCountAsync(transferredItemsCount).ConfigureAwait(false);

			// ASSERT
			IBatch readBatch = await _repository.GetAsync(_workspaceId, createdBatch.ArtifactId).ConfigureAwait(false);

			readBatch.Status.Should().Be(status);
			readBatch.FailedItemsCount.Should().Be(failedItemsCount);
			readBatch.LockedBy.Should().Be(lockedBy);
			readBatch.Progress.Should().Be(progress);
			readBatch.TransferredItemsCount.Should().Be(transferredItemsCount);
		}
	}
}