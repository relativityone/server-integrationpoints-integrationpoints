using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Tests.Repositories.Implementations.CommonTests;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Field;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;
using FieldRef = Relativity.Services.Objects.DataContracts.FieldRef;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class KeplerDocumentRepositoryTests
    {
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private ExportInitializationResultsDto _initializationResults;
        private KeplerDocumentRepository _sut;
        private MassUpdateTests _massUpdateTests;

        private const int _SEARCH_ARTIFACT_ID = 12321;
        private const int _PRODUCTION_ARTIFACT_ID = 45654;
        private const int _START_AT_RECORD = 1;
        private const string _EXTRACTED_TEXT_FIELD_NAME = "Extracted Text";
        private const string _CONTROL_NUMBER_FIELD_NAME = "Control Number";
        private readonly Guid _runIDGuid = new Guid("8D65B607-31C9-4B50-BB19-D3139873E65D");

        [SetUp]
        public void SetUp()
        {
            string[] fieldNames = {"Control Number", "Email", "Test1", "Test2"};
            _initializationResults = new ExportInitializationResultsDto(_runIDGuid, 0, fieldNames);
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _sut = new KeplerDocumentRepository(_objectManagerMock.Object);

            _massUpdateTests = new MassUpdateTests(_sut, _objectManagerMock);
        }

        [Test]
        public async Task RetrieveDocumentsArtifactIDsAsync_ShouldBuildProperRequest()
        {
            // arrange
            const string documentIdentifierField = "CONTROL NUMBER";
            int[] artifactsIDs = { 5, 9843, 3212 };
            string[] documentsIdentifiers = artifactsIDs.Select(x => x.ToString()).ToArray();
            List<RelativityObject> response = artifactsIDs
                .Select(artifactID => new RelativityObject { ArtifactID = artifactID })
                .ToList();

            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));

            Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
            {
                bool isValid = true;
                isValid &= queryRequest.Condition == @"'CONTROL NUMBER' in ['5','9843','3212']";
                isValid &= queryRequest.Fields.Any(field => field.Name == "Artifact ID");
                isValid &= queryRequest.ObjectType.ArtifactTypeID == (int) ArtifactType.Document;
                return isValid;
            };

            // act
            await _sut.RetrieveDocumentsAsync(documentIdentifierField, documentsIdentifiers).ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(
                x => x.QueryAsync(It.Is<QueryRequest>(query => queryRequestValidator(query)),
                    ExecutionIdentity.CurrentUser));
        }

        [Test]
        public async Task RetrieveDocumentsArtifactIDsAsync_ShouldReturnArtifactIDs()
        {
            // arrange
            int[] artifactsIDs = { 5, 9843, 3212 };
            List<RelativityObject> response = artifactsIDs
                .Select(artifactID => new RelativityObject { ArtifactID = artifactID })
                .ToList();

            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));

            // act
            int[] result = await _sut
                .RetrieveDocumentsAsync(
                    docIdentifierField: string.Empty,
                    docIdentifierValues: new[] { string.Empty })
                .ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(artifactsIDs);
        }

        [Test]
        public void RetrieveDocumentsArtifactIDsAsync_ShouldRethrowObjectManagerException()
        {
            // arrange
            var exceptionToThrow = new Exception();
            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Throws(exceptionToThrow);

            // act
            Func<Task> retrieveAction = () => _sut
                .RetrieveDocumentsAsync(
                    docIdentifierField: string.Empty,
                    docIdentifierValues: new[] { string.Empty });

            // assert
            retrieveAction.ShouldThrow<Exception>()
                .Which.Should().Be(exceptionToThrow);
        }

        [Test]
        public async Task RetrieveDocumentsAsync_ByFieldIDs_ShouldBuildProperRequest()
        {
            // arrange
            int[] artifactsIDs = { 5, 9843, 3212 };
            var fieldIDs = new HashSet<int> { 597, 412 };
            Arrange_RetrieveDocumentsAsync_ShouldBuildProperRequest(artifactsIDs);

            const string condition = @"'ArtifactID' in [5,9843,3212]";
            Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
            {
                bool isValid = true;
                isValid &= queryRequest.Condition == condition;
                foreach (int fieldID in fieldIDs)
                {
                    isValid &= queryRequest.Fields.Any(field => field.ArtifactID == fieldID);
                }

                isValid &= queryRequest.ObjectType.ArtifactTypeID == (int) ArtifactType.Document;
                return isValid;
            };

            // act
            await _sut
                .RetrieveDocumentsAsync(artifactsIDs, fieldIDs)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(
                x => x.QueryAsync(It.Is<QueryRequest>(query => queryRequestValidator(query)),
                    ExecutionIdentity.CurrentUser));
        }

        [Test]
        public async Task RetrieveDocumentsAsync_ByFieldNames_ShouldBuildProperRequest()
        {
            // arrange
            int[] artifactsIDs = { 5, 9843, 3212 };
            var fieldNames = new HashSet<string> { "Control Number", "Sent from" };
            Arrange_RetrieveDocumentsAsync_ShouldBuildProperRequest(artifactsIDs);

            const string condition = @"'ArtifactID' in [5,9843,3212]";
            Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
            {
                bool isValid = true;
                isValid &= queryRequest.Condition == condition;
                foreach (string fieldName in fieldNames)
                {
                    isValid &= queryRequest.Fields.Any(field => field.Name == fieldName);
                }

                isValid &= queryRequest.ObjectType.ArtifactTypeID == (int) ArtifactType.Document;
                return isValid;
            };

            // act
            await _sut
                .RetrieveDocumentsAsync(artifactsIDs, fieldNames)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(
                x => x.QueryAsync(It.Is<QueryRequest>(query => queryRequestValidator(query)),
                    ExecutionIdentity.CurrentUser));
        }

        [Test]
        public async Task RetrieveDocumentsAsync_ByFieldIDs_ShouldReturnArtifactDTOs()
        {
            // arrange
            const int firstFieldArtifactID = 597;
            const int secondFieldArtifactID = 412;
            int[] artifactsIDs = { 5, 9843, 3212 };
            var fieldIDs = new HashSet<int> { firstFieldArtifactID, secondFieldArtifactID };
            List<RelativityObject> response = artifactsIDs
                .Select(
                    artifactID => new RelativityObject
                    {
                        ArtifactID = artifactID,
                        FieldValues = new List<FieldValuePair>
                        {
                            new FieldValuePair
                            {
                                Field = new Field
                                {
                                    ArtifactID = firstFieldArtifactID
                                },
                                Value = string.Empty
                            },
                            new FieldValuePair
                            {
                                Field = new Field
                                {
                                    ArtifactID = secondFieldArtifactID
                                },
                                Value = string.Empty
                            }
                        }
                    })
                .ToList();

            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));

            // act
            ArtifactDTO[] result = await _sut
                .RetrieveDocumentsAsync(artifactsIDs, fieldIDs)
                .ConfigureAwait(false);

            // assert
            result.Select(x => x.ArtifactId).Should()
                .BeEquivalentTo(
                    artifactsIDs,
                    "because all requested documents should be returned");
            foreach (ArtifactDTO artifactDto in result)
            {
                artifactDto.Fields.Select(x => x.ArtifactId).Should()
                    .BeEquivalentTo(
                        fieldIDs,
                        "because for each documents each field should be present");
            }
        }

        [Test]
        public async Task RetrieveDocumentsAsync_ByFieldNames_ShouldReturnArtifactDTOs()
        {
            // arrange
            const string firstFieldName = "Control Number";
            const string secondFieldName = "Sent To";
            int[] artifactsIDs = { 5, 9843, 3212 };
            var fieldNames = new HashSet<string> { firstFieldName, secondFieldName };
            List<RelativityObject> response = artifactsIDs
                .Select(
                    artifactID => new RelativityObject
                    {
                        ArtifactID = artifactID,
                        FieldValues = new List<FieldValuePair>
                        {
                            new FieldValuePair
                            {
                                Field = new Field
                                {
                                    Name = firstFieldName
                                },
                                Value = string.Empty
                            },
                            new FieldValuePair
                            {
                                Field = new Field
                                {
                                    Name = secondFieldName
                                },
                                Value = string.Empty
                            }
                        }
                    })
                .ToList();

            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));

            // act
            ArtifactDTO[] result = await _sut
                .RetrieveDocumentsAsync(artifactsIDs, fieldNames)
                .ConfigureAwait(false);

            // assert
            result.Select(x => x.ArtifactId).Should()
                .BeEquivalentTo(
                    artifactsIDs,
                    "because all requested documents should be returned");
            foreach (ArtifactDTO artifactDto in result)
            {
                artifactDto.Fields.Select(x => x.Name).Should()
                    .BeEquivalentTo(
                        fieldNames,
                        "because for each documents each field should be present");
            }
        }

        [Test]
        public void RetrieveDocumentsAsync_ByFieldIDs_ShouldRethrowObjectManagerException()
        {
            // arrange
            var exceptionToThrow = new Exception();
            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Throws(exceptionToThrow);

            // act
            Func<Task> retrieveDocumentsAction = () => _sut
                .RetrieveDocumentsAsync(Enumerable.Empty<int>(), new HashSet<int>());

            // assert
            retrieveDocumentsAction.ShouldThrow<Exception>()
                .Which.Should().Be(exceptionToThrow);
        }

        [Test]
        public void RetrieveDocumentsAsync_ByFieldNames_ShouldRethrowObjectManagerException()
        {
            // arrange
            var exceptionToThrow = new Exception();
            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Throws(exceptionToThrow);

            // act
            Func<Task> retrieveDocumentsAction = () => _sut
                .RetrieveDocumentsAsync(Enumerable.Empty<int>(), new HashSet<string>());

            // assert
            retrieveDocumentsAction.ShouldThrow<Exception>()
                .Which.Should().Be(exceptionToThrow);
        }

        [Test]
        public async Task RetrieveDocumentByIdentifierPrefixAsync_ShouldBuildProperRequest()
        {
            // arrange
            const string documentIdentifierField = "CONTROL NUMBER";
            const string identifierPrefix = "ZIPPER";
            var response = new List<Document>();

            _objectManagerMock
                .Setup(x => x.QueryAsync<Document>(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<bool>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));

            Func<QueryRequest, bool> queryRequestValidator = queryRequest =>
            {
                bool isValid = true;
                isValid &= queryRequest.Condition == @"'CONTROL NUMBER' like 'ZIPPER%'";
                return isValid;
            };

            // act
            await _sut
                .RetrieveDocumentByIdentifierPrefixAsync(documentIdentifierField, identifierPrefix)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(
                x => x.QueryAsync<Document>(
                    It.Is<QueryRequest>(query => queryRequestValidator(query)),
                    true,
                    ExecutionIdentity.CurrentUser));
        }

        [Test]
        public async Task RetrieveDocumentByIdentifierPrefixAsync_ShouldReturnArtifactIDs()
        {
            // arrange
            const string documentIdentifierField = "CONTROL NUMBER";
            const string identifierPrefix = "ZIPPER";
            var documentsIDS = new List<int> { 5324, 546596, 31232, 312412, 32132 };
            List<Document> response = documentsIDS.Select(id => new Document { ArtifactId = id }).ToList();

            _objectManagerMock
                .Setup(x => x.QueryAsync<Document>(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<bool>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));

            // act
            int[] result = await _sut
                .RetrieveDocumentByIdentifierPrefixAsync(documentIdentifierField, identifierPrefix)
                .ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(documentsIDS);
        }

        [Test]
        public void RetrieveDocumentByIdentifierPrefixAsync_ShouldRethrowObjectManagerException()
        {
            // arrange
            var exceptionToThrow = new Exception();
            _objectManagerMock
                .Setup(x => x.QueryAsync<Document>(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<bool>(),
                    It.IsAny<ExecutionIdentity>()))
                .Throws(exceptionToThrow);

            // act
            Func<Task> retrieveDocumentsAction = () => _sut
                .RetrieveDocumentByIdentifierPrefixAsync(string.Empty, string.Empty);

            // assert
            retrieveDocumentsAction.ShouldThrow<Exception>()
                .Which.Should().Be(exceptionToThrow);
        }

        [Test]
        public Task MassUpdateAsync_ShouldBuildProperRequest()
        {
            return _massUpdateTests.ShouldBuildProperRequest();
        }

        [TestCase(true)]
        [TestCase(false)]
        public Task MassUpdateAsync_ShouldReturnCorrectResult(bool expectedResult)
        {
            return _massUpdateTests.ShouldReturnCorrectResult(expectedResult);
        }

        [Test]
        public void MassUpdateAsync_ShouldRethrowObjectManagerException()
        {
            _massUpdateTests.ShouldRethrowObjectManagerException();
        }

        [Test]
        public async Task InitializeSearchExport_ShouldCallObjectManagerWithProperParametersAndReturnProperValue()
        {
            // arrange
            int[] viewFieldIDs = { 1, 5, 88, 2222 };
            QueryRequest queryRequest = PrepareTestQueryRequestForSavedSearch(viewFieldIDs);
            ExportInitializationResults exportInitializationResults = CreateTestExportInitializationResults();
            _objectManagerMock.Setup(x =>
                x.InitializeExportAsync(
                    It.IsAny<QueryRequest>(),
                    _START_AT_RECORD,
                    ExecutionIdentity.CurrentUser
                )
            ).ReturnsAsync(exportInitializationResults);

            // act
            ExportInitializationResultsDto result = await _sut
                .InitializeSearchExportAsync(_SEARCH_ARTIFACT_ID, viewFieldIDs, _START_AT_RECORD)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x =>
                x.InitializeExportAsync(
                    It.Is<QueryRequest>(query =>
                        VerifyFieldsAreIdentical(queryRequest, query) &&
                        queryRequest.Condition == query.Condition
                    ),
                    _START_AT_RECORD,
                    ExecutionIdentity.CurrentUser),
                Times.Once);
            result.FieldNames.Should().Equal(
                exportInitializationResults.FieldData.Select(x => x.Name));
        }

        [Test]
        public async Task InitializeProductionExport_ShouldCallObjectManagerWithProperParametersAndReturnProperValue()
        {
            // arrange
            int[] viewFieldIDs = { 1, 5, 88, 2222 };
            QueryRequest queryRequest = PrepareTestQueryRequestForProduction(viewFieldIDs);
            ExportInitializationResults exportInitializationResults = CreateTestExportInitializationResults();
            _objectManagerMock.Setup(x =>
                x.InitializeExportAsync(
                    It.IsAny<QueryRequest>(),
                    _START_AT_RECORD,
                    ExecutionIdentity.CurrentUser
                )
            ).ReturnsAsync(exportInitializationResults);

            // act
            ExportInitializationResultsDto result = await _sut
                .InitializeProductionExportAsync(_PRODUCTION_ARTIFACT_ID, viewFieldIDs, _START_AT_RECORD)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x =>
                x.InitializeExportAsync(
                    It.Is<QueryRequest>(query =>
                        VerifyFieldsAreIdentical(queryRequest, query) &&
                        queryRequest.Condition == query.Condition
                    ),
                    _START_AT_RECORD,
                    ExecutionIdentity.CurrentUser),
                Times.Once);
            result.FieldNames.Should().Equal(
                exportInitializationResults.FieldData.Select(x => x.Name));
        }

        [Test]
        public async Task RetrieveResultsBlockFromExport_ShouldReturnSameResultAsObjectManager()
        {
            // arrange
            const int resultsBlockSize = 10;
            const int exportIndexID = 0;
            RelativityObjectSlim[] objects = CreateTestRelativityObjectsSlim(resultsBlockSize);
            _objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(
                        _runIDGuid,
                        resultsBlockSize,
                        exportIndexID,
                        ExecutionIdentity.CurrentUser))
                .ReturnsAsync(objects);

            // act
            IList<RelativityObjectSlimDto> result = await _sut
                .RetrieveResultsBlockFromExportAsync(_initializationResults, resultsBlockSize, exportIndexID)
                .ConfigureAwait(false);

            // assert
            _objectManagerMock.Verify(x =>
                    x.RetrieveResultsBlockFromExportAsync(
                        _runIDGuid,
                        resultsBlockSize,
                        exportIndexID,
                        ExecutionIdentity.CurrentUser),
                Times.Once);
            result.Count.Should().Be(objects.Length);
            VerifyObjectDtosAreTheSameAsObjects(result, objects);
        }

        private void Arrange_RetrieveDocumentsAsync_ShouldBuildProperRequest(IEnumerable<int> artifactIDs)
        {
            List<RelativityObject> response = artifactIDs
                .Select(
                    artifactID => new RelativityObject
                    {
                        ArtifactID = artifactID,
                        FieldValues = new List<FieldValuePair>()
                    })
                .ToList();

            _objectManagerMock
                .Setup(x => x.QueryAsync(
                    It.IsAny<QueryRequest>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(Task.FromResult(response));
        }

        private ExportInitializationResults CreateTestExportInitializationResults()
        {
            List<FieldMetadata> fields = new List<FieldMetadata>
            {
                new FieldMetadata
                {
                    Name = _CONTROL_NUMBER_FIELD_NAME
                },
                new FieldMetadata
                {
                    Name = _EXTRACTED_TEXT_FIELD_NAME
                }
            };
            ExportInitializationResults exportInitializationResults = new ExportInitializationResults()
            {
                RunID = _runIDGuid,
                RecordCount = fields.Count,
                FieldData = fields
            };
            return exportInitializationResults;
        }

        private static RelativityObjectSlim[] CreateTestRelativityObjectsSlim(int size)
        {
            var objects = new RelativityObjectSlim[size];
            int iterator = 1;
            for (int i = 0; i < size; ++i)
            {
                int artifactID = ++iterator;
                var values = new List<object> {++iterator, ++iterator, ++iterator, ++iterator};
                var objectSlim = new RelativityObjectSlim
                {
                    ArtifactID = artifactID,
                    Values = values
                };
                objects[i] = objectSlim;
            }
            return objects;
        }

        private static QueryRequest PrepareTestQueryRequestForSavedSearch(IEnumerable<int> artifactViewFieldIDs)
        {
            string queryString = $"'ArtifactId' IN SAVEDSEARCH {_SEARCH_ARTIFACT_ID}";
            return PrepareTestQueryRequest(artifactViewFieldIDs, queryString);
        }

        private static QueryRequest PrepareTestQueryRequestForProduction(IEnumerable<int> artifactViewFieldIDs)
        {
            string queryString =
                $"(('Production' SUBQUERY ((('Production::ProductionSet' == OBJECT {_PRODUCTION_ARTIFACT_ID})))))";
            return PrepareTestQueryRequest(artifactViewFieldIDs, queryString);
        }

        private static QueryRequest PrepareTestQueryRequest(IEnumerable<int> artifactViewFieldIDs, string queryString)
        {
            List<FieldRef> fields = artifactViewFieldIDs.Select(
                artifactViewFieldId => new FieldRef
                {
                    ArtifactID = artifactViewFieldId
                }
            ).ToList();

            var queryRequest = new QueryRequest
            {
                Fields = fields,
                Condition = queryString
            };
            return queryRequest;
        }

        private bool VerifyFieldsAreIdentical(QueryRequest expectedQueryRequest, QueryRequest actualQueryRequest)
        {
            expectedQueryRequest.Fields.Length().Should().Be(actualQueryRequest.Fields.Length());
            FieldRef[] expectedFieldRef = expectedQueryRequest.Fields.ToArray();
            FieldRef[] actualFieldRef = actualQueryRequest.Fields.ToArray();

            var asserts = expectedFieldRef.Zip(actualFieldRef, (a, e) => new
            {
                Expected = e,
                Actual = a
            });

            foreach (var assert in asserts)
            {
                assert.Expected.ArtifactID.Should().Be(assert.Actual.ArtifactID);
                assert.Expected.Guid.Should().Be(assert.Actual.Guid);
                assert.Expected.ViewFieldID.Should().Be(assert.Actual.ViewFieldID);
                assert.Expected.Name.Should().Be(assert.Actual.Name);
            }

            return true;
        }

        private void VerifyObjectDtosAreTheSameAsObjects(
            IEnumerable<RelativityObjectSlimDto> objectDtos,
            IEnumerable<RelativityObjectSlim> objects)
        {
            var zippedObjects = objectDtos.Zip(objects, (a, e) => new
            {
                ObjectDto = a,
                Object = e
            });

            foreach (var zippedObject in zippedObjects)
            {
                RelativityObjectSlimDto objectDto = zippedObject.ObjectDto;
                RelativityObjectSlim relativityObject = zippedObject.Object;
                objectDto.ArtifactID.Should().Be(relativityObject.ArtifactID);
                objectDto.FieldValues.Values.Should().BeEquivalentTo(relativityObject.Values);
            }
        }
    }
}
