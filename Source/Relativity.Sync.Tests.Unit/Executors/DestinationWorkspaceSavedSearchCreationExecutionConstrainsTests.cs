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
    internal sealed class DestinationWorkspaceSavedSearchCreationExecutionConstrainsTests
    {
        private Mock<IDestinationWorkspaceSavedSearchCreationConfiguration> _config;
        private DestinationWorkspaceSavedSearchCreationExecutionConstrains _instance;

        [SetUp]
        public void SetUp()
        {
            _config = new Mock<IDestinationWorkspaceSavedSearchCreationConfiguration>();
            _instance = new DestinationWorkspaceSavedSearchCreationExecutionConstrains();
        }

        [Test]
        public async Task ItShouldNotExecuteWhenSavedSearchCreationDisabled()
        {
            _config.SetupGet(x => x.CreateSavedSearchForTags).Returns(false);

            // act
            bool canExecute = await _instance.CanExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

            // assert
            canExecute.Should().Be(false);
        }

        [Test]
        public async Task ItShouldNotExecuteWhenSavedSearchArtifactIdIsSet()
        {
            _config.SetupGet(x => x.IsSavedSearchArtifactIdSet).Returns(true);

            // act
            bool canExecute = await _instance.CanExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

            // assert
            canExecute.Should().Be(false);
        }

        [Test]
        public async Task ItShouldExecute()
        {
            _config.SetupGet(x => x.CreateSavedSearchForTags).Returns(true);
            _config.SetupGet(x => x.IsSavedSearchArtifactIdSet).Returns(false);

            // act
            bool canExecute = await _instance.CanExecuteAsync(_config.Object, CancellationToken.None).ConfigureAwait(false);

            // assert
            canExecute.Should().Be(true);
        }
    }
}
