using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Castle.Core.Internal;
using Relativity.API;
using Relativity.Core;


namespace kCura.IntegrationPoints.Data
{
	public class TempDocTableHelper : ITempDocTableHelper
	{
		private readonly ICoreContext _context;
		private readonly IDBContext _caseContext;
		private readonly string _tableName;
		private string _tableSuffix;

		public TempDocTableHelper(ICoreContext context, string tableName, string tableSuffix)
		{
			_context = context;
			_tableName = tableName;
			_tableSuffix = tableSuffix;
		}

		public TempDocTableHelper(IDBContext caseContext, string tableName, string tableSuffix = "")
		{
			_caseContext = caseContext;
			_tableName = tableName;
			if (!String.IsNullOrEmpty(tableSuffix))
			{
				_tableSuffix = tableSuffix;
			}		
		}

		public void CreateTemporaryDocTable(List<int> artifactIds)
		{
			if (!artifactIds.IsNullOrEmpty())
			{
				string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";
				string fullTableName = _tableName + "_" + _tableSuffix;

				string sql = String.Format(@"IF NOT EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
											BEGIN 
											CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED, [ID] [int] IDENTITY(1,1) NOT NULL)
											END
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", fullTableName, artifactIdList);

				_context.ChicagoContext.DBContext.ExecuteNonQuerySQLStatement(sql);
			}
		}

		public void RemoveErrorDocument(string docIdentifier)
		{
			int docId = GetDocumentId(docIdentifier);
			string fullTableName = _tableName + "_" + _tableSuffix;
			string sql = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", fullTableName, docId);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public List<int> GetDocumentIdsFromTable()
		{
			string fullTableName = _tableName + "_" + _tableSuffix;
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										SELECT [ArtifactID] FROM [EDDSRESOURCE]..[{0}]", fullTableName);
			var documentIds = new List<int>();

			SqlDataReader docIdReader = _caseContext.ExecuteSQLStatementAsReader(sql);
			while (docIdReader.Read())
			{
				int docId = Convert.ToInt32(docIdReader["ArtifactID"]);
				documentIds.Add(docId);
			}
	
			docIdReader.Close();

			return documentIds;
		}

		public void DeleteTable()
		{
			string fullTableName = _tableName + "_" + _tableSuffix;
			string sql = String.Format(@"IF EXISTS (SELECT * FROM EDDSRESOURCE.INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}')
										DROP TABLE [EDDSRESOURCE]..[{0}]", fullTableName);

			_caseContext.ExecuteNonQuerySQLStatement(sql);
		}

		public void SetTableSuffix(string tableSuffix)
		{
			_tableSuffix = tableSuffix;
		}

		private int GetDocumentId(string controlNumber)
		{
			string sql = String.Format(@"Select [ArtifactId] FROM [Document] WHERE [ControlNumber] = '{0}'", controlNumber);

			return _caseContext.ExecuteSqlStatementAsScalar<int>(sql);
		}
	}
}
