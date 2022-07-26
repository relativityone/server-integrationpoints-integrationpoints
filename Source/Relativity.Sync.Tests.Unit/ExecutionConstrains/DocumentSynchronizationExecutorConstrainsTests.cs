using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class DocumentSynchronizationExecutorConstrainsTests
    {
        private CancellationToken _token;
        private IAPILog _syncLog;

        private Mock<IDocumentSynchronizationConfiguration> _synchronizationConfiguration;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _token = CancellationToken.None;
            _syncLog = new EmptyLogger();

            _synchronizationConfiguration = new Mock<IDocumentSynchronizationConfiguration>();
        }

        [Test]
        [TestCase(new []{1}, ExpectedResult = true)]
        [TestCase(new int[0], ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public async Task<bool> CanExecuteAsyncGoldFlowTests(IEnumerable<int> batchIds)
        {
            // Arrange
            var batchRepository = new Mock<IBatchRepository>();
            batchRepository.Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).ReturnsAsync(batchIds);

            var synchronizationExecutorConstrains = new DocumentSynchronizationExecutionConstrains(batchRepository.Object, _syncLog);

            // Act
            bool actualResult = await synchronizationExecutorConstrains.CanExecuteAsync(_synchronizationConfiguration.Object, _token).ConfigureAwait(false);

            // Assert
            return actualResult;
        }

        [Test]
        public void CanExecuteAsyncThrowsWhenGettingBatchIdsTest()
        {
            // Arrange
            var batchRepository = new Mock<IBatchRepository>();
            batchRepository.Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>())).Throws<OutOfMemoryException>();

            var synchronizationExecutorConstrains = new DocumentSynchronizationExecutionConstrains(batchRepository.Object, _syncLog);

            // Act & Assert
            Assert.ThrowsAsync<OutOfMemoryException>(async () => await synchronizationExecutorConstrains.CanExecuteAsync(_synchronizationConfiguration.Object, _token).ConfigureAwait(false));
        }
    }
}
