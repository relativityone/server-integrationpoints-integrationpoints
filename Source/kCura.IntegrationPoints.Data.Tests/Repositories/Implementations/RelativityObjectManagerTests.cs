using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.StreamWrappers;
using kCura.IntegrationPoints.Domain.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Kepler.Transport;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class RelativityObjectManagerTests
    {
        private Mock<IAPILog> _apiLogMock;
        private Mock<IObjectManagerFacadeFactory> _objectManagerFacadeFactoryMock;
        private Mock<IObjectManagerFacade> _objectManagerFacadeMock;
        private RelativityObjectManager _sut;

        private const int _WORKSPACE_ARTIFACT_ID = 12345;
        private const int _REL_OBJECT_ARTIFACT_ID = 10;
        private const int _FIELD_ARTIFACT_ID = 789;

        [SetUp]
        public void SetUp()
        {
            _apiLogMock = new Mock<IAPILog>();
            _apiLogMock.Setup(x => x.ForContext<RelativityObjectManager>()).Returns(_apiLogMock.Object);
            _apiLogMock.Setup(x => x.ForContext<SelfDisposingStream>()).Returns(_apiLogMock.Object);
            _apiLogMock.Setup(x => x.ForContext<SelfRecreatingStream>()).Returns(_apiLogMock.Object);
            _objectManagerFacadeMock = new Mock<IObjectManagerFacade>();
            _objectManagerFacadeFactoryMock = new Mock<IObjectManagerFacadeFactory>();
            _objectManagerFacadeFactoryMock
                .Setup(x => x.Create(It.IsAny<ExecutionIdentity>()))
                .Returns(_objectManagerFacadeMock.Object);
            _sut = new RelativityObjectManager(
                _WORKSPACE_ARTIFACT_ID,
                _apiLogMock.Object,
                _objectManagerFacadeFactoryMock.Object);
        }

        [Test]
        public void StreamUnicodeLongText_ItShouldRethrowIntegrationPointException()
        {
            // arrange
            _objectManagerFacadeMock
                .Setup(x => 
                    x.StreamLongTextAsync(
                        It.IsAny<int>(), 
                        It.IsAny<RelativityObjectRef>(), 
                        It.IsAny<FieldRef>()))
                .Throws<IntegrationPointsException>();

            //act
            Action action = () => 
                _sut.StreamUnicodeLongText(
                    _REL_OBJECT_ARTIFACT_ID,
                    new FieldRef {ArtifactID = _FIELD_ARTIFACT_ID},
                    ExecutionIdentity.System);

            // assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void StreamNonUnicodeLongText_ItShouldRethrowIntegrationPointException()
        {
            // arrange
            _objectManagerFacadeMock
                .Setup(x =>
                    x.StreamLongTextAsync(
                        It.IsAny<int>(),
                        It.IsAny<RelativityObjectRef>(),
                        It.IsAny<FieldRef>()))
                .Throws<IntegrationPointsException>();

            // act
            Action action = () =>
                _sut.StreamNonUnicodeLongText(
                    _REL_OBJECT_ARTIFACT_ID,
                    new FieldRef { ArtifactID = _FIELD_ARTIFACT_ID },
                    ExecutionIdentity.System);

            // assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void StreamUnicodeLongText_ItShouldThrowExceptionWrappedInIntegrationPointException()
        {
            // arrange
            _objectManagerFacadeMock
                .Setup(x =>
                    x.StreamLongTextAsync(
                        It.IsAny<int>(),
                        It.IsAny<RelativityObjectRef>(),
                        It.IsAny<FieldRef>()))
                .Throws<Exception>();

            // act
            Action action = () => 
                _sut.StreamUnicodeLongText(
                    _REL_OBJECT_ARTIFACT_ID,
                    new FieldRef() {ArtifactID = _FIELD_ARTIFACT_ID},
                    ExecutionIdentity.System);

            // assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void StreamNonUnicodeLongText_ItShouldThrowExceptionWrappedInIntegrationPointException()
        {
            // arrange
            _objectManagerFacadeMock
                .Setup(x =>
                    x.StreamLongTextAsync(
                        It.IsAny<int>(),
                        It.IsAny<RelativityObjectRef>(),
                        It.IsAny<FieldRef>()))
                .Throws<Exception>();

            // act
            Action action = () =>
                _sut.StreamNonUnicodeLongText(
                    _REL_OBJECT_ARTIFACT_ID,
                    new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
                    ExecutionIdentity.System);

            // assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void StreamUnicodeLongText_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade()
        {
            // arrange
            Stream expectedStream = new Mock<Stream>().Object;
            var keplerStreamMock = new Mock<IKeplerStream>();
            keplerStreamMock.Setup(x => x.GetStreamAsync()).ReturnsAsync(expectedStream);
            _objectManagerFacadeMock
                .Setup(x =>
                    x.StreamLongTextAsync(
                        It.IsAny<int>(),
                        It.IsAny<RelativityObjectRef>(),
                        It.IsAny<FieldRef>()))
                .ReturnsAsync(keplerStreamMock.Object);

            // act
            Stream result = _sut.StreamUnicodeLongText(
                    _REL_OBJECT_ARTIFACT_ID,
                    new FieldRef() {ArtifactID = _FIELD_ARTIFACT_ID},
                    ExecutionIdentity.System);

            // assert
            result.Should().BeOfType<SelfDisposingStream>();
            var selfDisposingStream = (SelfDisposingStream) result;
            Stream innerStream = selfDisposingStream.InnerStream;
            innerStream.Should().BeOfType<SelfRecreatingStream>();
            var selfRecreatingStream = (SelfRecreatingStream) innerStream;
            selfRecreatingStream.InnerStream.Should().Be(expectedStream);
        }

        [Test]
        public void StreamNonUnicodeLongText_ItShouldReturnIOStreamGivenKeplerStreamFromRelativityObjectManagerFacade()
        {
            // arrange
            Stream expectedStream = new Mock<Stream>().Object;
            var keplerStreamMock = new Mock<IKeplerStream>();
            keplerStreamMock.Setup(x => x.GetStreamAsync()).ReturnsAsync(expectedStream);
            _objectManagerFacadeMock
                .Setup(x =>
                    x.StreamLongTextAsync(
                        It.IsAny<int>(),
                        It.IsAny<RelativityObjectRef>(),
                        It.IsAny<FieldRef>()))
                .ReturnsAsync(keplerStreamMock.Object);

            // act
            Stream result = _sut.StreamNonUnicodeLongText(
                _REL_OBJECT_ARTIFACT_ID,
                new FieldRef() { ArtifactID = _FIELD_ARTIFACT_ID },
                ExecutionIdentity.System);

            // assert
            result.Should().BeOfType<SelfDisposingStream>();
            var selfDisposingStream = (SelfDisposingStream) result;
            Stream innerStream1 = selfDisposingStream.InnerStream;
            innerStream1.Should().BeOfType<AsciiToUnicodeStream>();
            var asciiToUnicodeStream = (AsciiToUnicodeStream) innerStream1;
            Stream innerStream2 = asciiToUnicodeStream.AsciiStream;
            innerStream2.Should().BeOfType<SelfRecreatingStream>();
            var selfRecreatingStream = (SelfRecreatingStream) innerStream2;
            selfRecreatingStream.InnerStream.Should().Be(expectedStream);
        }

        [Test]
        public void MassDeleteAsync_ShouldRethrowIntegrationPointsException()
        {
            // arrange
            IntegrationPointsException expectedException = new IntegrationPointsException();
            _objectManagerFacadeMock
                .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<MassDeleteByObjectIdentifiersRequest>()))
                .Throws(expectedException);

            // act
            Func<Task> massDeleteAction = () => _sut.MassDeleteAsync(Enumerable.Empty<int>(), ExecutionIdentity.System);
            
            // assert
            massDeleteAction.ShouldThrow<IntegrationPointsException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void MassDeleteAsync_ShouldWrapExceptionInIntegrationPointException()
        {
            // arrange
            Exception expectedInnerException = new Exception();
            _objectManagerFacadeMock
                .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<MassDeleteByObjectIdentifiersRequest>()))
                .Throws(expectedInnerException);

            // act
            Func<Task> massDeleteAction = () => _sut.MassDeleteAsync(Enumerable.Empty<int>(), ExecutionIdentity.System);

            // assert
            massDeleteAction.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().Be(expectedInnerException);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task MassDeleteAsync_ShouldReturnValueFromObjectManagerFacade(bool isSuccess)
        {
            // arrange
            var massDeleteResult = new MassDeleteResult()
            {
                Success = isSuccess
            };
            _objectManagerFacadeMock
                .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<MassDeleteByObjectIdentifiersRequest>()))
                .ReturnsAsync(massDeleteResult);

            // act
            bool actualResult = await _sut.MassDeleteAsync(Enumerable.Empty<int>(), ExecutionIdentity.System).ConfigureAwait(false);

            // assert
            actualResult.Should().Be(isSuccess);
        }

        [Test]
        public async Task MassDeleteAsync_ShouldSendProperRequest()
        {
            // arrange
            var massDeleteResult = new MassDeleteResult()
            {
                Success = true
            };
            _objectManagerFacadeMock
                .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<MassDeleteByObjectIdentifiersRequest>()))
                .ReturnsAsync(massDeleteResult);
            IList<int> ids = Enumerable.Range(0, 3).ToList();

            // act
            await _sut.MassDeleteAsync(ids, ExecutionIdentity.System).ConfigureAwait(false);

            // assert
            Func<MassDeleteByObjectIdentifiersRequest, bool> requestVerifier = request => request.Objects.Select(item => item.ArtifactID).ToList().SequenceEqual(ids);
            _objectManagerFacadeMock.Verify(x => x.DeleteAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<MassDeleteByObjectIdentifiersRequest>(request => requestVerifier(request))));
        }

        [Test]
        public void MassUpdateAsync_ShouldRethrowIntegrationPointException()
        {
            // arrange
            var expectedException = new IntegrationPointsException();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
                        It.IsAny<MassUpdateOptions>()))
                .Throws(expectedException);

            // act
            Func<Task> massUpdateAction = () =>
                _sut.MassUpdateAsync(
                    Enumerable.Empty<int>(),
                    It.IsAny<IEnumerable<FieldRefValuePair>>(),
                    It.IsAny<FieldUpdateBehavior>(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            massUpdateAction.ShouldThrow<IntegrationPointsException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void MassUpdateAsync_ShouldWrapExceptionInIntegrationPointException()
        {
            // arrange
            var expectedInnerException = new Exception();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
                        It.IsAny<MassUpdateOptions>()))
                .Throws(expectedInnerException);

            // act
            Func<Task> massUpdateAction = () =>
                _sut.MassUpdateAsync(
                    Enumerable.Empty<int>(),
                    It.IsAny<IEnumerable<FieldRefValuePair>>(),
                    It.IsAny<FieldUpdateBehavior>(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            massUpdateAction.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().Be(expectedInnerException);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task MassUpdateAsync_ShouldReturnValueFromObjectManagerFacade(bool isSuccess)
        {
            // arrange
            var massUpdateResult = new MassUpdateResult
            {
                Success = isSuccess
            };
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
                        It.IsAny<MassUpdateOptions>()))
                .ReturnsAsync(massUpdateResult);

            // act
            bool actualResult = await _sut.MassUpdateAsync(
                    Enumerable.Empty<int>(),
                    It.IsAny<IEnumerable<FieldRefValuePair>>(),
                    It.IsAny<FieldUpdateBehavior>(),
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(isSuccess);
        }

        [Test]
        public async Task MassUpdateAsync_ShouldSendProperRequest()
        {
            // arrange
            var massUpdateResult = new MassUpdateResult
            {
                Success = true
            };

            IList<int> objectIDs = Enumerable.Range(0, 5).ToList();

            FieldRefValuePair[] fields =
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 1
                    },
                    Value = "one"
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 2
                    },
                    Value = "two"
                }
            };

            FieldUpdateBehavior updateBehavior = FieldUpdateBehavior.Merge;

            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
                        It.IsAny<MassUpdateOptions>()))
                .ReturnsAsync(massUpdateResult);

            // act
            await _sut.MassUpdateAsync(
                    objectIDs,
                    fields,
                    updateBehavior,
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            Func<MassUpdateByObjectIdentifiersRequest, bool> requestVerifier = request =>
            {
                bool isValid = true;
                isValid &= request.Objects.Select(x => x.ArtifactID).SequenceEqual(objectIDs);
                isValid &= request.FieldValues.SequenceEqual(fields);
                return isValid;
            };

            Func<MassUpdateOptions, bool> updateOptionsVerifier = options =>
                options.UpdateBehavior == updateBehavior;

            _objectManagerFacadeMock.Verify(x => x.UpdateAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<MassUpdateByObjectIdentifiersRequest>(request => requestVerifier(request)),
                It.Is<MassUpdateOptions>(options => updateOptionsVerifier(options)))
            );
        }

        [Test]
        public async Task InitializeExportAsync_ItShouldReturnSameResultAsFacade()
        {
            // arrange
            var queryRequest = new QueryRequest();
            const int start = 6;
            var expectedResult = new ExportInitializationResults();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.InitializeExportAsync(
                        _WORKSPACE_ARTIFACT_ID,
                        queryRequest,
                        start))
                .ReturnsAsync(expectedResult);

            // act
            ExportInitializationResults actualResult = await _sut.InitializeExportAsync(
                    queryRequest,
                    start,
                    ExecutionIdentity.System)
                .ConfigureAwait(false);

            // assert
            _objectManagerFacadeMock.Verify(x => x.InitializeExportAsync(
                _WORKSPACE_ARTIFACT_ID,
                queryRequest,
                start));
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public void InitializeExportAsync_ItShouldRethrowIntegrationPointException()
        {
            // arrange
            var queryRequest = new QueryRequest();
            const int start = 6;
            _objectManagerFacadeMock
                .Setup(x =>
                    x.InitializeExportAsync(
                        _WORKSPACE_ARTIFACT_ID,
                        queryRequest,
                        start))
                .Throws<IntegrationPointsException>();

            // act
            Func<Task> action = () =>
                _sut.InitializeExportAsync(
                    queryRequest,
                    start,
                    ExecutionIdentity.System);

            // assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void InitializeExportAsync_ItShouldThrowExceptionWrappedInIntegrationPointException()
        {
            // arrange
            var queryRequest = new QueryRequest();
            const int start = 6;
            _objectManagerFacadeMock
                .Setup(x =>
                    x.InitializeExportAsync(
                        _WORKSPACE_ARTIFACT_ID,
                        queryRequest,
                        start))
                .Throws<Exception>();

            // act
            Func<Task> action = () =>
                _sut.InitializeExportAsync(
                    queryRequest,
                    start,
                    ExecutionIdentity.System);

            // assert
            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public async Task RetrieveResultsBlockFromExport_ShouldCallFacadeOnce_WhenEntireBlockIsReturned()
        {
            // arrange
            Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
            const int resultsBlockSize = 10;
            const int exportIndexID = 0;
            RelativityObjectSlim[] expectedResult = CreateTestRelativityObjectsSlim(resultsBlockSize);
            _objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize,
                    exportIndexID))
                .ReturnsAsync(expectedResult);

            // act
            RelativityObjectSlim[] actualResult = await _sut.RetrieveResultsBlockFromExportAsync(
                    runID,
                    resultsBlockSize,
                    exportIndexID,
                    ExecutionIdentity.System)
                .ConfigureAwait(false);

            // assert
            _objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize,
                    exportIndexID),
                Times.Once);
            actualResult.ShouldBeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public async Task RetrieveResultsBlockFromExport_ShouldCallFacadeTwoTimes_WhenBlockIsReturnedInHalves()
        {
            // arrange
            Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
            const int resultsBlockSize = 10;
            const int returnedSize = 5;
            const int exportIndexID = 0;
            const int expectedCallsCount = 2;
            RelativityObjectSlim[] expectedResult = CreateTestRelativityObjectsSlim(resultsBlockSize);
            _objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize,
                    exportIndexID))
                .ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, 0, returnedSize));
            _objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize - returnedSize,
                    exportIndexID + returnedSize))
                .ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, returnedSize, returnedSize));

            // act
            RelativityObjectSlim[] actualResult = await _sut.RetrieveResultsBlockFromExportAsync(
                    runID,
                    resultsBlockSize,
                    exportIndexID,
                    ExecutionIdentity.System)
                .ConfigureAwait(false);

            // assert
            _objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Exactly(expectedCallsCount));
            actualResult.ShouldBeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public async Task RetrieveResultsBlockFromExport_ShouldCallFacadThreeTimes_WhenBlockIsReturnedInUnevenParts()
        {
            // arrange
            Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
            const int resultsBlockSize = 10;
            const int returnedSize = 4;
            const int exportIndexID = 0;
            const int expectedCallsCount = 3;
            RelativityObjectSlim[] expectedResult = CreateTestRelativityObjectsSlim(resultsBlockSize);
            _objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize,
                    exportIndexID))
                .ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, 0, returnedSize));
            _objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize - returnedSize,
                    exportIndexID + returnedSize))
                .ReturnsAsync(GetRelativityObjectSlimArrayPart(expectedResult, returnedSize, returnedSize));
            _objectManagerFacadeMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    resultsBlockSize - 2 * returnedSize,
                    exportIndexID + 2 * returnedSize))
                .ReturnsAsync(GetRelativityObjectSlimArrayPart(
                    expectedResult, 
                    2 * returnedSize,
                    resultsBlockSize - 2 * returnedSize));

            // act
            RelativityObjectSlim[] actualResult = await _sut.RetrieveResultsBlockFromExportAsync(
                    runID,
                    resultsBlockSize,
                    exportIndexID,
                    ExecutionIdentity.System)
                .ConfigureAwait(false);

            // assert
            _objectManagerFacadeMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(
                    _WORKSPACE_ARTIFACT_ID,
                    runID,
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Exactly(expectedCallsCount));
            actualResult.ShouldBeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        [Test]
        public void RetrieveResultsBlockFromExportAsync_ItShouldRethrowIntegrationPointException()
        {
            Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
            const int resultsBlockSize = 6;
            const int exportIndexID = 0;
            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ARTIFACT_ID,
                        runID,
                        resultsBlockSize,
                        exportIndexID))
                .Throws<IntegrationPointsException>();

            Func<Task> action = () =>
                _sut.RetrieveResultsBlockFromExportAsync(
                        runID,
                        resultsBlockSize,
                        exportIndexID,
                        ExecutionIdentity.System);

            action.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void RetrieveResultsBlockFromExportAsync_ItShouldThrowExceptionWrappedInIntegrationPointException()
        {
            Guid runID = Guid.Parse("885B7099-963B-493F-895D-6692A6340B5E");
            const int resultsBlockSize = 6;
            const int exportIndexID = 0;
            _objectManagerFacadeMock
                .Setup(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _WORKSPACE_ARTIFACT_ID,
                        runID,
                        resultsBlockSize,
                        exportIndexID))
                .Throws<Exception>();

            Func<Task> action = () =>
                _sut.RetrieveResultsBlockFromExportAsync(
                        runID,
                        resultsBlockSize,
                        exportIndexID,
                        ExecutionIdentity.System);

            action.ShouldThrow<IntegrationPointsException>();
        }


        [TestCase(true)]
        [TestCase(false)]
        public async Task UpdateAsync_ShouldReturnValueFromObjectManagerFacade(bool isSuccess)
        {
            // arrange
            var updateResult = new UpdateResult
            {
                EventHandlerStatuses = new List<EventHandlerStatus>
                {
                    new EventHandlerStatus
                    {
                        Success = isSuccess
                    }
                }
            };
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .ReturnsAsync(updateResult);

            // act
            bool actualResult = await _sut.UpdateAsync(
                    _FIELD_ARTIFACT_ID,
                    It.IsAny<IList<FieldRefValuePair>>(),
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(isSuccess);
        }

        [Test]
        public void UpdateAsync_ShouldRethrowIntegrationPointException()
        {
            // arrange
            var expectedException = new IntegrationPointsException();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .Throws(expectedException);

            // act
            Func<Task> updateAction = () =>
                _sut.UpdateAsync(
                    _FIELD_ARTIFACT_ID,
                    It.IsAny<IList<FieldRefValuePair>>(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            updateAction.ShouldThrow<IntegrationPointsException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void UpdateAsync_ShouldWrapExceptionInIntegrationPointException()
        {
            // arrange
            var expectedInnerException = new Exception();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .Throws(expectedInnerException);

            // act
            Func<Task> massUpdateAction = () =>
                _sut.UpdateAsync(
                    _FIELD_ARTIFACT_ID,
                    It.IsAny<IList<FieldRefValuePair>>(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            massUpdateAction.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().Be(expectedInnerException);
        }

        [Test]
        public async Task UpdateAsync_ShouldSendProperRequest()
        {
            // arrange
            var updateResult = new UpdateResult
            {
                EventHandlerStatuses = new List<EventHandlerStatus>
                {
                    new EventHandlerStatus
                    {
                        Success = true
                    }
                }
            };

            FieldRefValuePair[] fields =
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 1
                    },
                    Value = "one"
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 2
                    },
                    Value = "two"
                }
            };

            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .ReturnsAsync(updateResult);

            // act
            await _sut.UpdateAsync(
                    _FIELD_ARTIFACT_ID,
                    fields,
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            Func<UpdateRequest, bool> requestVerifier = request =>
            {
                bool isValid = true;
                isValid &= request.FieldValues.SequenceEqual(fields);
                return isValid;
            };

            _objectManagerFacadeMock.Verify(x => x.UpdateAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<UpdateRequest>(request => requestVerifier(request)))
            );
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task UpdateAsync_T_ShouldReturnValueFromObjectManagerFacade(bool isSuccess)
        {
            // arrange
            var updateResult = new UpdateResult
            {
                EventHandlerStatuses = new List<EventHandlerStatus>
                {
                    new EventHandlerStatus
                    {
                        Success = isSuccess
                    }
                }
            };
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .ReturnsAsync(updateResult);

            // act
            bool actualResult = await _sut.UpdateAsync(
                    new JobHistory(),
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(isSuccess);
        }

        [Test]
        public void UpdateAsync_T_ShouldRethrowIntegrationPointException()
        {
            // arrange
            var expectedException = new IntegrationPointsException();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .Throws(expectedException);

            // act
            Func<Task> updateAction = () =>
                _sut.UpdateAsync(
                    new JobHistory(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            updateAction.ShouldThrow<IntegrationPointsException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void UpdateAsync_T_ShouldWrapExceptionInIntegrationPointException()
        {
            // arrange
            var expectedInnerException = new Exception();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .Throws(expectedInnerException);

            // act
            Func<Task> updateAction = () =>
                _sut.UpdateAsync(
                    new JobHistory(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            updateAction.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().Be(expectedInnerException);
        }

        [Test]
        public async Task UpdateAsync_T_ShouldSendProperRequest()
        {
            // arrange
            var updateResult = new UpdateResult
            {
                EventHandlerStatuses = new List<EventHandlerStatus>
                {
                    new EventHandlerStatus
                    {
                        Success = true
                    }
                }
            };

            JobHistory jobHistory = new JobHistory
            {
                ArtifactId = 1234,
                JobID = "JobID",
                BatchInstance = "BatchInstance",
                DestinationInstance = "DestinationInstance"
            };

            _objectManagerFacadeMock
                .Setup(x =>
                    x.UpdateAsync(
                        It.IsAny<int>(),
                        It.IsAny<UpdateRequest>()))
                .ReturnsAsync(updateResult);

            // act
            await _sut.UpdateAsync(
                    jobHistory,
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            Func<UpdateRequest, bool> requestVerifier = request =>
            {
                bool isValid = true;
                isValid &= request.Object.ArtifactID == jobHistory.ArtifactId;
                isValid &= request.FieldValues.SingleOrDefault(x => x.Field.Name == "Batch Instance").Value.ToString() == jobHistory.BatchInstance;
                isValid &= request.FieldValues.SingleOrDefault(x => x.Field.Name == "Destination Instance").Value.ToString() == jobHistory.DestinationInstance;
                isValid &= request.FieldValues.SingleOrDefault(x => x.Field.Name == "Job ID").Value.ToString() == jobHistory.JobID;
                return isValid;
            };

            _objectManagerFacadeMock.Verify(x => x.UpdateAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<UpdateRequest>(request => requestVerifier(request)))
            );
        }

        public async Task CreateAsync_ShouldReturnValueFromObjectManagerFacade()
        {
            // arrange
            CreateResult createResult = new CreateResult
            {
                Object = new RelativityObject
                {
                    ArtifactID = 1234
                }
            };
            _objectManagerFacadeMock
                .Setup(x =>
                    x.CreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<CreateRequest>()))
                .ReturnsAsync(createResult);

            // act
            int actualResult = await _sut.CreateAsync(
                    new ObjectTypeRef(),
                    It.IsAny<List<FieldRefValuePair>>(),
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            actualResult.Should().Be(createResult.Object.ArtifactID);
        }

        [Test]
        public void CreateAsync_ShouldRethrowIntegrationPointException()
        {
            // arrange
            var expectedException = new IntegrationPointsException();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.CreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<CreateRequest>()))
                .Throws(expectedException);

            // act
            Func<Task> createAction = () =>
                _sut.CreateAsync(
                    new ObjectTypeRef(),
                    It.IsAny<List<FieldRefValuePair>>(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            createAction.ShouldThrow<IntegrationPointsException>()
                .Which.Should().Be(expectedException);
        }

        [Test]
        public void CreateAsync_ShouldWrapExceptionInIntegrationPointException()
        {
            // arrange
            var expectedInnerException = new Exception();
            _objectManagerFacadeMock
                .Setup(x =>
                    x.CreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<CreateRequest>()))
                .Throws(expectedInnerException);

            // act
            Func<Task> createAction = () =>
                _sut.CreateAsync(
                    new ObjectTypeRef(),
                    It.IsAny<List<FieldRefValuePair>>(),
                    It.IsAny<ExecutionIdentity>());

            // assert
            createAction.ShouldThrow<IntegrationPointsException>()
                .Which.InnerException.Should().Be(expectedInnerException);
        }

        [Test]
        public async Task CreateAsync_ShouldSendProperRequest()
        {
            // arrange
            const int artifactId = 1234;
            CreateResult createResult = new CreateResult
            {
                Object = new RelativityObject
                {
                    ArtifactID = artifactId
                }
            };

            ObjectTypeRef objectTypeRef = new ObjectTypeRef
            {
                ArtifactID = artifactId,
                ArtifactTypeID = artifactId + 1,
                Guid = Guid.NewGuid(),
                Name = "Adler Sieben"
            };

            List<FieldRefValuePair> fields = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 1
                    },
                    Value = "one"
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 2
                    },
                    Value = "two"
                }
            };

            _objectManagerFacadeMock
                .Setup(x =>
                    x.CreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<CreateRequest>()))
                .ReturnsAsync(createResult);

            // act
            await _sut.CreateAsync(
                    objectTypeRef,
                    fields,
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            Func<CreateRequest, bool> requestVerifier = request =>
            {
                bool isValid = request.FieldValues.SequenceEqual(fields);
                isValid &= request.ObjectType.ArtifactID == objectTypeRef.ArtifactID;
                isValid &= request.ObjectType.ArtifactTypeID == objectTypeRef.ArtifactTypeID;
                isValid &= request.ObjectType.Guid == objectTypeRef.Guid;
                isValid &= request.ObjectType.Name == objectTypeRef.Name;
                return isValid;
            };

            _objectManagerFacadeMock.Verify(x => x.CreateAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<CreateRequest>(request => requestVerifier(request)))
            );
        }

        [Test]
        public async Task CreateAsync_WithSpecifiedParent_ShouldSendProperRequest()
        {
            // arrange
            const int artifactId = 1234;
            CreateResult createResult = new CreateResult
            {
                Object = new RelativityObject
                {
                    ArtifactID = artifactId
                }
            };

            ObjectTypeRef objectTypeRef = new ObjectTypeRef
            {
                ArtifactID = artifactId,
                ArtifactTypeID = artifactId + 1,
                Guid = Guid.NewGuid(),
                Name = "Adler Sieben"
            };

            RelativityObjectRef parentObject = new RelativityObjectRef
            {
                ArtifactID = artifactId + 2,
                Guid = Guid.NewGuid()
            };

            List<FieldRefValuePair> fields = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 1
                    },
                    Value = "one"
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef
                    {
                        ArtifactID = 2
                    },
                    Value = "two"
                }
            };

            _objectManagerFacadeMock
                .Setup(x =>
                    x.CreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<CreateRequest>()))
                .ReturnsAsync(createResult);

            // act
            await _sut.CreateAsync(
                    objectTypeRef,
                    parentObject,
                    fields,
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            Func<CreateRequest, bool> requestVerifier = request =>
            {
                bool isValid = request.FieldValues.SequenceEqual(fields);
                isValid &= request.ObjectType.ArtifactID == objectTypeRef.ArtifactID;
                isValid &= request.ObjectType.ArtifactTypeID == objectTypeRef.ArtifactTypeID;
                isValid &= request.ObjectType.Guid == objectTypeRef.Guid;
                isValid &= request.ObjectType.Name == objectTypeRef.Name;
                isValid &= request.ParentObject.ArtifactID == parentObject.ArtifactID;
                isValid &= request.ParentObject.Guid == parentObject.Guid;
                return isValid;
            };

            _objectManagerFacadeMock.Verify(x => x.CreateAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<CreateRequest>(request => requestVerifier(request)))
            );
        }

        [Test]
        public async Task CreateAsync_T_ShouldSendProperRequest()
        {
            // arrange
            const int artifactId = 1234;
            CreateResult createResult = new CreateResult
            {
                Object = new RelativityObject
                {
                    ArtifactID = artifactId
                }
            };

            JobHistory jobHistory = new JobHistory
            {
                ArtifactId = 1234,
                JobID = "JobID",
                BatchInstance = "BatchInstance",
                DestinationInstance = "DestinationInstance"
            };

            _objectManagerFacadeMock
                .Setup(x =>
                    x.CreateAsync(
                        It.IsAny<int>(),
                        It.IsAny<CreateRequest>()))
                .ReturnsAsync(createResult);

            // act
            await _sut.CreateAsync(
                    jobHistory,
                    It.IsAny<ExecutionIdentity>())
                .ConfigureAwait(false);

            // assert
            Func<CreateRequest, bool> requestVerifier = request =>
            {
                bool isValid = request.ObjectType.Guid == ObjectTypeGuids.JobHistoryGuid;
                isValid &= request.FieldValues.SingleOrDefault(x => x.Field.Name == "Batch Instance").Value.ToString() == jobHistory.BatchInstance;
                isValid &= request.FieldValues.SingleOrDefault(x => x.Field.Name == "Destination Instance").Value.ToString() == jobHistory.DestinationInstance;
                isValid &= request.FieldValues.SingleOrDefault(x => x.Field.Name == "Job ID").Value.ToString() == jobHistory.JobID;
                return isValid;
            };

            _objectManagerFacadeMock.Verify(x => x.CreateAsync(
                _WORKSPACE_ARTIFACT_ID,
                It.Is<CreateRequest>(request => requestVerifier(request)))
            );
        }

        private static RelativityObjectSlim[] CreateTestRelativityObjectsSlim(int size)
        {
            var objects = new RelativityObjectSlim[size];
            int iterator = 1;
            for (int i = 0; i < size; ++i)
            {
                int artifactID = ++iterator;
                var values = new List<object> { ++iterator, ++iterator, ++iterator, ++iterator };
                var objectSlim = new RelativityObjectSlim
                {
                    ArtifactID = artifactID,
                    Values = values
                };
                objects[i] = objectSlim;
            }
            return objects;
        }

        private static RelativityObjectSlim[] GetRelativityObjectSlimArrayPart(RelativityObjectSlim[] originalArray, int start, int length)
        {
            var result = new RelativityObjectSlim[length];
            Array.Copy(originalArray, start, result, 0, length);
            return result;
        }
    }
}
