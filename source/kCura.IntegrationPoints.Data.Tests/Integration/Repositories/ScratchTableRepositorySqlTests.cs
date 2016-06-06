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
	[Explicit]
	public class ScratchTableRepositorySqlTests : WorkspaceDependentTemplate
	{
		private IHelper _helper;
		private IExtendedRelativityToggle _toggle;
		private IDocumentRepository _documentsRepo;
		private IFieldRepository _fileRepo;
		private IDBContext _dbContext;
		private const string _PREFIX = "prefix";
		private const string _SUFFIX = "_suffix";

		public ScratchTableRepositorySqlTests()
			: base("Scratch table", null)
		{
		}

		[SetUp]
		public void Setup()
		{
			_dbContext = Substitute.For<IDBContext>();
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(SourceWorkspaceArtifactId).Returns(_dbContext);
			_toggle = Substitute.For<IExtendedRelativityToggle>();
			_documentsRepo = Substitute.For<IDocumentRepository>();
			_fileRepo = Substitute.For<IFieldRepository>();
		}

		#region SqlGeneration
		[Test]
		public void DeleteTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'EDDSResource_prefix__suffix')
										DROP TABLE [Resource].[EDDSResource_prefix__suffix]";
			_toggle.IsAOAGFeatureEnabled().Returns(true);
			var instance = new ScratchTableRepository(_helper, _toggle, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);

			// act
			instance.DeleteTable();

			// assert
			_dbContext.Received(1).ExecuteNonQuerySQLStatement(expectedQuery);
		}

		[Test]
		public void DeleteTable_EDDSResourceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF EXISTS (SELECT * FROM [EDDSRESOURCE].INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'prefix__suffix')
										DROP TABLE [EDDSRESOURCE]..[prefix__suffix]";
			_toggle.IsAOAGFeatureEnabled().Returns(false);
			var instance = new ScratchTableRepository(_helper, _toggle, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);

			// act
			instance.DeleteTable();

			// assert
			_dbContext.Received(1).ExecuteNonQuerySQLStatement(expectedQuery);
		}

		[Test]
		public void AddArtifactIdsIntoScratchTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EDDSResource_prefix__suffix')
											BEGIN
												CREATE TABLE [Resource].[EDDSResource_prefix__suffix] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [Resource].[EDDSResource_prefix__suffix] ([ArtifactID]) VALUES (1),(2)";
			_toggle.IsAOAGFeatureEnabled().Returns(true);
			var instance = new ScratchTableRepository(_helper, _toggle, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);
			var list = new List<int>() { 1, 2 };

			// act
			instance.AddArtifactIdsIntoTempTable(list);

			// assert
			_dbContext.Received(1).ExecuteNonQuerySQLStatement(expectedQuery);

		}

		[Test]
		public void AddArtifactIdsIntoScratchTable_EddsResourceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF NOT EXISTS (SELECT * FROM [EDDSRESOURCE].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prefix__suffix')
											BEGIN
												CREATE TABLE [EDDSRESOURCE]..[prefix__suffix] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[prefix__suffix] ([ArtifactID]) VALUES (1),(2)";
			_toggle.IsAOAGFeatureEnabled().Returns(false);
			var instance = new ScratchTableRepository(_helper, _toggle, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);
			var list = new List<int>() { 1, 2 };

			// act
			instance.AddArtifactIdsIntoTempTable(list);

			// assert
			_dbContext.Received(1).ExecuteNonQuerySQLStatement(expectedQuery);

		}

		[Test]
		public void GetDocumentIdsDataReaderFromTable_WorkspaceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EDDSResource_prefix__suffix')
											SELECT [ArtifactID] FROM [Resource].[EDDSResource_prefix__suffix]";
			_toggle.IsAOAGFeatureEnabled().Returns(true);
			var instance = new ScratchTableRepository(_helper, _toggle, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);

			// act
			instance.GetDocumentIdsDataReaderFromTable();

			// assert
			_dbContext.Received(1).ExecuteSQLStatementAsReader(expectedQuery);
		}

		[Test]
		public void GetDocumentIdsDataReaderFromTable_EddsResourceScratchTable()
		{
			// arrange
			string expectedQuery = @"IF EXISTS (SELECT * FROM [EDDSRESOURCE].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prefix__suffix')
											SELECT [ArtifactID] FROM [EDDSRESOURCE]..[prefix__suffix]";
			_toggle.IsAOAGFeatureEnabled().Returns(false);
			var instance = new ScratchTableRepository(_helper, _toggle, _documentsRepo, _fileRepo, _PREFIX, _SUFFIX, SourceWorkspaceArtifactId);

			// act
			instance.GetDocumentIdsDataReaderFromTable();

			// assert
			_dbContext.Received(1).ExecuteSQLStatementAsReader(expectedQuery);
		}

		#endregion

	}
}