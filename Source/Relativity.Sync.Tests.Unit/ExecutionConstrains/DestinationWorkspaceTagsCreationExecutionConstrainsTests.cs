using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public sealed class DestinationWorkspaceTagsCreationExecutionConstrainsTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public async Task CanExecuteAsync_ShouldReturnBasedOnEnableTaggingConfiguration(bool enableTagging)
        {
            // Arrange
            Mock<IDestinationWorkspaceTagsCreationConfiguration> configuration =
                new Mock<IDestinationWorkspaceTagsCreationConfiguration>();
            configuration.SetupGet(x => x.EnableTagging).Returns(enableTagging);

            DestinationWorkspaceTagsCreationExecutionConstrains sut = new DestinationWorkspaceTagsCreationExecutionConstrains();

            // Act
            bool result = await sut.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().Be(enableTagging);
        }
    }
}
