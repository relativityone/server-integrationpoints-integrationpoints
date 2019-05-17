using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class ScratchTableRepositoryTests
	{
		private Mock<IDocumentRepository> _documentsRepositoryMock;
		private Mock<IFieldQueryRepository> _fileRepositoryMock;
		private Mock<IResourceDbProvider> _resourceDbProviderMock;
		private Mock<IWorkspaceDBContext> _workspaceDbContextMock;

		private ScratchTableRepository _sut;
		private string _SCHEMALESS_RESOURCE_DATABASE_PREPEND;
		private string _TABLE_NAME;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1234321;
		private const string _PREFIX = "prefix";
		private const string _SUFFIX = "_suffix";
		private const string _RESOURCE_DB_PREPEND = "[Resource]";
		[SetUp]
		public void SetUp()
		{
			_TABLE_NAME = $"{_PREFIX}_{_SUFFIX}";
			_SCHEMALESS_RESOURCE_DATABASE_PREPEND = $"EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}";
			_workspaceDbContextMock = new Mock<IWorkspaceDBContext>();
			_documentsRepositoryMock = new Mock<IDocumentRepository>();
			_fileRepositoryMock = new Mock<IFieldQueryRepository>();
			_resourceDbProviderMock = new Mock<IResourceDbProvider>();
			_resourceDbProviderMock.Setup(x => x.GetSchemalessResourceDataBasePrepend(It.IsAny<int>())).Returns($"EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}");
			_resourceDbProviderMock.Setup(x => x.GetResourceDbPrepend(It.IsAny<int>())).Returns("[Resource]");

			_sut = new ScratchTableRepository(
				_workspaceDbContextMock.Object,
				_documentsRepositoryMock.Object,
				_fileRepositoryMock.Object,
				_resourceDbProviderMock.Object,
				_PREFIX,
				_SUFFIX,
				_SOURCE_WORKSPACE_ARTIFACT_ID
			);
		}

		[Test]
		public void DeleteTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery =
				$@"
			IF EXISTS (SELECT * FROM EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{_TABLE_NAME}') 
			DROP TABLE [Resource].[{_TABLE_NAME}]
			";

			// act
			_sut.DeleteTable();

			// assert
			_workspaceDbContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(expectedQuery), Times.Once);
		}

		[Test]
		public void GetDocumentIdsDataReaderFromTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery =
				$@"
			IF EXISTS (SELECT * FROM {_SCHEMALESS_RESOURCE_DATABASE_PREPEND}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_TABLE_NAME}') 
			SELECT [ArtifactID] FROM {_RESOURCE_DB_PREPEND}.[{_TABLE_NAME}]
			";

			// act
			_sut.GetDocumentIDsDataReaderFromTable();

			// assert
			_workspaceDbContextMock.Verify(x => x.ExecuteSQLStatementAsReader(expectedQuery), Times.Once);
		}

		[Test]
		public void CreateBatchOfDocumentIdReader_ShouldExecuteProperSqlQuery()
		{
			// arrange
			Mock<IDataReader> dataReaderMock = new Mock<IDataReader>();
			_workspaceDbContextMock.Setup(x =>
					x.ExecuteSQLStatementAsReader(It.IsAny<string>()))
				.Returns(dataReaderMock.Object);

			int offset = 0;
			int size = 5;
			string expectedQuery =
				$@"
			IF EXISTS (SELECT * FROM {_SCHEMALESS_RESOURCE_DATABASE_PREPEND}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_TABLE_NAME}') 
			SELECT [ArtifactID] FROM {_RESOURCE_DB_PREPEND}.[{_TABLE_NAME}] ORDER BY [ArtifactID] OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
			";

			// act
			_sut.ReadDocumentIDs(offset, size).ToList();

			// assert
			_workspaceDbContextMock.Verify(x => x.ExecuteSQLStatementAsReader(expectedQuery), Times.Once);
		}
	}
}