using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using kCura.IntegrationPoints.EventHandlers.Commands.Helpers;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture]
    public class UpdateIntegrationPointConfigurationCommandBaseTests : TestBase
    {
        private Mock<IEHHelper> _helperFake;
        private Mock<IRelativityObjectManager> _relativityObjectManagerMock;
        private Mock<IObjectManager> _objectManagerMock;
        private Mock<IExportQueryResult> _exportQueryResultMock;

        private UpdateIntegrationPointConfigurationCommandBaseForTest _sut;

        private const string _ENTITY_TOO_LARGE_EXCEPTION = "Request Entity Too Large";
        private const int _BLOCK_SIZE = 1000;

        private readonly List<SourceProvider> _sourceProviders = new List<SourceProvider>
        {
            new SourceProvider { ArtifactId = 1 }
        };

        public override void SetUp()
        {
            _exportQueryResultMock = new Mock<IExportQueryResult>();

            _relativityObjectManagerMock = new Mock<IRelativityObjectManager>();
            _relativityObjectManagerMock.Setup(x => x.QueryWithExportAsync(
                It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<ExecutionIdentity>())).ReturnsAsync(_exportQueryResultMock.Object);

            _objectManagerMock = new Mock<IObjectManager>();

            Mock<IServicesMgr> servicesMgrFake = new Mock<IServicesMgr>();
            servicesMgrFake.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns(_objectManagerMock.Object);

            IAPILog log = NSubstitute.Substitute.For<IAPILog>();

            Mock<ILogFactory> logFactoryFake = new Mock<ILogFactory>();
            logFactoryFake.Setup(x => x.GetLogger()).Returns(log);

            _helperFake = new Mock<IEHHelper>();
            _helperFake.Setup(x => x.GetLoggerFactory()).Returns(logFactoryFake.Object);
            _helperFake.Setup(x => x.GetServicesManager()).Returns(servicesMgrFake.Object);

            _sut = new UpdateIntegrationPointConfigurationCommandBaseForTest(_helperFake.Object, _relativityObjectManagerMock.Object);
        }

        [Test]
        public void Execute_ShouldThrow_WhenRetrievingIntegrationPointsThrow()
        {
            // Arrange
            _relativityObjectManagerMock.Setup(x => x.Query<SourceProvider>(It.IsAny<QueryRequest>(), ExecutionIdentity.System))
                .Throws<TimeoutException>();

            // Act & Assert
            Action action = () => _sut.Execute();

            action.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void Execute_ShouldNotThrow_WhenNoSourceProviderIsReturned()
        {
            // Arrange
            _relativityObjectManagerMock.Setup(x => x.Query<SourceProvider>(It.IsAny<QueryRequest>(), ExecutionIdentity.System))
                .Returns(new List<SourceProvider>());

            // Act
            _sut.Execute();

            // Assert
            _relativityObjectManagerMock.Verify(x => x.QueryWithExportAsync(It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<ExecutionIdentity>()),
                Times.Never);
        }

        [Test]
        public void Execute_ShouldNotThrow_WhenNoIntegrationPointsAreReturned()
        {
            // Arrange
            SetupRead(0);

            // Act
            _sut.Execute();

            // Assert
            _objectManagerMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdatePerObjectsRequest>()), Times.Never);
        }

        [Test]
        public void Execute_ShouldNotThrow_WhenObjectToUpdateIsNullAfterUpdating()
        {
            // Arrange
            const int integrationPointsCount = 1;

            SetupRead(integrationPointsCount);

            _sut.UpdateFieldsFunc = x =>
            {
                return null;
            };

            // Act & Assert
            Action action = () => _sut.Execute();

            action.ShouldNotThrow();
        }

        [Test]
        public void Execute_ShouldThrowException_WhenFieldsToUpdateDoesNotMatchWithFieldsNamesForUpdateProperty()
        {
            // Arrange
            const int integrationPointsCount = 1;

            SetupRead(integrationPointsCount);

            _sut.UpdateFieldsFunc = x =>
            {
                x.FieldValues["Invalid field"] = "Test";
                return x;
            };

            // Act & Assert
            Action action = () => _sut.Execute();

            action.ShouldThrow<CommandExecutionException>();
        }

        [TestCase(0, 1)]
        [TestCase(1000, 2)]
        [TestCase(2000, 3)]
        public void Execute_ShouldReadDataInBlocks(int integrationPointsCount, int expectedCalls)
        {
            // Arrange
            SetupRead(integrationPointsCount);

            SetupMassUpdate(_BLOCK_SIZE);

            // Act
            _sut.Execute();

            // Assert
            _exportQueryResultMock.Verify(x => x.GetNextBlockAsync(It.IsAny<int>(), _BLOCK_SIZE),
                Times.Exactly(expectedCalls));
        }

        [TestCase(0, 5, 0)]
        [TestCase(4, 5, 1)]
        [TestCase(5, 5, 3)]
        [TestCase(6, 5, 3)]
        public void Execute_ShouldUpdateIntegrationPointsInChunks_WhenEntityTooLargeExceptionOccurs(int integrationPointsCount, int lessThanItemsPerRequest, int expectedCalls)
        {
            // Arrange
            SetupRead(integrationPointsCount);

            SetupMassUpdate(lessThanItemsPerRequest);

            // Act
            _sut.Execute();

            // Assert
            _objectManagerMock.Verify(x => x.UpdateAsync(
                It.IsAny<int>(), It.IsAny<MassUpdatePerObjectsRequest>()),
                Times.Exactly(expectedCalls));
        }

        [Test]
        public void Execute_ShouldThrowException_WhenEvenSingleItemIsTooLargeForObjectManager()
        {
            // Arrange
            const int integrationPointsCount = 3;
            SetupRead(integrationPointsCount);

            const int lessThanItemsPerRequest = 1;
            SetupMassUpdate(lessThanItemsPerRequest);

            // Act & Assert
            Action action = () => _sut.Execute();

            action.ShouldThrow<CommandExecutionException>();
        }

        private void SetupRead(int integrationPointsCount)
        {
            SetupExportQueryResult(integrationPointsCount);

            _relativityObjectManagerMock.Setup(x => x.Query<SourceProvider>(It.IsAny<QueryRequest>(), ExecutionIdentity.System))
                .Returns(_sourceProviders);
        }

        private void SetupExportQueryResult(int integrationPointsCount)
        {
            IList<string> fieldNames = _sut.FieldsNamesForUpdate_VIEW;

            RelativityObjectSlim testObject = new RelativityObjectSlim
            {
                ArtifactID = 1,
                Values = Enumerable.Repeat<object>(string.Empty, fieldNames.Count).ToList()
            };

            _exportQueryResultMock.Setup(x => x.ExportResult).Returns(new ExportInitializationResults
            {
                FieldData = fieldNames.Select(x => new FieldMetadata { Name = x }).ToList()
            });
            _exportQueryResultMock.Setup(x => x.GetNextBlockAsync(0, _BLOCK_SIZE))
                .ReturnsAsync(Enumerable.Repeat(testObject, integrationPointsCount > _BLOCK_SIZE ? _BLOCK_SIZE : integrationPointsCount));
            _exportQueryResultMock.Setup(x => x.GetNextBlockAsync(It.Is<int>(i => i < integrationPointsCount), _BLOCK_SIZE))
                .ReturnsAsync(Enumerable.Repeat(testObject, integrationPointsCount > _BLOCK_SIZE ? _BLOCK_SIZE : integrationPointsCount));
            _exportQueryResultMock.Setup(x => x.GetNextBlockAsync(It.Is<int>(i => i >= integrationPointsCount), _BLOCK_SIZE))
                .ReturnsAsync(new List<RelativityObjectSlim>());
        }

        private void SetupMassUpdate(int lessThanItemsPerRequest)
        {
            _objectManagerMock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.Is<MassUpdatePerObjectsRequest>(req => req.ObjectValues.Count >= lessThanItemsPerRequest)))
                .Throws(new ServiceException(_ENTITY_TOO_LARGE_EXCEPTION));
            _objectManagerMock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.Is<MassUpdatePerObjectsRequest>(req => req.ObjectValues.Count < lessThanItemsPerRequest)))
                .Returns((int workspaceId, MassUpdatePerObjectsRequest request) => Task.FromResult(new MassUpdateResult()));
        }

        private class UpdateIntegrationPointConfigurationCommandBaseForTest : UpdateIntegrationPointConfigurationCommandBase
        {
            public List<string> FieldsNamesForUpdate_VIEW { get; } = new List<string>
            {
                IntegrationPointFields.Name
            };

            public Func<RelativityObjectSlimDto, RelativityObjectSlimDto> UpdateFieldsFunc { get; set; }

            public UpdateIntegrationPointConfigurationCommandBaseForTest(IEHHelper helper, IRelativityObjectManager objectManager)
                : base(helper, objectManager)
            {
            }

            protected override string SourceProviderGuid => Guid.NewGuid().ToString();

            protected override IList<string> FieldsNamesForUpdate => FieldsNamesForUpdate_VIEW;

            protected override RelativityObjectSlimDto UpdateFields(RelativityObjectSlimDto values) 
                => UpdateFieldsFunc != null ? UpdateFieldsFunc(values) : values;
        }
    }
}
