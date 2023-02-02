using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation
{
    [TestFixture]
    [Category("Unit")]
    public class ObjectManagerFacadeTests
    {
        private const int _WORKSPACE_ID = 101;

        private Mock<IObjectManager> _objectManagerMock;
        private ObjectManagerFacade _sut;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            _sut = new ObjectManagerFacade(() => _objectManagerMock.Object);
        }

        [Test]
        public async Task CreateAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var request = new CreateRequest();
            var expectedResult = new CreateResult();

            _objectManagerMock.Setup(x => x.CreateAsync(
                    It.IsAny<int>(),
                    It.IsAny<CreateRequest>()))
                .ReturnsAsync(expectedResult);

            // act
            CreateResult actualResult = await _sut
                .CreateAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task ReadAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var request = new ReadRequest();
            var expectedResult = new ReadResult();

            _objectManagerMock.Setup(x => x.ReadAsync(
                    It.IsAny<int>(),
                    It.IsAny<ReadRequest>()))
                .ReturnsAsync(expectedResult);

            // act
            ReadResult actualResult = await _sut
                .ReadAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task UpdateAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var request = new UpdateRequest();
            var expectedResult = new UpdateResult();

            _objectManagerMock.Setup(x => x.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateRequest>()))
                .ReturnsAsync(expectedResult);

            // act
            UpdateResult actualResult = await _sut
                .UpdateAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task QueryAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            const int start = 0;
            const int length = 1;
            var request = new QueryRequest();
            var expectedResult = new QueryResult();

            _objectManagerMock.Setup(x => x.QueryAsync(
                    It.IsAny<int>(),
                    It.IsAny<QueryRequest>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(expectedResult);

            // act
            QueryResult actualResult = await _sut
                .QueryAsync(_WORKSPACE_ID, request, start, length)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task QuerySlimAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            const int start = 0;
            const int length = 1;
            var request = new QueryRequest();
            var expectedResult = new QueryResultSlim();

            _objectManagerMock.Setup(x => x.QuerySlimAsync(
                    It.IsAny<int>(),
                    It.IsAny<QueryRequest>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(expectedResult);

            // act
            QueryResultSlim actualResult = await _sut
                .QuerySlimAsync(_WORKSPACE_ID, request, start, length)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var request = new DeleteRequest();
            var expectedResult = new DeleteResult();

            _objectManagerMock.Setup(x => x.DeleteAsync(
                    It.IsAny<int>(),
                    It.IsAny<DeleteRequest>()))
                .ReturnsAsync(expectedResult);

            // act
            DeleteResult actualResult = await _sut
                .DeleteAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task MassDeleteAsync_ShouldReturnSameResultAsObjectManager()
        {
            var request = new MassDeleteByObjectIdentifiersRequest();
            var expectedResult = new MassDeleteResult();

            _objectManagerMock.Setup(x => x.DeleteAsync(
                    It.IsAny<int>(),
                    It.IsAny<MassDeleteByObjectIdentifiersRequest>()))
                .ReturnsAsync(expectedResult);

            // act
            MassDeleteResult actualResult = await _sut.DeleteAsync(_WORKSPACE_ID, request).ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public void MassDeleteAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var request = new MassDeleteByObjectIdentifiersRequest();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = () => _sut
                .DeleteAsync(_WORKSPACE_ID, request);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public async Task StreamLongTextAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var relativityObjectRef = new RelativityObjectRef();
            var fieldRef = new FieldRef();
            IKeplerStream expectedResult = new Mock<IKeplerStream>().Object;

            _objectManagerMock.Setup(x => x.StreamLongTextAsync(
                    It.IsAny<int>(),
                    It.IsAny<RelativityObjectRef>(),
                    It.IsAny<FieldRef>()))
                .ReturnsAsync(expectedResult);

            // act
            IKeplerStream actualResult = await _sut
                .StreamLongTextAsync(_WORKSPACE_ID, relativityObjectRef, fieldRef)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task InitializeExportAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var queryRequest = new QueryRequest();
            const int start = 5;
            ExportInitializationResults expectedResult = new ExportInitializationResults();

            _objectManagerMock.Setup(x => x.InitializeExportAsync(
                    _WORKSPACE_ID,
                    queryRequest,
                    start))
                .ReturnsAsync(expectedResult);

            // act
            ExportInitializationResults actualResult = await _sut
                .InitializeExportAsync(_WORKSPACE_ID, queryRequest, start)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, queryRequest, start), Times.Once);
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task RetrieveResultsBlockFromExportAsync_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            Guid runID = Guid.Parse("EA150180-3A58-4DFF-AA6C-6385075FCFD3");
            const int resultsBlockSize = 5;
            const int exportIndexID = 0;
            RelativityObjectSlim relativityObjectSlim = new RelativityObjectSlim();
            RelativityObjectSlim[] expectedResult = { relativityObjectSlim };

            _objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ID,
                    runID,
                    resultsBlockSize,
                    exportIndexID))
                .ReturnsAsync(expectedResult);

            // act
            RelativityObjectSlim[] actualResult = await _sut
                .RetrieveResultsBlockFromExportAsync(_WORKSPACE_ID, runID, resultsBlockSize, exportIndexID)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(_WORKSPACE_ID, runID, resultsBlockSize, exportIndexID), Times.Once);
            actualResult.Should().BeSameAs(expectedResult);
        }

        [Test]
        public void CreateAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var request = new CreateRequest();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = async () => await _sut
                .CreateAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void ReadAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var request = new ReadRequest();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = async () => await _sut
                .ReadAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void UpdateAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var request = new UpdateRequest();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = async () => await _sut
                .UpdateAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void QueryAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            const int start = 0;
            const int length = 1;
            var request = new QueryRequest();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = async () => await _sut
                .QueryAsync(_WORKSPACE_ID, request, start, length)
                .ConfigureAwait(false);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void DeleteAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var request = new DeleteRequest();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = async () => await _sut
                .DeleteAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void StreamLongTextAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var relativityObjectRef = new RelativityObjectRef();
            var fieldRef = new FieldRef();

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = async () => await _sut
                .StreamLongTextAsync(_WORKSPACE_ID, relativityObjectRef, fieldRef)
                .ConfigureAwait(false);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public async Task UpdateAsync_MassUpdate_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            var request = new MassUpdateByObjectIdentifiersRequest();
            var options = new MassUpdateOptions();
            var expectedResult = new MassUpdateResult();

            _objectManagerMock
                .Setup(x => x.UpdateAsync(_WORKSPACE_ID, request, options))
                .ReturnsAsync(expectedResult);

            // act
            MassUpdateResult actualResult = await _sut
                .UpdateAsync(_WORKSPACE_ID, request, options)
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public void InitializeExportAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var queryRequest = new QueryRequest();
            const int start = 5;

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = () => _sut
                .InitializeExportAsync(_WORKSPACE_ID, queryRequest, start);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void UpdateAsync_MassUpdate_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            var request = new MassUpdateByObjectIdentifiersRequest();
            var options = new MassUpdateOptions();
            var exceptionToThrow = new Exception();

            _objectManagerMock
                .Setup(x => x.UpdateAsync(_WORKSPACE_ID, request, options))
                .Throws(exceptionToThrow);

            // act
            Func<Task> massUpdateAction = () => _sut.UpdateAsync(_WORKSPACE_ID, request, options);

            // assert
            massUpdateAction.ShouldThrow<Exception>()
                .Which.Should().Be(exceptionToThrow);
        }

        [Test]
        public void RetrieveResultsBlockFromExportAsync_ShouldThrowWhenObjectManagerNotInitialized()
        {
            // arrange
            Guid runID = Guid.Parse("EA150180-3A58-4DFF-AA6C-6385075FCFD3");
            const int resultsBlockSize = 5;
            const int exportIndexID = 0;

            _sut = new ObjectManagerFacade(() => null);

            // act
            Func<Task> action = () => _sut
                .RetrieveResultsBlockFromExportAsync(_WORKSPACE_ID, runID, resultsBlockSize, exportIndexID);

            // assert
            action.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public async Task This_ShouldDisposeObjectManagerWhenItsAlreadyCreated()
        {
            // arrange
            ReadRequest request = new ReadRequest();
            await _sut
                .ReadAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // act
            _sut.Dispose();

            // assert
            _objectManagerMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void This_ShouldNotDisposeObjectManagerWhenItsNotCreated()
        {
            // act
            _sut.Dispose();

            // assert
            _objectManagerMock.Verify(x => x.Dispose(), Times.Never);
        }

        [Test]
        public async Task This_ShouldDisposeObjectManagerOnlyOnceWhenItsAlreadyCreated()
        {
            // arrange
            ReadRequest request = new ReadRequest();
            await _sut
                .ReadAsync(_WORKSPACE_ID, request)
                .ConfigureAwait(false);

            // act
            _sut.Dispose();
            _sut.Dispose();

            // assert
            _objectManagerMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
