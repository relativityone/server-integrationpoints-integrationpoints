using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class WorkspaceContext : IWorkspaceDBContext
	{
		private readonly IDBContext _context;

		public WorkspaceContext(IDBContext context)
		{
			_context = context;
		}

		public void BeginTransaction()
		{
			_context.BeginTransaction();
		}

		public void CommitTransaction()
		{
			_context.CommitTransaction();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement)
		{
			return _context.ExecuteNonQuerySQLStatement(sqlStatement);
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteNonQuerySQLStatement(sqlStatement, parameters);
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement)
		{
			return _context.ExecuteSqlStatementAsDataTable(sqlStatement);
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsDataTable(sqlStatement, parameters);
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters);
		}
	}
}