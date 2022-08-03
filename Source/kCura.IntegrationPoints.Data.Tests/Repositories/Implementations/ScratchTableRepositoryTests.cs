using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class ScratchTableRepositoryTests
    {
        private Mock<IDocumentRepository> _documentRepositoryMock;
        private Mock<IFieldQueryRepository> _fieldRepositoryMock;
        private Mock<IResourceDbProvider> _resourceDbProviderMock;
        private Mock<IWorkspaceDBContext> _workspaceDbContextMock;

        private ScratchTableRepository _sut;
        private string _schemalessResourceDatabasePrepend;
        private string _tableName;
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1234321;
        private const string _PREFIX = "prefix";
        private const string _SUFFIX = "_suffix";
        private const string _RESOURCE_DB_PREPEND = "[Resource]";
        private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "ArtifactID";

        [SetUp]
        public void SetUp()
        {
            _tableName = $"{_PREFIX}_{_SUFFIX}";
            _schemalessResourceDatabasePrepend = $"EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}";
            _workspaceDbContextMock = new Mock<IWorkspaceDBContext>();
            _documentRepositoryMock = new Mock<IDocumentRepository>();
            _fieldRepositoryMock = new Mock<IFieldQueryRepository>();
            _resourceDbProviderMock = new Mock<IResourceDbProvider>();
            _resourceDbProviderMock.Setup(x => x.GetSchemalessResourceDataBasePrepend(It.IsAny<int>())).Returns($"EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}");
            _resourceDbProviderMock.Setup(x => x.GetResourceDbPrepend(It.IsAny<int>())).Returns("[Resource]");

            _sut = new ScratchTableRepository(
                _workspaceDbContextMock.Object,
                _documentRepositoryMock.Object,
                _fieldRepositoryMock.Object,
                _resourceDbProviderMock.Object,
                _PREFIX,
                _SUFFIX,
                _SOURCE_WORKSPACE_ARTIFACT_ID
            );
        }

        [Test]
        public void DeleteTable_ShouldPassProperSqlQueryToContext()
        {
            //ARRANGE
            string expectedQuery =
                $@"
            IF EXISTS (SELECT * FROM EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{_tableName}') 
            DROP TABLE [Resource].[{_tableName}]
            ";

            //ACT
            _sut.DeleteTable();

            //ASSERT
            _workspaceDbContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(expectedQuery), Times.Once);
        }

        [Test]
        public void GetDocumentIDsDataReaderFromTable_ShouldPassProperSqlQueryToContext()
        {
            //ARRANGE
            string expectedQuery =
                $@"
            IF EXISTS (SELECT * FROM {_schemalessResourceDatabasePrepend}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_tableName}') 
            SELECT [ArtifactID] FROM {_RESOURCE_DB_PREPEND}.[{_tableName}]
            ";

            //ACT
            _sut.GetDocumentIDsDataReaderFromTable();

            //ASSERT
            _workspaceDbContextMock.Verify(x => x.ExecuteSQLStatementAsReader(expectedQuery), Times.Once);
        }

        [Test]
        public void ReadDocumentIDs_ShouldPassProperSqlQueryToContext()
        {
            //ARRANGE
            Mock<IDataReader> dataReaderMock = new Mock<IDataReader>();
            _workspaceDbContextMock.Setup(x =>
                    x.ExecuteSQLStatementAsReader(It.IsAny<string>()))
                .Returns(dataReaderMock.Object);

            int offset = 0;
            int size = 5;
            string expectedQuery =
                $@"
            IF EXISTS (SELECT * FROM {_schemalessResourceDatabasePrepend}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_tableName}') 
            SELECT [ArtifactID] FROM {_RESOURCE_DB_PREPEND}.[{_tableName}]
            ORDER BY [ArtifactID] OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY";

            //ACT
            _sut.ReadArtifactIDs(offset, size);

            //ASSERT
            _workspaceDbContextMock.Verify(x => x.ExecuteSQLStatementAsReader(expectedQuery), Times.Once);
        }

        [Test]
        public void RemoveErrorDocuments_ShouldPassProperSqlQueryToContext()
        {
            //ARRANGE
            string[] documentControlNumbers = { "CN1", "CN2", "CN3" };
            int[] documentArtifactIDs = { 1, 2, 3 };
            string documentArtifactIDsAsString = $"({string.Join(",", documentArtifactIDs)})";

            _documentRepositoryMock.Setup(x =>
                x.RetrieveDocumentsAsync(
                    It.IsAny<string>(),
                    It.IsAny<ICollection<string>>()
                )
            ).ReturnsAsync(documentArtifactIDs.ToArray());

            string expectedQuery = $@"DELETE FROM {_RESOURCE_DB_PREPEND}.[{_tableName}] WHERE [{_DOCUMENT_ARTIFACT_ID_COLUMN_NAME}] in {documentArtifactIDsAsString}";

            //ACT
            _sut.RemoveErrorDocuments(documentControlNumbers);

            //ASSERT
            _workspaceDbContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(expectedQuery), Times.Once);
        }

        [Test]
        public void RemoveErrorDocuments_ShouldNotThrowIfParameterIsNull()
        {
            //ARRANGE
            Action action = () => _sut.RemoveErrorDocuments(documentControlNumbers: null);

            //ACT & ASSERT
            action.ShouldNotThrow();
        }

        [Test]
        public void RemoveErrorDocuments_ShouldNotThrowIfParameterIsEmptyArray()
        {
            //ARRANGE
            string[] emptyArray = { };
            Action action = () => _sut.RemoveErrorDocuments(documentControlNumbers: emptyArray);

            //ACT & ASSERT
            action.ShouldNotThrow();
        }
    }
}