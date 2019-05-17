using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class ScratchTableRepositorySqlTests
	{
		private IDocumentRepository _documentsRepo;
		private IFieldQueryRepository _fileRepo;
		private IRipDBContext _dbContext;
		private IResourceDbProvider _resourceDbProvider;

		private const string _PREFIX = "prefix";
		private const string _SUFFIX = "_suffix";
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1234321;
		[SetUp]
		public void SetUp()
		{
			_dbContext = Substitute.For<IRipDBContext>();
			_documentsRepo = Substitute.For<IDocumentRepository>();
			_fileRepo = Substitute.For<IFieldQueryRepository>();
			_resourceDbProvider = Substitute.For<IResourceDbProvider>();
			_resourceDbProvider.GetSchemalessResourceDataBasePrepend(Arg.Any<int>()).Returns($"EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}");
			_resourceDbProvider.GetResourceDbPrepend(Arg.Any<int>()).Returns("[Resource]");
		}

		#region SqlGeneration
		[Test]
		public void DeleteTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = $"IF EXISTS (SELECT * FROM EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}.INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'prefix__suffix') DROP TABLE [Resource].[prefix__suffix]";

			var instance = new ScratchTableRepository(_dbContext, _documentsRepo, _fileRepo, _resourceDbProvider, _PREFIX, _SUFFIX, _SOURCE_WORKSPACE_ARTIFACT_ID);

			// act
			instance.DeleteTable();

			// assert
			_dbContext.Received(1).ExecuteNonQuerySQLStatement(expectedQuery);
		}

		[Test]
		public void GetDocumentIdsDataReaderFromTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = $"IF EXISTS (SELECT * FROM EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prefix__suffix') SELECT [ArtifactID] FROM [Resource].[prefix__suffix]";

			var instance = new ScratchTableRepository(_dbContext, _documentsRepo, _fileRepo, _resourceDbProvider, _PREFIX, _SUFFIX, _SOURCE_WORKSPACE_ARTIFACT_ID);

			// act
			instance.GetDocumentIdsDataReaderFromTable();

			// assert
			_dbContext.Received(1).ExecuteSQLStatementAsReader(expectedQuery);
		}

		[Test]
		public void CreateBatchOfDocumentIdReader_ShouldExecuteProperSqlQuery()
		{
			// arrange
			int offset = 0;
			int size = 5;
			string expectedQuery = $@"IF EXISTS (SELECT * FROM EDDS{_SOURCE_WORKSPACE_ARTIFACT_ID}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prefix__suffix') SELECT [ArtifactID] FROM [Resource].[prefix__suffix] ORDER BY [ArtifactID] OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY";

			var instance = new ScratchTableRepository(_dbContext, _documentsRepo, _fileRepo, _resourceDbProvider, _PREFIX, _SUFFIX, _SOURCE_WORKSPACE_ARTIFACT_ID);

			// act
			instance.CreateBatchOfDocumentIdReader(offset, size);

			// assert
			_dbContext.Received(1).ExecuteSQLStatementAsReader(expectedQuery);
		}

		#endregion

	}
}