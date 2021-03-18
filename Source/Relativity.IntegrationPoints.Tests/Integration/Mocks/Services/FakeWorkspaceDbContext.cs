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
			throw new System.NotImplementedException();
		}

		public void CommitTransaction()
		{
			throw new System.NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement)
		{
			throw new System.NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
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