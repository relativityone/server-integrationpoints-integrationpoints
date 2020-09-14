﻿using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
	[TestFixture]
	public sealed class ImageRetryDataSourceSnapshotExecutionConstrainsTests
	{
		private ImageRetryDataSourceSnapshotExecutionConstrains _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new ImageRetryDataSourceSnapshotExecutionConstrains();
		}

		[TestCase(true, false)]
		[TestCase(false, true)]
		public async Task CanExecuteAsync_ShouldReturnTrue_When_IsSnapshotCreated(bool snapshotExists, bool expectedCanExecute)
		{
			Mock<IImageRetryDataSourceSnapshotConfiguration> configuration = new Mock<IImageRetryDataSourceSnapshotConfiguration>();

			configuration.Setup(x => x.IsSnapshotCreated).Returns(snapshotExists);
			configuration.Setup(x => x.JobHistoryToRetryId).Returns(1);

			// ACT
			bool canExecute = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().Be(expectedCanExecute);
		}

		[TestCase(null, false)]
		[TestCase(1, true)]
		public async Task CanExecuteAsync_ShouldReturnTrue_OnlyWhen_JobHistoryToRetry_IsNotNull(int? jobHisotryToRetry, bool expectedCanExecute)
		{
			Mock<IImageRetryDataSourceSnapshotConfiguration> configuration = new Mock<IImageRetryDataSourceSnapshotConfiguration>();

			configuration.Setup(x => x.IsSnapshotCreated).Returns(false);
			configuration.Setup(x => x.JobHistoryToRetryId).Returns(jobHisotryToRetry);

			// ACT
			bool canExecute = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().Be(expectedCanExecute);
		}
	}
}