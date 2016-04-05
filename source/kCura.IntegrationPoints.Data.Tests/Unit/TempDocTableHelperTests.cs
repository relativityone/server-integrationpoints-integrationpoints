using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class TempDocTableHelperTests
	{
		private string _tableName = "Temp_Doc_Table";
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

			_instance = new TempDocTableHelper(_helper, _tableName, _tableSuffix, _sourceWorkspaceId, _docIdColumn);
		}

		[Test]
		public void CreateTemporaryDocTable_Test()
		{
			//Arrange
			var artifactIds = new List<int>();
			artifactIds.Add(12345);
			artifactIds.Add(56789);
			string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
											BEGIN 
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED, [ID] [int] IDENTITY(1,1) NOT NULL)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", _tableName + "_" + _tableSuffix, artifactIdList);

			
			//Act
			_instance.CreateTemporaryDocTable(artifactIds);
			
			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		public void CreateTemporaryDocTable_EmptyList_Test()
		{
			//Arrange
			var artifactIds = new List<int>();
			string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

			string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
											BEGIN 
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED, [ID] [int] IDENTITY(1,1) NOT NULL)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", _tableName + "_" + _tableSuffix, artifactIdList);


			//Act
			_instance.CreateTemporaryDocTable(artifactIds);

			//Assert
			_caseContext.DidNotReceive().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		public void RemoveErrorDocument_Test()
		{
			//Arrange
			string docIdentifier = "REL-001";
			int docArtifactId = 12345;

			string sqlDelete = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", _tableName+ "_" +_tableSuffix, docArtifactId);
			string sqlGetId = String.Format(@"Select [ArtifactId] FROM [Document] WHERE [{0}] = '{1}'", _docIdColumn, docIdentifier);

			_caseContext.ExecuteSqlStatementAsScalar<int>(sqlGetId).Returns(docArtifactId);

			//Act
			_instance.RemoveErrorDocument(docIdentifier);

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sqlDelete);
		}

		[Test]
		public void DeleteTable_Test()
		{
			//Arrange
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]",_tableName + "_" + _tableSuffix);

			//Act
			_instance.DeleteTable();

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sql);
		}
	}
}
