using Relativity.API;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.ChoiceQueryManager.Models;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using QueryRequest = Relativity.Services.Objects.DataContracts.QueryRequest;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    public class NonDocumentObjectDataSourceSnapshotExecutorTests
    {
        private NonDocumentObjectDataSourceSnapshotExecutor _sut;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUserMock;
        private Mock<IJobProgressUpdaterFactory> _jobProgressUpdaterFactoryMock;
        private Mock<ISnapshotQueryRequestProvider> _snapshotQueryRequestProvider;
        private Mock<IAPILog> _logMock;
        private Mock<IJobProgressUpdater> _jobProgressUpdater;

        private static Guid AllObjectsExportGuid = Guid.Parse("C942F549-7262-4D09-9C04-DFA8BDA97D61");
        private static Guid LinkingObjectsExportGuid = Guid.Parse("2377DFEB-BE8A-4D43-9E20-69842A9CE248");

        private int AllObjectsCount = 68;
        private int LinkingObjectsCount = 70;
        private QueryRequest AllObjectsRequest;
        private QueryRequest LinkingObjectsRequest;
        private ConfigurationStub _configuration;

        [SetUp]
        public void Setup()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            _serviceFactoryForUserMock = new Mock<ISourceServiceFactoryForUser>();

            _serviceFactoryForUserMock.Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManagerMock.Object);

            _jobProgressUpdaterFactoryMock = new Mock<IJobProgressUpdaterFactory>();
            _jobProgressUpdater = new Mock<IJobProgressUpdater>();
            _jobProgressUpdaterFactoryMock.Setup(x => x.CreateJobProgressUpdater())
                .Returns(_jobProgressUpdater.Object);

            _jobProgressUpdater
                .Setup(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            
            _snapshotQueryRequestProvider = new Mock<ISnapshotQueryRequestProvider>();
            _logMock = new Mock<IAPILog>();

            AllObjectsRequest = new QueryRequest();
            LinkingObjectsRequest = new QueryRequest();

            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), AllObjectsRequest, 1))
                .ReturnsAsync(() => new ExportInitializationResults
                {
                    FieldData = new List<FieldMetadata>(),
                    RecordCount = AllObjectsCount,
                    RunID = AllObjectsExportGuid
                });
            
            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), LinkingObjectsRequest, 1))
                .ReturnsAsync(() => new ExportInitializationResults
                {
                    FieldData = new List<FieldMetadata>(),
                    RecordCount = LinkingObjectsCount,
                    RunID = LinkingObjectsExportGuid
                });

            _snapshotQueryRequestProvider.Setup(x => x.GetRequestForCurrentPipelineAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(AllObjectsRequest);

            _snapshotQueryRequestProvider.Setup(x =>
                    x.GetRequestForLinkingNonDocumentObjectsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(LinkingObjectsRequest);

            _configuration = new ConfigurationStub();
            
            _sut = new NonDocumentObjectDataSourceSnapshotExecutor(_serviceFactoryForUserMock.Object, _jobProgressUpdaterFactoryMock.Object,_snapshotQueryRequestProvider.Object, _logMock.Object);
        }

        [Test]
        public async Task Execute_ShouldCompleteHappyPath()
        {
            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configuration, new CompositeCancellationTokenStub());
            
            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Completed);
            _configuration.ExportRunId.Should().Be(AllObjectsExportGuid);
            _configuration.TotalRecordsCount.Should().Be(AllObjectsCount);
            
            _configuration.ObjectLinkingSnapshotId.Should().Be(LinkingObjectsExportGuid);
            _configuration.ObjectLinkingSnapshotRecordsCount.Should().Be(LinkingObjectsCount);
        }

        [Test]
        public async Task Execute_ShouldNot_ExportLinkingObjects_WhenSnapshotProviderReturnsNull()
        {
            // Arrange
            _snapshotQueryRequestProvider.Setup(x =>
                    x.GetRequestForLinkingNonDocumentObjectsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null);
            
            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configuration, new CompositeCancellationTokenStub()).ConfigureAwait(false);
            
            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Completed);
            _configuration.ExportRunId.Should().Be(AllObjectsExportGuid);
            _configuration.TotalRecordsCount.Should().Be(AllObjectsCount);
            
            _objectManagerMock.Verify(x => x.InitializeExportAsync(It.IsAny<int>(), It.Is<QueryRequest>(q => q != AllObjectsRequest), It.IsAny<int>()), Times.Never);
            _configuration.ObjectLinkingSnapshotId.Should().Be(null);
            _configuration.ObjectLinkingSnapshotRecordsCount.Should().Be(0);
        }

        [Test]
        public async Task Execute_ShouldNotSaveExportRunForLinkingObjects_WhenCountIsZero()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), LinkingObjectsRequest, 1))
                .ReturnsAsync(() => new ExportInitializationResults
                {
                    FieldData = new List<FieldMetadata>(),
                    RecordCount = 0,
                    RunID = LinkingObjectsExportGuid
                });

            _objectManagerMock.Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), LinkingObjectsExportGuid, 0, 0))
                .ReturnsAsync(Array.Empty<RelativityObjectSlim>());
            
            // Act
            ExecutionResult result = await _sut.ExecuteAsync(_configuration, new CompositeCancellationTokenStub()).ConfigureAwait(false);
            
            // Assert
            result.Status.Should().BeEquivalentTo(ExecutionStatus.Completed);
            _configuration.ExportRunId.Should().Be(AllObjectsExportGuid);
            _configuration.TotalRecordsCount.Should().Be(AllObjectsCount);
            
            _configuration.ObjectLinkingSnapshotId.Should().Be(null);
            _configuration.ObjectLinkingSnapshotRecordsCount.Should().Be(0);

            // delete export table
            _objectManagerMock.Verify(x =>
                x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), LinkingObjectsExportGuid, 0, 0));
        }

        [Test]
        public async Task Execute_ShouldFail_WhenExportFails()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), AllObjectsRequest, 1))
                .Throws<Exception>();
            
            // Act
            var result = await _sut.ExecuteAsync(_configuration, new CompositeCancellationTokenStub())
                .ConfigureAwait(false); 
            
            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().Be("ExportAPI failed to initialize export for all non-document objects");
        }
        
        [Test]
        public async Task Execute_ShouldFail_WhenObjectLinkingExportFails()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), LinkingObjectsRequest, 1))
                .Throws<Exception>();
            
            // Act
            var result = await _sut.ExecuteAsync(_configuration, new CompositeCancellationTokenStub())
                .ConfigureAwait(false); 
            
            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().Be("ExportAPI failed to initialize export linking non-document objects");
        }
        
        [Test]
        public async Task Execute_ShouldFail_WhenBothExportsFail()
        {
            // Arrange
            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), LinkingObjectsRequest, 1))
                .Throws<Exception>();
            
            _objectManagerMock.Setup(x => x.InitializeExportAsync(It.IsAny<int>(), AllObjectsRequest, 1))
                .Throws<Exception>();
            
            // Act
            var result = await _sut.ExecuteAsync(_configuration, new CompositeCancellationTokenStub())
                .ConfigureAwait(false); 
            
            // Assert
            result.Status.Should().Be(ExecutionStatus.Failed);
            result.Message.Should().Be("Failed to initialize objects exports");
        }
    }
}
