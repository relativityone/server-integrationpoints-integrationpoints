using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Field = Relativity.Services.Objects.DataContracts.Field;
using QueryResult = Relativity.Services.Objects.DataContracts.QueryResult;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
    [TestFixture, Category("Unit")]
    public class FieldsRepositoryTests
    {
        private const int _ARTIFACT_TYPE_ID = 111;
        private Mock<IObjectManager> _objectManagerMock;
        private IFieldsRepository _sut;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            Mock<IServicesMgr> servicesMgrFake = new Mock<IServicesMgr>();
            servicesMgrFake.Setup(x => x.CreateProxy<IObjectManager>(It.IsAny<ExecutionIdentity>())).Returns(_objectManagerMock.Object);
            _sut = new FieldsRepository(servicesMgrFake.Object, new Mock<IAPILog>().Object);
        }

        [Test]
        public async Task GetAllDocumentFieldsAsync_ShouldUseBatching_WhenQueryingWithObjectManager()
        {
            // Arrange
            MockSequence mockSequence = new MockSequence();
            QueryResult firstQueryResult = CreateQueryResult(Enumerable.Range(1, 2).Select(x => CreateField(x.ToString())).ToList());
            QueryResult secondQueryResult = CreateQueryResult(Enumerable.Range(3, 2).Select(x => CreateField(x.ToString())).ToList());
            QueryResult thirdQueryResult = CreateQueryResult(new List<RelativityObject>());

            _objectManagerMock
                .InSequence(mockSequence)
                .Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 50))
                .ReturnsAsync(firstQueryResult)
                .Verifiable();

            _objectManagerMock
                .InSequence(mockSequence)
                .Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 3, 50))
                .ReturnsAsync(secondQueryResult)
                .Verifiable();

            _objectManagerMock
                .InSequence(mockSequence)
                .Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 5, 50))
                .ReturnsAsync(thirdQueryResult)
                .Verifiable();

            // Act
            await _sut.GetAllFieldsAsync(It.IsAny<int>(), _ARTIFACT_TYPE_ID).ConfigureAwait(false);

            // Assert
            _objectManagerMock.Verify();
        }

        [Test]
        public async Task GetAllDocumentFieldsAsync_ShouldReturnAllDocumentFieldsForWorkspace()
        {
            // Arrange
            const int count = 3;
            SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField($"Field {x}", x, "Some type")).ToList());
            var fieldInfoRference = Enumerable.Range(1, count).Select(x => new FieldInfo($"{x}", $"Field {x}", "Some type"));

            // Act
            IList<FieldInfo> fields = (await _sut.GetAllFieldsAsync(It.IsAny<int>(), _ARTIFACT_TYPE_ID).ConfigureAwait(false)).ToList();

            // Assert
            fields.Should().HaveCount(3);
            fields.Should().Contain(f => f.FieldIdentifier == "1" && f.Name == "Field 1" && f.Type == "Some type");

            _objectManagerMock.Verify(
                m => m.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(q => q.Condition == $"'FieldArtifactTypeID' == {_ARTIFACT_TYPE_ID}"), It.IsAny<int>(), It.IsAny<int>()));
        }

        [Test]
        public async Task GetFieldsByArtifactsIdAsync_ShouldReturnEmptyCollction_WhenArtifactsListIsNull()
        {
            // Act
            IEnumerable<FieldInfo> fields = await _sut.GetFieldsByArtifactsIdAsync(null, It.IsAny<int>()).ConfigureAwait(false);

            // Assert
            fields.Should().BeEmpty();
        }

        [Test]
        public async Task GetFieldsByArtifactsIdAsync_ShouldReturnEmptyCollction_WhenArtifactsListIsEmpty()
        {
            // Act
            IEnumerable<FieldInfo> fields = await _sut.GetFieldsByArtifactsIdAsync(Enumerable.Empty<string>(), It.IsAny<int>()).ConfigureAwait(false);

            // Assert
            fields.Should().BeEmpty();
        }

        [Test]
        public async Task GetFieldsByArtifactsIdAsync_ShouldReturnDocumentsFieldsByArtifactsForWorkspace()
        {
            // Arrange
            const int count = 3;
            SetupWorkspaceFields(Enumerable.Range(1, count).Select(x => CreateField($"Field {x}", x, "Some type")).ToList());
            var fieldInfoRference = Enumerable.Range(1, count).Select(x => new FieldInfo($"{x}", $"Field {x}", "Some type"));

            // Act
            IList<FieldInfo> fields = (await _sut.GetFieldsByArtifactsIdAsync(new List<string> { "1", "2" }, It.IsAny<int>()).ConfigureAwait(false)).ToList();

            // Assert
            fields.Should().HaveCount(3);
            fields.Should().Contain(f => f.FieldIdentifier == "1" && f.Name == "Field 1" && f.Type == "Some type");

            _objectManagerMock.Verify(
                m => m.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(q => q.Condition == "'ArtifactID' IN [1,2]"), It.IsAny<int>(), It.IsAny<int>()));
        }

        private void SetupWorkspaceFields(List<RelativityObject> fields)
        {
            _objectManagerMock
                .SetupSequence(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(CreateQueryResult(fields))
                .ReturnsAsync(CreateQueryResult(new List<RelativityObject>()));
        }

        private QueryResult CreateQueryResult(List<RelativityObject> fields)
        {
            QueryResult queryResult = new QueryResult()
            {
                Objects = fields,
                ResultCount = fields.Count
            };
            return queryResult;
        }

        private RelativityObject CreateField(string name, int artifactID = 0, string type = "", bool isIdentifier = false)
        {
            return new RelativityObject()
            {
                ArtifactID = artifactID,
                Name = name,
                FieldValues = new List<FieldValuePair>()
                {
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Is Identifier"
                        },
                        Value = isIdentifier
                    },
                    new FieldValuePair()
                    {
                        Field = new Field()
                        {
                            Name = "Field Type"
                        },
                        Value = type
                    }
                }
            };
        }
    }
}
