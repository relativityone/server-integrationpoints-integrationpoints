﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal class JobStatusConsolidationExecutorTests
	{
		private Mock<IObjectManager> _objectManager;
		private Mock<IBatchRepository> _batchRepository;
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactory;
		private Mock<IJobStatusConsolidationConfiguration> _configuration;
		private List<IBatch> _batches;

		private IExecutor<IJobStatusConsolidationConfiguration> _sut;

		private static readonly InvalidOperationException Exception = new InvalidOperationException();
		private static readonly Guid CompletedItemsCountGuid = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");
		private static readonly Guid FailedItemsCountGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");
		private static readonly Guid TotalItemsCountGuid = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");

		public static IEnumerable<Action<JobStatusConsolidationExecutorTests>> ServiceFailures { get; } = new Action<JobStatusConsolidationExecutorTests>[]
		{
			ctx => ctx._serviceFactory
				.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ThrowsAsync(Exception),
			ctx => ctx._objectManager
				.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
				.ThrowsAsync(Exception),
			ctx => ctx._batchRepository
				.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ThrowsAsync(Exception)
		};

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();
			_batchRepository = new Mock<IBatchRepository>();
			_configuration = new Mock<IJobStatusConsolidationConfiguration>();

			_serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			_serviceFactory
				.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);

			_batches = new List<IBatch>();

			_batchRepository
				.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(_batches);

			SetUpUpdateCall(true);

			_sut = new JobStatusConsolidationExecutor(_batchRepository.Object, _serviceFactory.Object);
		}

		[Test]
		[TestCaseSource(nameof(ServiceFailures))]
		public async Task ItShouldReportFailureWhenAnyServiceThrow(Action<JobStatusConsolidationExecutorTests> setUpFailure)
		{
			// Arrange
			const int totalBatchCount = 5;
			const int totalTransferredCount = 1000;
			const int totalFailedCount = 10;
			SetUpBatches(totalBatchCount, totalTransferredCount, totalFailedCount);

			setUpFailure(this);

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configuration.Object, CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Failed);
			result.Exception.Should().Be(Exception);
		}

		[Test]
		public async Task ItShouldReportFailureWhenFailedToUpdateJobHistory()
		{
			// Arrange
			const int totalBatchCount = 5;
			const int totalTransferredCount = 1000;
			const int totalFailedCount = 10;
			SetUpBatches(totalBatchCount, totalTransferredCount, totalFailedCount);

			SetUpUpdateCall(false);

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configuration.Object, CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Failed);
			result.Exception.Should().Be(null);
		}

		[Test]
		public async Task ItShouldAggregateStatisticsFromBatches(
			[Values(1, 2, 5, 8)] int batchCount,
			[Values(0, 2, 10, 1000)] int transferredCount,
			[Values(0, 2, 10, 1000)] int failedCount)
		{
			// Arrange
			int totalItemCount = transferredCount + failedCount;
			SetUpBatches(batchCount, transferredCount, failedCount);

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configuration.Object, CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			VerifyUpdateCall(transferredCount, failedCount, totalItemCount);
		}

		[Test]
		public async Task ItShouldUpdateWithZerosWhenThereAreNoBatches()
		{
			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configuration.Object, CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			VerifyUpdateCall(0, 0, 0);
		}

		[Test]
		public async Task ItShouldReportFailureWhenUpdateResultIsNull()
		{
			// Arrange
			const int totalBatchCount = 5;
			const int totalTransferredCount = 1000;
			const int totalFailedCount = 10;
			SetUpBatches(totalBatchCount, totalTransferredCount, totalFailedCount);

			_objectManager
				.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
				.ReturnsAsync((UpdateResult) null);

			// Act
			ExecutionResult result = await _sut
				.ExecuteAsync(_configuration.Object, CancellationToken.None)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(ExecutionStatus.Failed);
			result.Exception.Should().Be(null);
		}

		private void SetUpUpdateCall(bool success)
		{
			_objectManager
				.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
				.ReturnsAsync(() => CreateUpdateResultStatuses(success));
		}

		private static UpdateResult CreateUpdateResultStatuses(bool success)
		{
			var updateResult = new UpdateResult
			{
				EventHandlerStatuses = new List<EventHandlerStatus>
				{
					new EventHandlerStatus { Success = success }
				}
			};
			return updateResult;
		}

		private void SetUpBatches(int batchesTotalCount, int totalTransferred, int totalFailed)
		{
			if (batchesTotalCount > 0)
			{
				_batches.AddRange(CreateBatches(batchesTotalCount, totalTransferred, totalFailed));
			}
		}

		private static IEnumerable<IBatch> CreateBatches(int count, int transferred, int failed)
		{
			int total = transferred + failed;
			int batchSize = total / count;

			IEnumerable<IBatch> batches = Enumerable
				.Range(0, total)
				.Select(i => i < transferred)
				.ToList()
				.Shuffle()
				.SplitList(batchSize)
				.Select(isTransferredList =>
					CreateBatch(isTransferredList.Count(t => t), isTransferredList.Count(t => !t)));

			return batches;
		}

		private static IBatch CreateBatch(int transferred, int failed)
		{
			var batch = new Mock<IBatch>();

			batch
				.SetupGet(b => b.TransferredItemsCount)
				.Returns(transferred);

			batch
				.SetupGet(b => b.FailedItemsCount)
				.Returns(failed);

			batch
				.SetupGet(b => b.TotalItemsCount)
				.Returns(transferred + failed);

			return batch.Object;
		}

		private void VerifyUpdateCall(int transferredCount, int failedCount, int totalItemCount)
		{
			_objectManager
				.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.Is<UpdateRequest>(r =>
					(int)r.FieldValues.Single(fvp => fvp.Field.Guid.Equals(CompletedItemsCountGuid)).Value == transferredCount &&
					(int)r.FieldValues.Single(fvp => fvp.Field.Guid.Equals(FailedItemsCountGuid)).Value == failedCount &&
					(int)r.FieldValues.Single(fvp => fvp.Field.Guid.Equals(TotalItemsCountGuid)).Value == totalItemCount)),
					Times.Once);
		}
	}
}