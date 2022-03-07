using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
// ReSharper disable NUnit.IncorrectExpectedResultType

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public class ObjectLinkingSnapshotPartitionExecutionConstrainsTests
    {
        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public Task<bool> CanExecute_ShouldReturnCorrectValue(bool linkingExportExists)
        {
            // Arrange
            var configMock = new Mock<IObjectLinkingSnapshotPartitionConfiguration>();
            configMock.SetupGet(x => x.LinkingExportExists)
                .Returns(linkingExportExists);

            var sut = new ObjectLinkingSnapshotPartitionExecutionConstrains();

            return sut.CanExecuteAsync(configMock.Object, CancellationToken.None);
        }
    }
}