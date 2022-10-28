using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    internal class ConfigureDocumentSynchronizationExecutionConstrainsTests
    {
        [Test]
        public async Task CanExecuteAsync_ShouldAlwaysReturnTrue()
        {
            // Arrange
            IExecutionConstrains<IConfigureDocumentSynchronizationConfiguration> sut = new ConfigureDocumentSynchronizationExecutionConstrains();

            // Act
            bool canExecute = await sut.CanExecuteAsync(Mock.Of<IConfigureDocumentSynchronizationConfiguration>(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            canExecute.Should().BeTrue();
        }
    }
}
