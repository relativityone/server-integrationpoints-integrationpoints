using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.Toggle;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	public class ScratchTableRepositorySqlTests : RelativityProviderTemplate
	{
		private IHelper _helper;
		private IDocumentRepository _documentsRepo;
		private IFieldRepository _fileRepo;
		private IDBContext _dbContext;
		private const string _PREFIX = "prefix";
		private const string _SUFFIX = "_suffix";

		public ScratchTableRepositorySqlTests() : base("Scratch table", null)
		{
		}

		public override void TestSetup()
		{
			_dbContext = Substitute.For<IDBContext>();
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(SourceWorkspaceArtifactId).Returns(_dbContext);
			_documentsRepo = Substitute.For<IDocumentRepository>();
			_fileRepo = Substitute.For<IFieldRepository>();
		}

		#region SqlGeneration
		[Test]
		public void DeleteTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Resource_prefix__suffix')
										DROP TABLE [Resource].[Resource_prefix__suffix]";

			var instance = new ScratchTableRepository(_helper, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);

			// act
			instance.DeleteTable();

			// assert
			_dbContext.Received(1).ExecuteNonQuerySQLStatement(expectedQuery);
		}

		[Test]
		public void AddArtifactIdsIntoScratchTable_WorkspaceScratchTable()
		{
			// arrange
			using (var instance = new ScratchTableRepository(Helper,  _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId))
			{
				var list = new List<int>() {1, 2};
				// act & assert
				Assert.DoesNotThrow(() => instance.AddArtifactIdsIntoTempTable(list));
			}
		}

		[Test]
		public void GetDocumentIdsDataReaderFromTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Resource_prefix__suffix')
											SELECT [ArtifactID] FROM [Resource].[Resource_prefix__suffix]";

			var instance = new ScratchTableRepository(_helper, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);

			// act
			instance.GetDocumentIdsDataReaderFromTable();

			// assert
			_dbContext.Received(1).ExecuteSQLStatementAsReader(expectedQuery);
		}

		#endregion

	}
}