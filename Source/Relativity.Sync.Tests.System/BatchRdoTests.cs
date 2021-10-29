using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Utils;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class BatchRdoTests : SystemTest
	{
		private BatchRepository _sut;
		private int _syncConfigurationArtifactId;
		private int _workspaceId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_sut = new BatchRepository(new TestRdoManager(Logger),new ServiceFactoryStub(ServiceFactory), new DateTimeWrapper());

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspaceId).ConfigureAwait(false);
			_syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationRdoAsync(_workspaceId, jobHistoryArtifactId).ConfigureAwait(false);
		}

		[IdentifiedTest("d15a4f9e-a56c-4991-bb0f-017cd0e34ecd")]
		public async Task CreateAsync_ShouldCreateNewBatch()
		{
			const int startingIndex = 55;
			const int totalRecords = 100;

			// ACT
			IBatch createdBatch = await _sut.CreateAsync(_workspaceId, _syncConfigurationArtifactId, totalRecords, startingIndex).ConfigureAwait(false);
			IBatch readBatch = await _sut.GetAsync(_workspaceId, createdBatch.ArtifactId).ConfigureAwait(false);

			// ASSERT
			readBatch.StartingIndex.Should().Be(startingIndex);
			readBatch.TotalDocumentsCount.Should().Be(totalRecords);
		}

		[IdentifiedTest("5a0341fc-43ee-4d66-a0fe-f8a7bfd220c2")]
		public async Task SetMethods_ShouldUpdateBatch()
		{
			const BatchStatus status = BatchStatus.InProgress;

			const int startingIndex = 5;
			const int totalRecords = 10;

			const int failedDocumentsCount = 2;
			const int transferredDocumentsCount = 3;

			const int failedItemsCount = 8;
			const int transferredItemsCount = 45;
			const int metadataBytesTransferred = 1024;
			const int filesBytesTransferred = 5120;
			const int totalBytesTransferred = 6144;

			IBatch createdBatch = await _sut.CreateAsync(_workspaceId, _syncConfigurationArtifactId, totalRecords, startingIndex).ConfigureAwait(false);

			// ACT
			await createdBatch.SetStatusAsync(status).ConfigureAwait(false);
			await createdBatch.SetFailedItemsCountAsync(failedItemsCount).ConfigureAwait(false);
			await createdBatch.SetTransferredItemsCountAsync(transferredItemsCount).ConfigureAwait(false);
			await createdBatch.SetFailedDocumentsCountAsync(failedDocumentsCount).ConfigureAwait(false);
			await createdBatch.SetTransferredDocumentsCountAsync(transferredDocumentsCount).ConfigureAwait(false);
			await createdBatch.SetMetadataBytesTransferredAsync(metadataBytesTransferred).ConfigureAwait(false);
			await createdBatch.SetFilesBytesTransferredAsync(filesBytesTransferred).ConfigureAwait(false);
			await createdBatch.SetTotalBytesTransferredAsync(totalBytesTransferred).ConfigureAwait(false);

			// ASSERT
			IBatch readBatch = await _sut.GetAsync(_workspaceId, createdBatch.ArtifactId).ConfigureAwait(false);

			readBatch.Status.Should().Be(status);
			readBatch.FailedItemsCount.Should().Be(failedItemsCount);
			readBatch.TransferredItemsCount.Should().Be(transferredItemsCount);
			readBatch.FailedDocumentsCount.Should().Be(failedDocumentsCount);
			readBatch.TransferredDocumentsCount.Should().Be(transferredDocumentsCount);
			readBatch.MetadataBytesTransferred.Should().Be(metadataBytesTransferred);
			readBatch.FilesBytesTransferred.Should().Be(filesBytesTransferred);
			readBatch.TotalBytesTransferred.Should().Be(totalBytesTransferred);
		}

		[IdentifiedTest("7e5348d7-dee0-4f20-9da7-888a62f7ee1a")]
		public async Task DeleteAllForConfiguration_ShouldDeleteBatchesThatBelongToConfiguration()
		{
			const int startingIndex = 5;
			const int totalRecords = 10;

			IBatch batch1 = await _sut.CreateAsync(_workspaceId, _syncConfigurationArtifactId, totalRecords, startingIndex).ConfigureAwait(false);
			IBatch batch2 = await _sut.CreateAsync(_workspaceId, _syncConfigurationArtifactId, totalRecords, startingIndex).ConfigureAwait(false);

			// ACT
			await _sut.DeleteAllForConfigurationAsync(_workspaceId, _syncConfigurationArtifactId).ConfigureAwait(false);

			// ASSERT
			IEnumerable<IBatch> batches = await _sut.GetAllAsync(_workspaceId, _syncConfigurationArtifactId).ConfigureAwait(false);
			batches.Should().BeEmpty();
		}
	}
}