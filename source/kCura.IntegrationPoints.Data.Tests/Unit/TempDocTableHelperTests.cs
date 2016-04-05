﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class TempDocTableHelperTests
	{
		private string _tableName = "Temp_Doc_Table";
		private string _tableSuffix = "12345-6789";
		private ICoreContext _context;
		private IDBContext _caseContext;

		private ITempDocumentFactory _factory;
		private ITempDocTableHelper _instanceForCreation;
		private ITempDocTableHelper _instanceForDeletion;

		[SetUp]
		public void SetUp()
		{
			_context = Substitute.For<ICoreContext>();
			_caseContext = Substitute.For<IDBContext>();

			_factory = new TempDocumentFactory();
			_instanceForCreation = _factory.GetTableCreationHelper(_context, _tableName, _tableSuffix);
			_instanceForDeletion = _factory.GetDocTableHelper(_caseContext, _tableName);

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
			_instanceForCreation.CreateTemporaryDocTable(artifactIds);
			
			//Assert
			_context.ChicagoContext.DBContext.Received().ExecuteNonQuerySQLStatement(Arg.Is(sql));
		}

		[Test]
		public void RemoveErrorDocument_Test()
		{
			//Arrange
			string docIdentifier = "REL-001";
			int docArtifactId = 12345;

			string sqlDelete = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", _tableName+ "_" +_tableSuffix, docArtifactId);
			string sqlGetId = String.Format(@"Select [ArtifactId] FROM [Document] WHERE [ControlNumber] = '{0}'", docIdentifier);

			_caseContext.ExecuteSqlStatementAsScalar<int>(sqlGetId).Returns(docArtifactId);

			//Act
			_instanceForDeletion.SetTableSuffix(_tableSuffix);
			_instanceForDeletion.RemoveErrorDocument(docIdentifier);

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
			_instanceForDeletion.SetTableSuffix(_tableSuffix);
			_instanceForDeletion.DeleteTable();

			//Assert
			_caseContext.Received().ExecuteNonQuerySQLStatement(sql);
		}
	}
}
