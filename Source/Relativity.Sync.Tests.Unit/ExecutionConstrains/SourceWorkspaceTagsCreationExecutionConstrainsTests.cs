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
    public sealed class SourceWorkspaceTagsCreationExecutionConstrainsTests
    {
        private SourceWorkspaceTagsCreationExecutionConstrains _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SourceWorkspaceTagsCreationExecutionConstrains();
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ItShouldAlwaysCanExecute(bool enableTagging)
        {
            // Arrange
            Mock<ISourceWorkspaceTagsCreationConfiguration> configuration =
                new Mock<ISourceWorkspaceTagsCreationConfiguration>();
            configuration.SetupGet(x => x.EnableTagging).Returns(enableTagging);

            // Act
            bool canExecute = await _sut.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            canExecute.Should().Be(enableTagging);
        }
    }
}
