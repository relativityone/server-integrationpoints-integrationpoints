using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class DestinationWorkspaceSavedSearchCreationExecutorTests
    {
        private Mock<ITagSavedSearch> _tagSavedSearch;
        private Mock<ITagSavedSearchFolder> _tagSavedSearchFolder;
        private Mock<IDestinationWorkspaceSavedSearchCreationConfiguration> _config;
        private DestinationWorkspaceSavedSearchCreationExecutor _instance;

        [SetUp]
        public void SetUp()
        {
            _tagSavedSearch = new Mock<ITagSavedSearch>();
            _tagSavedSearchFolder = new Mock<ITagSavedSearchFolder>();
            _config = new Mock<IDestinationWorkspaceSavedSearchCreationConfiguration>();
            _instance = new DestinationWorkspaceSavedSearchCreationExecutor(_tagSavedSearch.Object, _tagSavedSearchFolder.Object, new EmptyLogger());
        }

        [Test]
        public async Task ItShouldSetSavedSearchArtifactIdInConfiguration()
        {
            const int folderId = 1;
            const int savedSearchId = 2;
            _tagSavedSearchFolder.Setup(x => x.GetFolderIdAsync(It.IsAny<int>())).ReturnsAsync(folderId);
            _tagSavedSearch.Setup(x => x.CreateTagSavedSearchAsync(It.IsAny<IDestinationWorkspaceSavedSearchCreationConfiguration>(), It.IsAny<int>(), CancellationToken.None))
                .ReturnsAsync(savedSearchId);

            // act
            ExecutionResult executionResult = await _instance.ExecuteAsync(_config.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            _config.Verify(x => x.SetSavedSearchInDestinationArtifactIdAsync(savedSearchId));
            Assert.AreEqual(ExecutionStatus.Completed, executionResult.Status);
        }

        [Test]
        public async Task ItShouldReturnFailedExecutionStatusWhenTagSavedSearchFolderFails()
        {
            _tagSavedSearchFolder.Setup(x => x.GetFolderIdAsync(It.IsAny<int>())).Throws<InvalidOperationException>();

            // act
            ExecutionResult executionResult = await _instance.ExecuteAsync(_config.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            executionResult.Status.Should().Be(ExecutionStatus.Failed);
            executionResult.Exception.Should().BeOfType<InvalidOperationException>();
        }

        [Test]
        public async Task ItShouldReturnFailedExecutionStatusWhenTagSavedSearchFails()
        {
            _tagSavedSearch.Setup(x => x.CreateTagSavedSearchAsync(It.IsAny<IDestinationWorkspaceSavedSearchCreationConfiguration>(), It.IsAny<int>(), CancellationToken.None)).Throws<InvalidOperationException>();

            // act
            ExecutionResult executionResult = await _instance.ExecuteAsync(_config.Object, CompositeCancellationToken.None).ConfigureAwait(false);

            // assert
            executionResult.Status.Should().Be(ExecutionStatus.Failed);
            executionResult.Exception.Should().BeOfType<InvalidOperationException>();
        }
    }
}