using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Handlers;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.DbContext
{
    public abstract class RipDbContextBase : IRipDBContext
    {
        private readonly IDBContext _context;
        private readonly IRetryHandler _retryHandler;

        public RipDbContextBase(IDBContext context, IRetryHandlerFactory retryHandlerFactory)
        {
            _context = context;
            _retryHandler = retryHandlerFactory.Create();
        }

        public string ServerName => _context.ServerName;

        public void BeginTransaction()
        {
            _retryHandler.ExecuteWithRetries(
                () => _context.BeginTransaction());
        }

        public void CommitTransaction()
        {
            _retryHandler.ExecuteWithRetries(
                () => _context.CommitTransaction());
        }

        public int ExecuteNonQuerySQLStatement(string sqlStatement)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteNonQuerySQLStatement(sqlStatement));
        }

        public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteNonQuerySQLStatement(sqlStatement, parameters));
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteSqlStatementAsDataTable(sqlStatement));
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteSqlStatementAsDataTable(sqlStatement, parameters));
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters));
        }

        public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteSqlStatementAsScalar(sqlStatement, parameters));
        }

        public IDataReader ExecuteSQLStatementAsReader(string sql)
        {
            return _retryHandler.ExecuteWithRetries(
                () => _context.ExecuteSQLStatementAsReader(sql));
        }
    }
}
