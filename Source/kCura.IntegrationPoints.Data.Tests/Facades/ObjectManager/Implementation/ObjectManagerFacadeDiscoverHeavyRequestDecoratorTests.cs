using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.ObjectManager;
using kCura.IntegrationPoints.Data.Facades.ObjectManager.Implementation;
using kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation.TestCases;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;
using ReadResult = Relativity.Services.Objects.DataContracts.ReadResult;

namespace kCura.IntegrationPoints.Data.Tests.Facades.ObjectManager.Implementation
{
    [TestFixture]
    [Category("Unit")]
    public class ObjectManagerFacadeDiscoverHeavyRequestDecoratorTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<IObjectManagerFacade> _objectManagerMock;
        private ObjectManagerFacadeDiscoverHeavyRequestDecorator _sut;

        private static IEnumerable<TestCaseData> FieldValueExceededAndNotTestSource = ConvertToNamedTestCase(new[]
        {
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
                            .ToArray(),
                        name: "Array Field Exceeded"
                    ),
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
                            .ToArray(),
                        name: "Array Field Not Exceeded"
                    )
                }
            },
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
                            .ToList(),
                        name: "List Field Exceeded"
                    ),
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
                            .ToArray(),
                        name: "List Field Not Exceeded"
                    )
                }
            }
        });

        private static IEnumerable<TestCaseData> FieldValueExceededByOneTestSource = ConvertToNamedTestCase(new[]
        {
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
                            .ToArray(),
                        name: "Array Field"
                    )
                }
            },
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT + 1)
                            .ToList(),
                        name: "List Field"
                    )
                }
            }
        });

        private static IEnumerable<TestCaseData> FieldValueEqualToMaxTestSource = ConvertToNamedTestCase(new[]
        {
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
                            .ToArray(),
                        name: "Array Field"
                    )
                }
            },
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT)
                            .ToList(),
                        name: "List Field"
                    )
                }
            }
        });

        private static IEnumerable<TestCaseData> FieldValueLowerThanMaxTestSource = ConvertToNamedTestCase(new[]
        {
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT - 1)
                            .ToArray(),
                        name: "Array Field"
                    )
                }
            },
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(1, _MAX_COLLECTION_COUNT - 1)
                            .ToList(),
                        name: "List Field"
                    )
                }
            }
        });

        private static IEnumerable<TestCaseData> FieldValueAreEmptyTestSource = ConvertToNamedTestCase(new[]
        {
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(0, 0).ToArray(),
                        name: "Array Field"
                    )
                }
            },
            new CollectionFieldValueTestCases {
                FieldValues = new [] {
                    new CollectionFieldValueTestCase(
                        value: Enumerable.Range(0, 0).ToList(),
                        name: "List Field"
                    )
                }
            }
        });

        private static IEnumerable<TestCaseData> FieldValueAreNullsTestSource = ConvertToNamedTestCase(new[]
        {
            new CollectionFieldValueTestCases
            {
                FieldValues = new []
                {
                    new CollectionFieldValueTestCase(
                        value: null,
                        name: "Null Field"
                    )
                }
            },
        });

        private const int _MAX_COLLECTION_COUNT = 100000;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManagerFacade>();
            _loggerMock = new Mock<IAPILog>();
            _loggerMock.Setup(x => x.ForContext<ObjectManagerFacadeDiscoverHeavyRequestDecorator>())
                .Returns(_loggerMock.Object);

            _sut = new ObjectManagerFacadeDiscoverHeavyRequestDecorator(
                _objectManagerMock.Object,
                _loggerMock.Object);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
        [TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
        public async Task CreateAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "Object Is Exceeded Or Not";
            const string operation = "CREATE";
            const string objectId = "[UNKNOWN]";
            IEnumerable<CollectionFieldValueTestCase> exceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
            IEnumerable<CollectionFieldValueTestCase> notExceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

            CreateRequest request = BuildCreateRequest(
                fieldValueTestCases.FieldValues,
                objectTypeName);

            CreateResult result = new CreateResult();
            SetupCreateAsyncInObjectManager(result);

            //act
            CreateResult actualResult = await _sut
                .CreateAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfHeavyRequestIsLoggedIn(
                operation,
                objectTypeName,
                objectId,
                workspaceId,
                exceededFieldValues);
            VerifyIfHeavyRequestIsNotLoggedIn(operation, notExceededFieldValues);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueAreNullsTestSource))]
        [TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
        [TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
        [TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
        public async Task CreateAsync_ShouldNotLogWarningForNonHeavyRequest(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "Object Is Null";
            CreateRequest request = BuildCreateRequest(
                fieldValueTestCases.FieldValues,
                objectTypeName);

            CreateResult expectedResult = new CreateResult();
            SetupCreateAsyncInObjectManager(expectedResult);

            //act
            CreateResult actualResult = await _sut
                .CreateAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            actualResult.Should().Be(expectedResult);
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        [TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
        [TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
        public async Task ReadAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "[UNKNOWN]";
            const string operation = "READ";
            const int objectId = 33;

            IEnumerable<CollectionFieldValueTestCase> exceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
            IEnumerable<CollectionFieldValueTestCase> notExceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

            ReadRequest request = BuildReadRequest(objectId);

            ReadResult result = BuildReadResult(fieldValueTestCases.FieldValues);
            SetupReadAsyncInObjectManager(result);

            //act
            ReadResult actualResult = await _sut
                .ReadAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfHeavyRequestIsLoggedIn(
                operation,
                objectTypeName,
                objectId.ToString(),
                workspaceId,
                exceededFieldValues);
            VerifyIfHeavyRequestIsNotLoggedIn(operation, notExceededFieldValues);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueAreNullsTestSource))]
        [TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
        [TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
        [TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
        public async Task ReadAsync_ShouldNotLogWarningForNonHeavyRequest(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const int objectId = 33;
            ReadRequest request = BuildReadRequest(objectId);

            ReadResult expectedResult = BuildReadResult(fieldValueTestCases.FieldValues);
            SetupReadAsyncInObjectManager(expectedResult);

            //act
            ReadResult actualResult = await _sut
                .ReadAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            actualResult.Should().Be(expectedResult);
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        [TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
        [TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
        public async Task UpdateAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "[UNKNOWN]";
            const string operation = "UPDATE";
            const int objectId = 33;

            IEnumerable<CollectionFieldValueTestCase> exceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
            IEnumerable<CollectionFieldValueTestCase> notExceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

            UpdateRequest request = BuildUpdateRequest(
                fieldValueTestCases.FieldValues,
                objectId);

            UpdateResult result = new UpdateResult();
            SetupUpdateAsyncInObjectManager(result);

            //act
            UpdateResult actualResult = await _sut
                .UpdateAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfHeavyRequestIsLoggedIn(
                operation,
                objectTypeName,
                objectId.ToString(),
                workspaceId,
                exceededFieldValues);
            VerifyIfHeavyRequestIsNotLoggedIn(operation,notExceededFieldValues);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueAreNullsTestSource))]
        [TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
        [TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
        [TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
        public async Task UpdateAsync_ShouldNotLogWarningForNonHeavyRequest(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const int objectId = 33;

            UpdateRequest request = BuildUpdateRequest(
                fieldValueTestCases.FieldValues,
                objectId);

            UpdateResult result = new UpdateResult();
            SetupUpdateAsyncInObjectManager(result);

            //act
            UpdateResult actualResult = await _sut
                .UpdateAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        [TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
        [TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
        public async Task QueryAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "Object Exceeded";
            const string operation = "QUERY";
            const string objectId = "[UNKNOWN]";
            const int start = 0;
            const int length = 1;

            IEnumerable<CollectionFieldValueTestCase> exceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
            IEnumerable<CollectionFieldValueTestCase> notExceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

            QueryRequest request = BuildQueryRequest(objectTypeName);

            QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
            SetupQueryAsyncInObjectManager(result);

            //act
            QueryResult actualResult = await _sut
                .QueryAsync(workspaceId, request, start, length)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfHeavyRequestIsLoggedIn(
                operation,
                objectTypeName,
                objectId,
                workspaceId,
                exceededFieldValues);
            VerifyIfHeavyRequestIsNotLoggedIn(operation, notExceededFieldValues);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueAreNullsTestSource))]
        [TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
        [TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
        [TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
        public async Task QueryAsync_ShouldNotLogWarningForNonHeavyRequest(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "Object Not Exceeded";
            const int start = 0;
            const int length = 1;

            QueryRequest request = BuildQueryRequest(objectTypeName);

            QueryResult result = BuildQueryResult(fieldValueTestCases.FieldValues);
            SetupQueryAsyncInObjectManager(result);

            //act
            QueryResult actualResult = await _sut
                .QueryAsync(workspaceId, request, start, length)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        [TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
        [TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
        public async Task QuerySlimAsync_ShouldLogWarningOnlyWhenFieldCollectionCountIsExceeded(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "Object Exceeded";
            const string operation = "QUERY SLIM";
            const string objectId = "[UNKNOWN]";
            const int start = 0;
            const int length = 1;

            IEnumerable<CollectionFieldValueTestCase> exceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
            IEnumerable<CollectionFieldValueTestCase> notExceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

            QueryRequest request = BuildQueryRequest(objectTypeName);

            QueryResultSlim result = BuildQueryResultSlim(fieldValueTestCases.FieldValues);
            SetupQuerySlimAsyncInObjectManager(result);

            //act
            QueryResultSlim actualResult = await _sut
                .QuerySlimAsync(workspaceId, request, start, length)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfHeavyRequestIsLoggedIn(
                operation,
                objectTypeName,
                objectId,
                workspaceId,
                exceededFieldValues);
            VerifyIfHeavyRequestIsNotLoggedIn(operation, notExceededFieldValues);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueAreNullsTestSource))]
        [TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
        [TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
        [TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
        public async Task QuerySlimAsync_ShouldNotLogWarningForNonHeavyRequest(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "Object Not Exceeded";
            const int start = 0;
            const int length = 1;

            QueryRequest request = BuildQueryRequest(objectTypeName);

            QueryResultSlim result = BuildQueryResultSlim(fieldValueTestCases.FieldValues);
            SetupQuerySlimAsyncInObjectManager(result);

            //act
            QueryResultSlim actualResult = await _sut
                .QuerySlimAsync(workspaceId, request, start, length)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        public async Task UpdateAsync_MassUpdate_ShouldReturnDecoratedResult()
        {
            // arrange
            const int workspaceId = 101;
            var updateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };
            var request = new MassUpdateByObjectIdentifiersRequest
            {
                FieldValues = Enumerable.Empty<FieldRefValuePair>(),
                Objects = new List<RelativityObjectRef>
                {
                    new RelativityObjectRef()
                }
            };

            var expectedMassUpdateResult = new MassUpdateResult();
            _objectManagerMock.Setup(x => x.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
                    It.IsAny<MassUpdateOptions>()))
                .ReturnsAsync(expectedMassUpdateResult);

            // act
            MassUpdateResult result = await _sut
                .UpdateAsync(workspaceId, request, updateOptions)
                .ConfigureAwait(false);

            // assert
            result.Should().Be(expectedMassUpdateResult);
        }

        [Test]
        [TestCaseSource(nameof(FieldValueAreNullsTestSource))]
        [TestCaseSource(nameof(FieldValueAreEmptyTestSource))]
        [TestCaseSource(nameof(FieldValueLowerThanMaxTestSource))]
        [TestCaseSource(nameof(FieldValueEqualToMaxTestSource))]
        public async Task UpdateAsync_MassUpdate_ShouldNotLogWarningForNonHeavyRequest(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            // arrange
            const int workspaceId = 101;

            var updateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };

            var request = new MassUpdateByObjectIdentifiersRequest
            {
                FieldValues = ConvertTestCaseToFieldRefValuePair(fieldValueTestCases.FieldValues),
                Objects = new List<RelativityObjectRef>
                {
                    new RelativityObjectRef()
                }
            };

            // act
            await _sut
                .UpdateAsync(workspaceId, request, updateOptions)
                .ConfigureAwait(false);

            // assert
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        [TestCaseSource(nameof(FieldValueExceededByOneTestSource))]
        [TestCaseSource(nameof(FieldValueExceededAndNotTestSource))]
        public async Task UpdateAsync_MassUpdate_ShouldLogWarningWhenFieldCollectionCountIsExceeded(
            CollectionFieldValueTestCases fieldValueTestCases)
        {
            //arrange
            const int workspaceId = 101;
            const string objectTypeName = "[UNKNOWN]";
            const string operation = "UPDATE";
            const string objectId = "[UNKNOWN]";

            IEnumerable<CollectionFieldValueTestCase> exceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count > _MAX_COLLECTION_COUNT);
            IEnumerable<CollectionFieldValueTestCase> notExceededFieldValues = fieldValueTestCases
                .FieldValues
                .Where(x => x.Value.Count <= _MAX_COLLECTION_COUNT);

            var updateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };

            var request = new MassUpdateByObjectIdentifiersRequest
            {
                FieldValues = ConvertTestCaseToFieldRefValuePair(fieldValueTestCases.FieldValues),
                Objects = new List<RelativityObjectRef>
                {
                    new RelativityObjectRef()
                }
            };

            //act
            await _sut
                .UpdateAsync(workspaceId, request, updateOptions)
                .ConfigureAwait(false);

            //assert
            VerifyIfHeavyRequestIsLoggedIn(
                operation,
                objectTypeName,
                objectId,
                workspaceId,
                exceededFieldValues);
            VerifyIfHeavyRequestIsNotLoggedIn(operation, notExceededFieldValues);
        }

        [Test]
        public async Task UpdateAsync_MassUpdate_ShouldLogWarningWhenObjectsCollectionCountIsExceeded()
        {
            //arrange
            const int workspaceId = 101;
            const int objectIdsCount = _MAX_COLLECTION_COUNT + 1;

            var updateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };

            var request = new MassUpdateByObjectIdentifiersRequest
            {
                FieldValues = Enumerable.Empty<FieldRefValuePair>(),
                Objects = Enumerable.Range(0, objectIdsCount)
                    .Select(x => new RelativityObjectRef { ArtifactID = x })
                    .ToList()
            };

            //act
            await _sut
                .UpdateAsync(workspaceId, request, updateOptions)
                .ConfigureAwait(false);

            //assert
            string expectedStackTracePart = $"ObjectManagerFacadeDiscoverHeavyRequestDecorator.UpdateAsync";
            string expectedWarningMessage =
                "Requested mass update operation exceeded max collection count - {objectsCount}, when allowed is {maxCollectionCount}";
            var expectedParams = new object[] { objectIdsCount, _MAX_COLLECTION_COUNT };
            _loggerMock.Verify(x => x.LogWarning(
                It.Is((Exception ex) => ex.StackTrace.Contains(expectedStackTracePart)),
                expectedWarningMessage, expectedParams));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(_MAX_COLLECTION_COUNT)]
        public async Task UpdateAsync_MassUpdate_ShouldNotLogWarningWhenObjectsCollectionCountIsExceeded(int objectIdsCount)
        {
            //arrange
            const int workspaceId = 101;

            var updateOptions = new MassUpdateOptions
            {
                UpdateBehavior = FieldUpdateBehavior.Merge
            };

            var request = new MassUpdateByObjectIdentifiersRequest
            {
                FieldValues = Enumerable.Empty<FieldRefValuePair>(),
                Objects = Enumerable.Range(0, objectIdsCount)
                    .Select(x => new RelativityObjectRef { ArtifactID = x })
                    .ToList()
            };

            //act
            await _sut
                .UpdateAsync(workspaceId, request, updateOptions)
                .ConfigureAwait(false);

            //assert
            string expectedWarningMessage = $"Requested mass update operation exceeded max collection count - {objectIdsCount}, when allowed is {_MAX_COLLECTION_COUNT}";
            _loggerMock.Verify(x => x.LogWarning(expectedWarningMessage), Times.Never);
        }

        [Test]
        public async Task DeleteAsync_ShouldNotLogAnyWarning()
        {
            //arrange
            const int workspaceId = 101;

            DeleteRequest request = new DeleteRequest();
            DeleteResult result = new DeleteResult();
            SetupDeleteAsyncInObjectManager(result);

            //act
            DeleteResult actualResult = await _sut
                .DeleteAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            result.Should().Be(actualResult);
            VerifyIfNoneHeavyRequestIsNotLoggedIn();
        }

        [Test]
        public void This_ShouldDisposeObjectManagerFacade()
        {
            //act
            _sut.Dispose();

            //assert
            _objectManagerMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void This_ShouldDisposeObjectManagerFacadeOnlyOnce()
        {
            //act
            _sut.Dispose();
            _sut.Dispose();

            //assert
            _objectManagerMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public async Task InitializeExportAsync_ShouldReturnSameResultAsInnerFacade()
        {
            //arrange
            const int workspaceID = 101;
            var queryRequest = new QueryRequest();
            const int start = 5;
            ExportInitializationResults expectedResult = new ExportInitializationResults();

            _objectManagerMock.Setup(x => x.InitializeExportAsync(
                    workspaceID,
                    queryRequest,
                    start))
                .ReturnsAsync(expectedResult);

            //act
            ExportInitializationResults actualResult = await _sut
                .InitializeExportAsync(workspaceID, queryRequest, start)
                .ConfigureAwait(false);

            //assert
            _objectManagerMock.Verify(x => x.InitializeExportAsync(workspaceID, queryRequest, start), Times.Once);
            actualResult.Should().Be(expectedResult);
        }

        [Test]
        public async Task RetrieveResultsBlockFromExportAsync_ShouldReturnSameResultAsInnerFacade()
        {
            //arrange
            const int workspaceID = 101;
            Guid runID = Guid.Parse("EA150180-3A58-4DFF-AA6C-6385075FCFD3");
            const int resultsBlockSize = 5;
            const int exportIndexID = 0;
            RelativityObjectSlim relativityObjectSlim = new RelativityObjectSlim();
            RelativityObjectSlim[] expectedResult = { relativityObjectSlim };

            _objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                    workspaceID,
                    runID,
                    resultsBlockSize,
                    exportIndexID))
                .ReturnsAsync(expectedResult);

            //act
            RelativityObjectSlim[] actualResult = await _sut
                .RetrieveResultsBlockFromExportAsync(workspaceID, runID, resultsBlockSize, exportIndexID)
                .ConfigureAwait(false);

            //assert
            _objectManagerMock.Verify(x => x.RetrieveResultsBlockFromExportAsync(workspaceID, runID, resultsBlockSize, exportIndexID), Times.Once);
            actualResult.Should().BeSameAs(expectedResult);
        }

        [Test]
        public async Task DeleteAsync_MassDelete_ShouldReturnDecoratedResult()
        {
            // arrange
            const int workspaceId = 101;

            var request = new MassDeleteByObjectIdentifiersRequest()
            {
                Objects = new List<RelativityObjectRef>
                {
                    new RelativityObjectRef()
                }
            };

            var massDeleteResult = new MassDeleteResult();
            _objectManagerMock.Setup(x => x.DeleteAsync(
                    It.IsAny<int>(),
                    It.IsAny<MassDeleteByObjectIdentifiersRequest>()
                    ))
                .ReturnsAsync(massDeleteResult);

            // act
            MassDeleteResult result = await _sut
                .DeleteAsync(workspaceId, request)
                .ConfigureAwait(false);

            // assert
            result.Should().Be(massDeleteResult);
        }

        [Test]
        public async Task DeleteAsync_MassDelete_ShouldLogWarningWhenObjectsCollectionCountIsExceeded()
        {
            //arrange
            const int workspaceId = 101;
            const int objectIdsCount = _MAX_COLLECTION_COUNT + 1;

            var request = new MassDeleteByObjectIdentifiersRequest()
            {
                Objects = Enumerable.Range(0, objectIdsCount)
                    .Select(x => new RelativityObjectRef { ArtifactID = x })
                    .ToList()
            };

            //act
            await _sut
                .DeleteAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            string expectedStackTracePart = $"ObjectManagerFacadeDiscoverHeavyRequestDecorator.DeleteAsync";
            string expectedWarningMessage =
                "Requested mass delete operation exceeded max collection count - {objectsCount}, when allowed is {maxCollectionCount}";
            var expectedParams = new object[] { objectIdsCount, _MAX_COLLECTION_COUNT };
            _loggerMock.Verify(x => x.LogWarning(
                It.Is((Exception ex) => ex.StackTrace.Contains(expectedStackTracePart)),
                expectedWarningMessage, expectedParams));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(_MAX_COLLECTION_COUNT)]
        public async Task DeleteAsync_MassDelete_ShouldNotLogWarningWhenObjectsCollectionCountIsExceeded(int objectIdsCount)
        {
            //arrange
            const int workspaceId = 101;

            var request = new MassDeleteByObjectIdentifiersRequest()
            {
                Objects = Enumerable.Range(0, objectIdsCount)
                    .Select(x => new RelativityObjectRef { ArtifactID = x })
                    .ToList()
            };

            //act
            await _sut
                .DeleteAsync(workspaceId, request)
                .ConfigureAwait(false);

            //assert
            string expectedWarningMessage = $"Requested mass delete operation exceeded max collection count - {objectIdsCount}, when allowed is {_MAX_COLLECTION_COUNT}";
            _loggerMock.Verify(x => x.LogWarning(expectedWarningMessage), Times.Never);
        }

        private CreateRequest BuildCreateRequest(
            IEnumerable<CollectionFieldValueTestCase> fieldValueTestCases,
            string objectTypeName)
        {
            return new CreateRequest
            {
                FieldValues = ConvertTestCaseToFieldRefValuePair(fieldValueTestCases),
                ObjectType = new ObjectTypeRef { Name = objectTypeName }
            };
        }

        private UpdateRequest BuildUpdateRequest(
            IEnumerable<CollectionFieldValueTestCase> fieldValueTestCases,
            int objectId)
        {
            return new UpdateRequest
            {
                FieldValues = ConvertTestCaseToFieldRefValuePair(fieldValueTestCases),
                Object = new RelativityObjectRef { ArtifactID = objectId }
            };
        }

        private ReadRequest BuildReadRequest(int objectId)
        {
            return new ReadRequest
            {
                Object = new RelativityObjectRef
                {
                    ArtifactID = objectId
                }
            };
        }

        private ReadResult BuildReadResult(IEnumerable<CollectionFieldValueTestCase> fieldValueTestCases)
        {
            return new ReadResult
            {
                Object = new RelativityObject
                {
                    FieldValues = ConvertTestCaseToFieldValuePair(fieldValueTestCases).ToList()
                }
            };
        }

        private QueryRequest BuildQueryRequest(string objectTypeName)
        {
            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Name = objectTypeName
                }
            };
        }

        private QueryResult BuildQueryResult(IEnumerable<CollectionFieldValueTestCase> fieldValueTestCases)
        {
            return new QueryResult
            {
                Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        FieldValues = ConvertTestCaseToFieldValuePair(fieldValueTestCases).ToList()
                    }
                }
            };
        }

        private QueryResultSlim BuildQueryResultSlim(IEnumerable<CollectionFieldValueTestCase> fieldValueTestCases)
        {
            return new QueryResultSlim
            {
                Objects = fieldValueTestCases.Select(x => new RelativityObjectSlim
                {
                    Values = new List<object> { x.Value }
                }).ToList(),
                Fields = fieldValueTestCases.Select(x => new Field
                {
                    Name = x.Name
                }).ToList()
            };
        }

        private void SetupCreateAsyncInObjectManager(CreateResult result)
        {
            _objectManagerMock.Setup(x => x.CreateAsync(
                    It.IsAny<int>(),
                    It.IsAny<CreateRequest>()))
                .ReturnsAsync(result);
        }

        private void SetupUpdateAsyncInObjectManager(UpdateResult result)
        {
            _objectManagerMock.Setup(x => x.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateRequest>()))
                .ReturnsAsync(result);
        }

        private void SetupReadAsyncInObjectManager(ReadResult result)
        {
            _objectManagerMock.Setup(x => x.ReadAsync(
                    It.IsAny<int>(),
                    It.IsAny<ReadRequest>()))
                .ReturnsAsync(result);
        }

        private void SetupQueryAsyncInObjectManager(QueryResult result)
        {
            _objectManagerMock.Setup(x => x.QueryAsync(
                    It.IsAny<int>(),
                    It.IsAny<QueryRequest>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(result);
        }

        private void SetupQuerySlimAsyncInObjectManager(QueryResultSlim result)
        {
            _objectManagerMock.Setup(x => x.QuerySlimAsync(
                    It.IsAny<int>(),
                    It.IsAny<QueryRequest>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(result);
        }

        private void SetupDeleteAsyncInObjectManager(DeleteResult result)
        {
            _objectManagerMock.Setup(x => x.DeleteAsync(
                    It.IsAny<int>(),
                    It.IsAny<DeleteRequest>()))
                .ReturnsAsync(result);
        }

        private void VerifyIfNoneHeavyRequestIsNotLoggedIn()
        {
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Never);
        }

        private void VerifyIfHeavyRequestIsNotLoggedIn(
            string operation,
            IEnumerable<CollectionFieldValueTestCase> notExceededTestCases)
        {
            string expectedStackTracePart = $"ObjectManagerFacadeDiscoverHeavyRequestDecorator.{CapitilizeFirstLetter(operation)}Async";

            foreach (var notExceededTestCase in notExceededTestCases)
            {
                _loggerMock.Verify(x => x.LogWarning(
                        It.Is((Exception ex) => ex.StackTrace.Contains(expectedStackTracePart)),
                        "Requested field {fieldName} exceeded max collection count - {count}, when allowed is {maxAllowedCount}",
                        notExceededTestCase.Name, notExceededTestCase.Value.Count, _MAX_COLLECTION_COUNT)
                    , Times.Never);
            }
        }

        private void VerifyIfHeavyRequestIsLoggedIn(
            string operation,
            string objectTypeName,
            string objectId,
            int workspaceId,
            IEnumerable<CollectionFieldValueTestCase> exceededTestCases)
        {
            string expectedStackTracePart = $"ObjectManagerFacadeDiscoverHeavyRequestDecorator.{CapitilizeFirstLetter(operation)}Async";

            _loggerMock.Verify(
                x => x.LogWarning(
                    It.Is((Exception ex) => ex.StackTrace.Contains(expectedStackTracePart)),
                    //It.IsAny<Exception>(),
                    "Heavy request discovered when executing {operationName} on object of type [{rdoType}], id {rdoArtifactId} with ObjectManager (Workspace: {workspaceArtifactId}).", 
                    operation.Replace(" ", string.Empty),
                    objectTypeName,
                    objectId,
                    workspaceId),
                Times.AtLeastOnce);
            foreach (var exceededTestCase in exceededTestCases)
            {
                _loggerMock.Verify(
                    x => x.LogWarning(
                        It.Is((Exception ex) => ex.StackTrace.Contains(expectedStackTracePart)),
                        "Requested field {fieldName} exceeded max collection count - {count}, when allowed is {maxAllowedCount}",
                        exceededTestCase.Name,
                        exceededTestCase.Value.Count,
                        _MAX_COLLECTION_COUNT),
                    Times.Once);
            }
        }

        private static IEnumerable<TestCaseData> ConvertToNamedTestCase<T>(
            IEnumerable<T> testCases,
            [CallerMemberName] string testCaseName = "")
        {
            return testCases.Select(
                x => new TestCaseData(x).SetName(testCaseName)
            );
        }

        private IEnumerable<FieldRefValuePair> ConvertTestCaseToFieldRefValuePair(IEnumerable<CollectionFieldValueTestCase> testCase)
        {
            return testCase.Select(x => new FieldRefValuePair
            {
                Value = x.Value,
                Field = new FieldRef { Name = x.Name }
            });
        }

        private IEnumerable<FieldValuePair> ConvertTestCaseToFieldValuePair(IEnumerable<CollectionFieldValueTestCase> testCase)
        {
            return testCase.Select(x => new FieldValuePair
            {
                Value = x.Value,
                Field = new Field { Name = x.Name }
            });
        }

        private static string CapitilizeFirstLetter(string s)
        {
            string[] words = s.Split(' ');
            string capitalizedWord = string.Empty;
            foreach (string word in words)
            {
                capitalizedWord += word[0].ToString().ToUpper() + word.ToLower().Substring(1);
            }

            return capitalizedWord;
        }
    }
}
