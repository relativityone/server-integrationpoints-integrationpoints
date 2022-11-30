using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.DocumentTaggers;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Executors.DocumentTaggers
{
    [TestFixture]
    internal class DocumentTaggerTests
    {
        private Mock<IDocumentTagRepository> _documentsTagRepositoryMock;
        private Mock<Sync.Executors.IImportJob> _importJobMock;
        private readonly IEnumerable<int> _pushedDocumentsArtifactIds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        private IEnumerable<string> _pushedDocumentsIdentifiers;

        private DocumentTagger _sut;

        [SetUp]
        public void SetUp()
        {
            _documentsTagRepositoryMock = new Mock<IDocumentTagRepository>();
            _importJobMock = new Mock<Sync.Executors.IImportJob>();
            _pushedDocumentsIdentifiers = _pushedDocumentsArtifactIds.Select(x => x.ToString());

            _importJobMock.Setup(x => x.GetPushedDocumentArtifactIdsAsync()).Returns(Task.FromResult(_pushedDocumentsArtifactIds));
            _importJobMock.Setup(x => x.GetPushedDocumentIdentifiersAsync()).Returns(Task.FromResult(_pushedDocumentsIdentifiers));

            _sut = new DocumentTagger(_documentsTagRepositoryMock.Object, new EmptyLogger());
        }

        [Test]
        public async Task TagObjectsAsync_ShouldTagDocuments()
        {
            // Arrange
            TaggingExecutionResult taggingResult =
                new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsArtifactIds.Count()
                };

            SetTaggingResult(taggingResult, taggingResult);

            // Act
            TaggingExecutionResult result = await _sut.TagObjectsAsync(_importJobMock.Object, new ConfigurationStub(),
                new CompositeCancellationTokenStub());

            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Completed);
            result.TaggedDocumentsCount.Should().Be(_pushedDocumentsArtifactIds.Count());
        }

        [Test]
        public async Task TagObjectsAsync_ShouldEndWithStatusCancelledWhenStopped()
        {
            // Arrange
            TaggingExecutionResult taggingResult =
                new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsArtifactIds.Count()
                };
            SetTaggingResult(taggingResult, taggingResult);

            CompositeCancellationTokenStub token = new CompositeCancellationTokenStub
            {
                IsStopRequestedFunc = () => true
            };

            // Act
            TaggingExecutionResult result = await _sut.TagObjectsAsync(_importJobMock.Object, new ConfigurationStub(), token);

            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Canceled);
            result.TaggedDocumentsCount.Should().Be(0);
        }

        [Test]
        public async Task TagObjectsAsync_ShouldEndWithStatusFailedWhenTaggingSourceDocumentsFails()
        {
            // Arrange
            TaggingExecutionResult sourceTaggingResult =
                new TaggingExecutionResult(ExecutionStatus.Failed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsArtifactIds.Count()
                };

            TaggingExecutionResult destinationTaggingResult =
                new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsIdentifiers.Count()
                };
            SetTaggingResult(sourceTaggingResult, destinationTaggingResult);

            // Act
            TaggingExecutionResult result = await _sut.TagObjectsAsync(_importJobMock.Object, new ConfigurationStub(), new CompositeCancellationTokenStub());

            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Failed);
            result.TaggedDocumentsCount.Should().Be(_pushedDocumentsArtifactIds.Count());
            result.Exception.Should().BeOfType<AggregateException>();
        }

        [Test]
        public async Task TagObjectsAsync_ShouldEndWithStatusFailedWhenTaggingDestinationDocumentsFails()
        {
            // Arrange
            TaggingExecutionResult sourceTaggingResult =
                new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsArtifactIds.Count()
                };

            TaggingExecutionResult destinationTaggingResult =
                new TaggingExecutionResult(ExecutionStatus.Failed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsIdentifiers.Count()
                };
            SetTaggingResult(sourceTaggingResult, destinationTaggingResult);

            // Act
            TaggingExecutionResult result = await _sut.TagObjectsAsync(_importJobMock.Object, new ConfigurationStub(), new CompositeCancellationTokenStub());

            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Failed);
            result.TaggedDocumentsCount.Should().Be(_pushedDocumentsArtifactIds.Count());
            result.Exception.Should().BeOfType<AggregateException>();
        }

        [Test]
        public async Task TagObjectsAsync_ShouldSetTagDocumentsCountToMinValue()
        {
            // Arrange
            int taggedDestinationDocuments = 5;
            TaggingExecutionResult sourceTaggingResult =
                new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = _pushedDocumentsArtifactIds.Count()
                };

            TaggingExecutionResult destinationTaggingResult =
                new TaggingExecutionResult(ExecutionStatus.Completed, string.Empty, new Exception())
                {
                    TaggedDocumentsCount = taggedDestinationDocuments
                };
            SetTaggingResult(sourceTaggingResult, destinationTaggingResult);

            // Act
            TaggingExecutionResult result = await _sut.TagObjectsAsync(_importJobMock.Object, new ConfigurationStub(), new CompositeCancellationTokenStub());

            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Completed);
            result.TaggedDocumentsCount.Should().Be(taggedDestinationDocuments);
        }

        private void SetTaggingResult(TaggingExecutionResult sourceTaggingResult, TaggingExecutionResult destinationTaggingResult)
        {
            _documentsTagRepositoryMock.Setup(x => x.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
                    It.IsAny<ISynchronizationConfiguration>(),
                    _pushedDocumentsArtifactIds, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(sourceTaggingResult));
            _documentsTagRepositoryMock.Setup(x => x.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
                    It.IsAny<ISynchronizationConfiguration>(),
                    _pushedDocumentsIdentifiers, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(destinationTaggingResult));
        }
    }
}
