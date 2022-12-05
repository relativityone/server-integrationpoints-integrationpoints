using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class DestinationWorkspaceObjectTypesCreationExecutorConstrainsTests
    {
        private DestinationWorkspaceObjectTypesCreationExecutorConstrains _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new DestinationWorkspaceObjectTypesCreationExecutorConstrains();
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ItShouldAlwaysAllowExecution(bool enableTagging)
        {
            // Arrange
            Mock<IDestinationWorkspaceObjectTypesCreationConfiguration> configuration =
                new Mock<IDestinationWorkspaceObjectTypesCreationConfiguration>();
            configuration.SetupGet(x => x.EnableTagging).Returns(enableTagging);

            // Act
            bool canExecute = await _sut.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            canExecute.Should().Be(enableTagging);
        }
    }
}
