using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Import.V1.Models.Errors;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class ItemLevelErrorHandlerTests
    {
        private Mock<IItemLevelErrorHandlerConfiguration> _configurationFake;
        private Mock<IJobHistoryErrorRepository> _jobHistoryErrorRepositoryMock;
        private Mock<IItemStatusMonitor> _statusMonitorMock;
        private Mock<IItemLevelErrorLogAggregator> _itemLevelErrorAggregatorFake;

        private ItemLevelErrorHandler _sut;

        private IFixture _fxt;

        [SetUp]
        public void Setup()
        {
            _fxt = FixtureFactory.Create();

            _configurationFake = new Mock<IItemLevelErrorHandlerConfiguration>();
            _configurationFake.Setup(x => x.SourceWorkspaceArtifactId)
                .Returns(_fxt.Create<int>());
            _configurationFake.Setup(x => x.JobHistoryArtifactId)
                .Returns(_fxt.Create<int>());

            _jobHistoryErrorRepositoryMock = new Mock<IJobHistoryErrorRepository>();
            _statusMonitorMock = new Mock<IItemStatusMonitor>();
            _itemLevelErrorAggregatorFake = new Mock<IItemLevelErrorLogAggregator>();

            _sut = new ItemLevelErrorHandler(
                _configurationFake.Object,
                _jobHistoryErrorRepositoryMock.Object,
                _itemLevelErrorAggregatorFake.Object);
        }

        [Test]
        public async Task HandleItemLevelError_ShouldPrepareErrorEntry()
        {
            // Arrange
            ItemLevelError itemLevelError = _fxt.Create<ItemLevelError>();

            // Act
            _sut.HandleItemLevelError(It.IsAny<long>(), itemLevelError);

            // Assert
            await _sut.HandleRemainingErrorsAsync().ConfigureAwait(false);

            _jobHistoryErrorRepositoryMock.Verify(
                x => x.MassCreateAsync(
                    _configurationFake.Object.SourceWorkspaceArtifactId,
                    _configurationFake.Object.JobHistoryArtifactId,
                    It.Is<IList<CreateJobHistoryErrorDto>>(
                        errors => errors.Any(
                            e => e.ErrorMessage == itemLevelError.Message &&
                                e.SourceUniqueId == itemLevelError.Identifier))));
        }

        [Test]
        public async Task HandleRemainingErrorsAsync_ShouldHandleItemLevelErrorsInBatches()
        {
            // Arrange
            const int expectedErrorBatchesCount = 2;
            const int expectedErrorsCount = 15000;
            IEnumerable<ItemLevelError> itemLevelErrors = _fxt.CreateMany<ItemLevelError>(expectedErrorsCount);

            itemLevelErrors.ForEach(e => _sut.HandleItemLevelError(It.IsAny<long>(), e));

            // Act
            await _sut.HandleRemainingErrorsAsync().ConfigureAwait(false);

            // Assert
            _jobHistoryErrorRepositoryMock.Verify(
                x => x.MassCreateAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<IList<CreateJobHistoryErrorDto>>()),
                Times.Exactly(expectedErrorBatchesCount));
        }

        [Test]
        public async Task HandleIAPIItemLevelErrorsAsync_ShouldNotThrow_WhenDocumentIdentifierIsNotPresent()
        {
            // Arrange
            ImportErrors errors = CreateImportErrorsWithProperties(() => new Dictionary<string, string>());

            // Act
            Func<Task> action = async () => await _sut.HandleIAPIItemLevelErrorsAsync(errors).ConfigureAwait(false);

            // Assert
            action.Should().NotThrow();

            await _sut.HandleRemainingErrorsAsync().ConfigureAwait(false);

            _jobHistoryErrorRepositoryMock.Verify(
                x => x.MassCreateAsync(
                    _configurationFake.Object.SourceWorkspaceArtifactId,
                    _configurationFake.Object.JobHistoryArtifactId,
                    It.IsAny<IList<CreateJobHistoryErrorDto>>()));
        }

        [Test]
        public async Task HandleIAPIItemLevelErrorsAsync_ShouldCreateErrorBasedOnIdentifier()
        {
            // Arrange
            ImportErrors errors = CreateImportErrorsWithProperties(() => new Dictionary<string, string>
            {
                { "Identifier", _fxt.Create<string>() }
            });

            // Act
            Func<Task> action = async () => await _sut.HandleIAPIItemLevelErrorsAsync(errors).ConfigureAwait(false);

            // Assert
            action.Should().NotThrow();

            await _sut.HandleRemainingErrorsAsync().ConfigureAwait(false);

            _jobHistoryErrorRepositoryMock.Verify(
                x => x.MassCreateAsync(
                    _configurationFake.Object.SourceWorkspaceArtifactId,
                    _configurationFake.Object.JobHistoryArtifactId,
                    It.Is<IList<CreateJobHistoryErrorDto>>(
                        y => y.All(e => e.SourceUniqueId != "[NOT_FOUND]"))));
        }

        [Test]
        public async Task HandleIAPIItemLevelErrorsAsync_ShouldHandleIAPIErrorsEvenIfMultiple()
        {
            // Arrange
            ImportErrors errors = _fxt.Create<ImportErrors>();

            int expectedErrorsCount = errors.Errors.Select(x => x.ErrorDetails.Count).Sum();

            // Act
            await _sut.HandleIAPIItemLevelErrorsAsync(errors).ConfigureAwait(false);

            // Assert
            await _sut.HandleRemainingErrorsAsync().ConfigureAwait(false);

            _jobHistoryErrorRepositoryMock.Verify(
                x => x.MassCreateAsync(
                    _configurationFake.Object.SourceWorkspaceArtifactId,
                    _configurationFake.Object.JobHistoryArtifactId,
                    It.Is<IList<CreateJobHistoryErrorDto>>(
                        e => e.Count == expectedErrorsCount)));
        }

        [Test]
        public async Task HandleIAPIItemLevelErrorsAsync_Should()
        {
            // Arrange
            ImportErrors errors = _fxt.Create<ImportErrors>();

            int expectedErrorsCount = errors.Errors.Select(x => x.ErrorDetails.Count).Sum();

            // Act
            await _sut.HandleIAPIItemLevelErrorsAsync(errors).ConfigureAwait(false);

            // Assert
            await _sut.HandleRemainingErrorsAsync().ConfigureAwait(false);

            _jobHistoryErrorRepositoryMock.Verify(
                x => x.MassCreateAsync(
                    _configurationFake.Object.SourceWorkspaceArtifactId,
                    _configurationFake.Object.JobHistoryArtifactId,
                    It.Is<IList<CreateJobHistoryErrorDto>>(
                        e => e.Count == expectedErrorsCount)));
        }

        private ImportErrors CreateImportErrorsWithProperties(Func<Dictionary<string, string>> propertiesFunc)
        {
            int importErrorsCount = _fxt.Create<int>();
            int errorDetailsCount = _fxt.Create<int>();

            List<ErrorDetail> errorDetails = _fxt.Build<ErrorDetail>()
                .With(y => y.ErrorProperties, propertiesFunc())
                .CreateMany(errorDetailsCount)
                .ToList();

            List<ImportError> importErrors =
                _fxt.Build<ImportError>()
                    .With(x => x.ErrorDetails, errorDetails)
                    .CreateMany(importErrorsCount)
                    .ToList();

            ImportErrors errors = _fxt.Build<ImportErrors>()
                .With(x => x.Errors, importErrors)
                .Create();

            return errors;
        }
    }
}
