using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeWorkspaceDbContext : IWorkspaceDBContext
	{
		public string ServerName { get; }

		private readonly int _workspaceId;

		public FakeWorkspaceDbContext(int workspaceId)
		{
			_workspaceId = workspaceId;
		}

		public void BeginTransaction()
		{
		}

		public void CommitTransaction()
		{
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement)
		{
			return 1; 
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return 0;
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement)
		{
			throw new System.NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			throw new System.NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return default(T);
		}

		public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
		{
			throw new System.NotImplementedException();
		}

		public IDataReader ExecuteSQLStatementAsReader(string sql)
		{
			throw new System.NotImplementedException();
		}
	}
}