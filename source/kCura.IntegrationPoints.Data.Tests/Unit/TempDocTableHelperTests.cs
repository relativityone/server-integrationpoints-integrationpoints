using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class TempDocTableHelperTests
	{
		private string tableNameDestWorkspace = "TempRIPDocTable_DW";
		private string tableNameJobHistory = "TempRIPDocTable_JH";
		private string _tableSuffix = "12345-6789";
		private int _sourceWorkspaceId = 99999;
		private string _docIdColumn = "ControlNumber";
		private IDBContext _caseContext;

		private ITempDocTableHelper _instance;
		private IHelper _helper;

		[SetUp]
		public void SetUp()
		{
			_caseContext = Substitute.For<IDBContext>();
			_helper = Substitute.For<IHelper>();
			_helper.GetDBContext(_sourceWorkspaceId).Returns(_caseContext);

			_instance = new TempDocTableHelper(_helper, _tableSuffix, _sourceWorkspaceId, _docIdColumn);
		}

		[Test]
		[Ignore("Nsubstitute add * in expected resutls ")]
		public void CreateTemporaryDocTable_DestWorkspace_GoldFlow()
		{
			//Arrange
			var artifactIds = new List<int>();
			artifactIds.Add(12345);
			artifactIds.Add(56789);
			string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN 
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", tableNameDestWorkspace + "_" + _tableSuffix, artifactIdList);

			//Act
			_instance.AddArtifactIdsIntoTempTable(artifactIds, Constants.TEMPORARY_DOC_TABLE_DEST_WS);
			
			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		[Ignore("Nsubstitute add * in expected resutls ")]
		public void CreateTemporaryDocTable_JobHistory_GoldFlow()
		{
			//Arrange
			var artifactIds = new List<int>();
			artifactIds.Add(12345);
			artifactIds.Add(56789);
			string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')
											BEGIN 
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", tableNameJobHistory + "_" + _tableSuffix, artifactIdList);

			//Act
			_instance.AddArtifactIdsIntoTempTable(artifactIds, Constants.TEMPORARY_DOC_TABLE_JOB_HIST);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		public void CreateTemporaryDocTable_EmptyDocumentList()
		{
			//Arrange
			var artifactIds = new List<int>();

			//Act
			_instance.AddArtifactIdsIntoTempTable(artifactIds, Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS);

			//Assert
			_caseContext.DidNotReceive().ExecuteNonQuerySQLStatement(Arg.Any<string>());
		}

		[Test]
		public void RemoveErrorDocument_GoldFlow()
		{
			//Arrange
			string docIdentifier = "REL-001";
			int docArtifactId = 12345;

			string sqlDelete = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", tableNameDestWorkspace + "_" +_tableSuffix, docArtifactId);
			string sqlGetId = String.Format(@"Select [ArtifactId] FROM [Document] WITH (NOLOCK) WHERE [{0}] = '{1}'", _docIdColumn, docIdentifier);

			_caseContext.ExecuteSqlStatementAsScalar<int>(sqlGetId).Returns(docArtifactId);

			//Act
			_instance.RemoveErrorDocument(tableNameDestWorkspace, docIdentifier);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sqlDelete);
		}

		[Test]
		public void DeleteTable_DestinationWorkspace_GoldFlow()
		{
			//Arrange
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", tableNameDestWorkspace + "_" + _tableSuffix);

			//Act
			_instance.DeleteTable(Data.Constants.TEMPORARY_DOC_TABLE_DEST_WS);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sql);
		}

		[Test]
		public void DeleteTable_JobHistory_GoldFlow()
		{
			//Arrange
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", tableNameJobHistory + "_" + _tableSuffix);

			//Act
			_instance.DeleteTable(Data.Constants.TEMPORARY_DOC_TABLE_JOB_HIST);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sql);
		}
	}
}
