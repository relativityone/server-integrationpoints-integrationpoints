using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using Relativity.Core;


namespace kCura.IntegrationPoints.Data
{
	public class TempDocTableHelper : ITempDocTableHelper
	{
		private readonly ICoreContext _context;
		private readonly ICaseServiceContext _caseContext;
		private readonly string _tableName;

		public TempDocTableHelper(ICoreContext context, string tableName)
		{
			_context = context;
			_tableName = tableName;
		}

		public TempDocTableHelper(ICaseServiceContext caseContext, string tableName)
		{
			_caseContext = caseContext;
			_tableName = tableName;
		}

		public void CreateTemporaryDocTable(List<int> artifactIds)
		{
			if (!artifactIds.IsNullOrEmpty())
			{
				string artifactIdList = "(" + String.Join("),(", artifactIds.Select(x => x.ToString())) + ")";

				string sql = String.Format(@"CREATE TABLE [EDDSRESOURCE]..[{0}] ([ArtifactID] INT PRIMARY KEY CLUSTERED, [ID] [int] IDENTITY(1,1) NOT NULL)
									INSERT INTO [EDDSRESOURCE]..[{0}] ([ArtifactID]) VALUES {1}", _tableName, artifactIdList);

				_context.ChicagoContext.DBContext.ExecuteNonQuerySQLStatement(sql);
			}
		}

		public void RemoveErrorDocument(string docIdentifier, string tableSuffix)
		{
			int docId = GetDocumentId(docIdentifier);
			string fullTableName = _tableName + "_" + tableSuffix;
			string sql = String.Format(@"DELETE FROM EDDSRESOURCE..[{0}] WHERE [ArtifactID] = {1}", fullTableName, docId);

			_caseContext.SqlContext.ExecuteNonQuerySQLStatement(sql);
		}

		private int GetDocumentId(string controlNumber)
		{
			string sql = String.Format(@"Select [ArtifactId] FROM [Document] WHERE [ControlNumber] = '{0}'", controlNumber);

			return _caseContext.SqlContext.ExecuteSqlStatementAsScalar<int>(sql);
		}
	}
}
