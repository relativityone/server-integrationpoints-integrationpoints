using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public class NonDocumentSynchronizationExecutorConstrainsTests
    {

        [TestCase(new int[0], false)]//Empty array
        [TestCase(null,false)]
        [TestCase(new[] { 1 }, true)]//one element
        public async Task CanExecuteGoldFlowTests(IEnumerable<int> batchIds, bool expectedResult)
        {
            //Arrange
            var fakeBatchRepository = new Mock<IBatchRepository>();
            var fakeSyncLog = new Mock<ISyncLog>();
            var fakeConfiguration = new Mock<INonDocumentSynchronizationConfiguration>();

            fakeBatchRepository
                .Setup(x => x.GetAllBatchesIdsToExecuteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(batchIds);
            
            var sut = new NonDocumentSynchronizationExecutionConstrains(fakeBatchRepository.Object, fakeSyncLog.Object);

            //Act
            bool actualResult = await sut.CanExecuteAsync( fakeConfiguration.Object,It.IsAny<CancellationToken>())
                .ConfigureAwait(false);
            
            //Assert
            actualResult.Should().Be(expectedResult);
        }
    }
}
