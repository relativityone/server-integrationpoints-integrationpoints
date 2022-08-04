
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{


    [TestFixture, Category("Unit")]
    public class ExportQueryResultTests
    {
        private Mock<IObjectManagerFacadeFactory> _objectManagerFacadeFactoryMock;
        private Mock<IObjectManagerFacade> _objectManagerFacadeMock;
        private Guid _runID;
        private ExportInitializationResults _exportInitializationResults;

        private readonly int _WORKSPACE_ID = 5;
        private ExportQueryResult _sut;
        private Mock<Action<Exception, int, int>> _exceptionHandlerMock;

        [SetUp]
        public void SetUp()
        {
            _runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");

            _exportInitializationResults = new ExportInitializationResults
            {
                RunID = _runID,
                RecordCount = 5000
            };

            _objectManagerFacadeMock = new Mock<IObjectManagerFacade>();
            _objectManagerFacadeFactoryMock = new Mock<IObjectManagerFacadeFactory>();
            _objectManagerFacadeFactoryMock
                .Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
                .Returns(_objectManagerFacadeMock.Object);

            _exceptionHandlerMock = new Mock<Action<Exception, int, int>>();

            _sut = new ExportQueryResult(_objectManagerFacadeFactoryMock.Object,
                _exportInitializationResults, _WORKSPACE_ID, ExecutionIdentity.System, _exceptionHandlerMock.Object);
        }

        [Test]
        public void ExportQueryResult_ShouldDeleteExportTable_WhenDisposed()
        {
            // Act
            _sut.Dispose();

            // Assert
            _objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(_WORKSPACE_ID, _runID, 0, (int)_exportInitializationResults.RecordCount));
        }

        [Test]
        public void GetAllResultsAsync_ShouldCallExceptionHandler()
        {
            // Arrange
            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        0))
                .Throws<IntegrationPointsException>();

            // Act
            Func<Task> action = () => _sut.GetAllResultsAsync();

            // Assert
            action.ShouldThrow<IntegrationPointsException>();
            _exceptionHandlerMock.Verify(x => x(It.IsAny<Exception>(), It.IsAny<int>(), 0), Times.Once);
        }

        [Test]
        public void GetNextBlockAsync_ShouldCallExceptionHandler()
        {
            // Arrange
            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        0))
                .Throws<IntegrationPointsException>();

            // Act
            Func<Task> action = () => _sut.GetNextBlockAsync(0);

            // Assert
            action.ShouldThrow<IntegrationPointsException>();
            _exceptionHandlerMock.Verify(x => x(It.IsAny<Exception>(), It.IsAny<int>(), 0), Times.Once);
        }

        [Test]
        public async Task GetNextBlockAsync_ShouldCallFacadeMultipleTimes_WhenBlockIsNotReturnedAtOnce()
        {
            // Arrange
            const int requestedBlockSize = 1500;
            const int toReturn = 400;

            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(RetrieveResultsBlockFromExportAsyncMock(toReturn));

            // Act
            RelativityObjectSlim[] block = (await _sut.GetNextBlockAsync(0, requestedBlockSize).ConfigureAwait(false)).ToArray();

            // Assert
            block.Length.Should().Be(requestedBlockSize);

            _objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
                _WORKSPACE_ID,
                _runID,
                requestedBlockSize - toReturn,
                toReturn), Times.Once);

            _objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
                _WORKSPACE_ID,
                _runID,
                requestedBlockSize - toReturn,
                toReturn), Times.Once);
        }

        [Test]
        public async Task GetAllResultsAsync_ShouldCallFacadeMultipleTimes_WhenBlockIsNotReturnedAtOnce()
        {
            // Arrange
            int toReturn = 411;

            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(RetrieveResultsBlockFromExportAsyncMock(toReturn));

            // Act
            RelativityObjectSlim[] block = (await _sut.GetAllResultsAsync().ConfigureAwait(false)).ToArray();

            // Assert
            block.Length.Should().Be((int)_exportInitializationResults.RecordCount);

            int facadeExpectedCallCount = (int)(Math.Ceiling(_exportInitializationResults.RecordCount / (double)toReturn));

            _objectManagerFacadeMock
                .Verify(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsAny<int>()), Times.Exactly(facadeExpectedCallCount));
        }

        [TestCase(6000, 0)]
        [TestCase(1, 6000)]
        public async Task GetNextBlockAsync_ShouldNotDeleteExportTable(int blockSize, int startIndex)
        {
            // export table is deleted when RetrieveResultsBlockFromExportAsync is called with startIndex >= RecordsCount

            // Arrange
            int toReturn = 1000;

            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(RetrieveResultsBlockFromExportAsyncMock(toReturn));

            // Act
            await _sut.GetNextBlockAsync(startIndex, blockSize).ConfigureAwait(false);

            // Assert

            _objectManagerFacadeMock
                .Verify(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsInRange((int)_exportInitializationResults.RecordCount, Int32.MaxValue, Range.Inclusive)), Times.Never);
        }

        [Test]
        public async Task GetAllResultsAsync_ShouldNotDeleteExportTable()
        {
            // export table is deleted when RetrieveResultsBlockFromExportAsync is called with startIndex >= RecordsCount

            // Arrange
            int toReturn = 1000;

            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(RetrieveResultsBlockFromExportAsyncMock(toReturn));

            // Act
            await _sut.GetAllResultsAsync().ConfigureAwait(false);

            // Assert

            _objectManagerFacadeMock
                .Verify(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        It.IsAny<int>(),
                        It.IsInRange((int)_exportInitializationResults.RecordCount, Int32.MaxValue, Range.Inclusive)), Times.Never);
        }

        private static Func<int, Guid, int, int, Task<RelativityObjectSlim[]>> RetrieveResultsBlockFromExportAsyncMock(int toReturn)
        {
            return (int workspaceId, Guid runId, int blockSize, int startIndex) =>
            {
                var result = Enumerable.Repeat(new RelativityObjectSlim(), Math.Min(blockSize, toReturn)).ToArray();
                return Task.FromResult(result);
            };
        }

        [Test]
        public async Task GetNextBlockAsync_ShouldRespectNextBlockStartIndex()
        {
            // Arrange
            const int blockSize = 500;
            const int startIndex = 666;


            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        blockSize,
                        It.IsAny<int>()))
                .ReturnsAsync(Enumerable.Repeat(new RelativityObjectSlim(), blockSize).ToArray());


            // Act
            await _sut.GetNextBlockAsync(startIndex, blockSize).ConfigureAwait(false);

            // Assert
            _objectManagerFacadeMock
                .Verify(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ID,
                        _runID,
                        blockSize,
                        startIndex), Times.Once);
        }
    }
}
