using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    internal class DocumentSynchronizationMonitorExecutionConstrainsTests
    {
        [Test]
        public async Task CanExecuteAsync_ShouldAlwaysReturnTrue()
        {
            // Arrange
            IExecutionConstrains<DocumentSynchronizationMonitorConfiguration> sut = new DocumentSynchronizationMonitorExecutionConstrains();

            // Act
            bool canExecute = await sut.CanExecuteAsync(Mock.Of<DocumentSynchronizationMonitorConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            canExecute.Should().BeTrue();
        }
    }
}
