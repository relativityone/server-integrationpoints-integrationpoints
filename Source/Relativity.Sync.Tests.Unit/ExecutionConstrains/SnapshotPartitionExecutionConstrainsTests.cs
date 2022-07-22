using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    internal sealed class SnapshotPartitionExecutionConstrainsTests
    {
        private SnapshotPartitionExecutionConstrains _instance;

        private Mock<IBatchRepository> _batchRepository;
        private ISnapshotPartitionConfiguration _configuration;

        private const int _WORKSPACE_ID = 986574;
        private const int _SYNC_CONF_ID = 365298;

        private const int _TOTAL_NUMBER_OF_RECORDS = 100;

        [SetUp]
        public void SetUp()
        {
            _batchRepository = new Mock<IBatchRepository>();

            Mock<ISnapshotPartitionConfiguration> configurationMock = new Mock<ISnapshotPartitionConfiguration>();
            configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
            configurationMock.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);
            configurationMock.Setup(x => x.TotalRecordsCount).Returns(_TOTAL_NUMBER_OF_RECORDS);

            _configuration = configurationMock.Object;

            _instance = new SnapshotPartitionExecutionConstrains(_batchRepository.Object, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldExecuteWhenBatchesAreMissing()
        {
            _batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync((IBatch) null);

            // ACT
            bool shouldExecute = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            shouldExecute.Should().BeTrue();
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(50, 49)]
        [TestCase(98, 1)]
        [TestCase(0, 99)]
        public async Task ItShouldExecuteWhenRecordsAreNotIncludedInBatch(int startingIndex, int batchSize)
        {
            Mock<IBatch> batch = new Mock<IBatch>();
            batch.Setup(x => x.StartingIndex).Returns(startingIndex);
            batch.Setup(x => x.TotalDocumentsCount).Returns(batchSize);

            _batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync(batch.Object);

            // ACT
            bool shouldExecute = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            shouldExecute.Should().BeTrue();
        }

        [Test]
        [TestCase(0, 100)]
        [TestCase(50, 50)]
        [TestCase(99, 1)]
        [TestCase(100, 1)]
        public async Task ItShouldNotExecuteWhenRecordsAreIncludedInBatch(int startingIndex, int batchSize)
        {
            Mock<IBatch> batch = new Mock<IBatch>();
            batch.Setup(x => x.StartingIndex).Returns(startingIndex);
            batch.Setup(x => x.TotalDocumentsCount).Returns(batchSize);

            _batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).ReturnsAsync(batch.Object);

            // ACT
            bool shouldExecute = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            shouldExecute.Should().BeFalse();
        }

        [Test]
        public void ItShouldNotHideException()
        {
            _batchRepository.Setup(x => x.GetLastAsync(_WORKSPACE_ID, _SYNC_CONF_ID, It.IsAny<Guid>())).Throws<InvalidOperationException>();

            // ACT
            Func<Task> action = async () => await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            action.Should().Throw<InvalidOperationException>();
        }
    }
}