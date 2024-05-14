using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Tagging;
using Relativity.Sync.Kepler.Document;
using Relativity.Sync.Kepler.SyncBatch;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors.Tagging
{
    internal class SourceWorkspaceTaggingServiceTests
    {
        private const int _MAX_OBJECT_QUERY_BATCH_SIZE = 10000;

        private Mock<IRelativityExportBatcher> _exportBatchFake;
        private Mock<ITaggingRepository> _tagRepositoryFake;
        private Mock<IIdentifierFieldMapService> _identifierFieldMapServiceFake;
        private Mock<IDocumentRepository> _documentRepositoryFake;

        private ConfigurationStub _configurationFake;

        private IFixture _fxt;

        private SourceWorkspaceTaggingService _sut;

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _exportBatchFake = new Mock<IRelativityExportBatcher>();

            Mock<IRelativityExportBatcherFactory> exportBatcherFactory = new Mock<IRelativityExportBatcherFactory>();
            exportBatcherFactory.Setup(x => x.CreateRelativityExportBatchForTagging(It.IsAny<SyncBatchDto>()))
                .Returns(_exportBatchFake.Object);

            _tagRepositoryFake = new Mock<ITaggingRepository>();

            _identifierFieldMapServiceFake = new Mock<IIdentifierFieldMapService>();
            _identifierFieldMapServiceFake.Setup(x => x.GetObjectIdentifierField()).Returns(new FieldMap { FieldIndex = 0 });

            _documentRepositoryFake = new Mock<IDocumentRepository>();

            _configurationFake = new ConfigurationStub();
            _configurationFake.TaggingOption = TaggingOption.Enabled;

            Mock<IInstanceSettingsDocument> instanceSettingsMock = new Mock<IInstanceSettingsDocument>();
            instanceSettingsMock
                .Setup(x => x.GetSyncDocumentTaggingBatchSizeAsync(It.IsAny<int>()))
                .ReturnsAsync(10000);

            Mock<ISyncMetrics> syncMetricsMock = new Mock<ISyncMetrics>();
            IAPILog log = new EmptyLogger();

            _sut = new SourceWorkspaceTaggingService(
                exportBatcherFactory.Object,
                new StopwatchWrapper(),
                log,
                _documentRepositoryFake.Object,
                syncMetricsMock.Object,
                instanceSettingsMock.Object,
                _tagRepositoryFake.Object,
                _identifierFieldMapServiceFake.Object);
        }

        [Test]
        public async Task TagDocumentsInSourceWorkspaceAsync_ShouldSuccessfullyTagDocuments_WhenNoItemLevelErrors()
        {
            // Arrange
            List<int> documentIds = _fxt.CreateMany<int>((new Random().Next(0, 1) * _MAX_OBJECT_QUERY_BATCH_SIZE) + new Random().Next(0, _MAX_OBJECT_QUERY_BATCH_SIZE - 1)).ToList();

            SetupExportBatcher(documentIds);

            SyncBatchDto batch = _fxt.Create<SyncBatchDto>();

            _documentRepositoryFake
                .Setup(x => x.GetErroredDocumentsByBatchAsync(batch, It.IsAny<Identity>()))
                .ReturnsAsync(new List<int>());

            _tagRepositoryFake.Setup(x => x.TagDocumentsAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new MassUpdateResult
                {
                    Success = true
                });

            // Act
            TaggingExecutionResult actual = await _sut.TagDocumentsInSourceWorkspaceAsync(_configurationFake, batch).ConfigureAwait(false);

            // Assert
            actual.Should().BeEquivalentTo(TaggingExecutionResult.Success());

            _tagRepositoryFake.Verify(
                x => x.TagDocumentsAsync(
                    It.IsAny<int>(),
                    It.IsAny<List<int>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Exactly((documentIds.Count / _MAX_OBJECT_QUERY_BATCH_SIZE) + 1));
        }

        [Test]
        public async Task TagDocumentsInSourceWorkspaceAsync_ShouldOnlyTagDocumentsWhichAreNotErrored()
        {
            // Arrange
            List<int> documentIds = _fxt.CreateMany<int>((new Random().Next(0, 1) * _MAX_OBJECT_QUERY_BATCH_SIZE) + new Random().Next(0, _MAX_OBJECT_QUERY_BATCH_SIZE - 1)).ToList();
            List<int> erroredDocumentIds = _fxt.CreateMany<int>().ToList();

            List<int> allDocuments = documentIds.ToList();
            allDocuments.AddRange(erroredDocumentIds);

            SetupExportBatcher(allDocuments);

            _documentRepositoryFake.Setup(x => x.GetErroredDocumentsByBatchAsync(It.IsAny<SyncBatchDto>(), It.IsAny<Identity>()))
                .ReturnsAsync(erroredDocumentIds);

            SyncBatchDto batch = _fxt.Create<SyncBatchDto>();

            _tagRepositoryFake.Setup(x => x.TagDocumentsAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new MassUpdateResult
                {
                    Success = true
                });

            // Act
            TaggingExecutionResult actual = await _sut.TagDocumentsInSourceWorkspaceAsync(_configurationFake, batch).ConfigureAwait(false);

            // Assert
            actual.Should().BeEquivalentTo(TaggingExecutionResult.Success());

            _tagRepositoryFake.Verify(
                x => x.TagDocumentsAsync(
                    It.IsAny<int>(),
                    It.Is<List<int>>(z => !z.Any(y => erroredDocumentIds.Contains(y))),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Exactly((documentIds.Count / _MAX_OBJECT_QUERY_BATCH_SIZE) + 1));
        }

        [Test]
        public async Task TagDocumentsInSourceWorkspaceAsync_ShouldNotThrow_WhenExceptionOccurs()
        {
            // Arrange
            List<int> documentIds = _fxt.CreateMany<int>((new Random().Next(0, 1) * _MAX_OBJECT_QUERY_BATCH_SIZE) + new Random().Next(0, _MAX_OBJECT_QUERY_BATCH_SIZE - 1)).ToList();

            SetupExportBatcher(documentIds);

            SyncBatchDto batch = _fxt.Create<SyncBatchDto>();

            _documentRepositoryFake.Setup(x => x.GetErroredDocumentsByBatchAsync(batch, It.IsAny<Identity>()))
                .ReturnsAsync(new List<int>());

            _tagRepositoryFake.Setup(x => x.TagDocumentsAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new NotSupportedException());

            // Act
            TaggingExecutionResult actual = await _sut.TagDocumentsInSourceWorkspaceAsync(_configurationFake, batch).ConfigureAwait(false);

            // Assert
            actual.Status.Should().Be(ExecutionStatus.CompletedWithErrors);
            actual.Exception.Should().BeNull();
            actual.TaggedDocumentsCount.Should().Be(0);
        }

        [Test]
        public async Task TagDocumentsInSourceWorkspaceAsync_ShouldCreateItemLevelErrors_WhenTaggingHasFailed()
        {
            // Arrange
            List<int> documentIds = _fxt.CreateMany<int>((new Random().Next(0, 1) * _MAX_OBJECT_QUERY_BATCH_SIZE) + new Random().Next(1, _MAX_OBJECT_QUERY_BATCH_SIZE - 1)).ToList();

            SetupExportBatcher(documentIds);

            SyncBatchDto batch = _fxt.Create<SyncBatchDto>();

            _documentRepositoryFake.Setup(x => x.GetErroredDocumentsByBatchAsync(batch, It.IsAny<Identity>()))
                .ReturnsAsync(new List<int>());

            List<List<int>> chunks = documentIds.Chunk(_MAX_OBJECT_QUERY_BATCH_SIZE).ToList();

            foreach (List<int> chunk in chunks)
            {
                _tagRepositoryFake.Setup(x => x.TagDocumentsAsync(It.IsAny<int>(), It.Is<List<int>>(y => y.Count == chunk.Count), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(new MassUpdateResult());
            }

            // Act
            TaggingExecutionResult actual = await _sut.TagDocumentsInSourceWorkspaceAsync(_configurationFake, batch).ConfigureAwait(false);

            // Assert
            actual.Status.Should().BeEquivalentTo(ExecutionStatus.CompletedWithErrors);
            actual.TaggedDocumentsCount.Should().Be(0);

            foreach (List<int> chunk in chunks)
            {
                _tagRepositoryFake.Verify(
                x => x.TagDocumentsAsync(
                        It.IsAny<int>(),
                        It.Is<List<int>>(y => y.All(z => chunk.Contains(z))),
                        It.IsAny<int>(),
                        It.IsAny<int>()));
            }

            actual.FailedDocuments.Should().HaveCount(documentIds.Count);
        }

        private RelativityObjectSlim GetRelativityObjectSlimById(int id)
        {
            return new RelativityObjectSlim
            {
                ArtifactID = id,
                Values = new List<object> { _fxt.Create<string>() }
            };
        }

        private void SetupExportBatcher(IEnumerable<int> ids)
        {
            IEnumerable<IList<int>> splittedIds = ids.Chunk(2);

            foreach (var splitIdsItem in splittedIds)
            {
                _exportBatchFake.SetupSequence(x => x.GetNextItemsFromBatchAsync(CancellationToken.None))
                    .Returns(Task.FromResult(splitIdsItem.Select(x => GetRelativityObjectSlimById(x)).ToArray()));
            }
        }
    }
}
