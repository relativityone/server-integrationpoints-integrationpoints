using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.API.Context;

namespace Relativity.Sync.DbContext
{
    internal class EddsDbContext : IEddsDbContext
    {
        private readonly IDBContext _context;

        public EddsDbContext(IDBContext context)
        {
            _context = context;
        }

        public SqlConnection GetConnection()
        {
            return _context.GetConnection();
        }

        public DbParameter CreateDbParameter()
        {
            return _context.CreateDbParameter();
        }

        public SqlConnection GetConnection(bool openConnectionIfClosed)
        {
            return _context.GetConnection(openConnectionIfClosed);
        }

        public SqlTransaction GetTransaction()
        {
            return _context.GetTransaction();
        }

        public void BeginTransaction()
        {
            _context.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _context.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            _context.RollbackTransaction();
        }

        public void RollbackTransaction(Exception originatingException)
        {
            _context.RollbackTransaction(originatingException);
        }

        public void ReleaseConnection()
        {
            _context.ReleaseConnection();
        }

        public void Cancel()
        {
            _context.Cancel();
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement)
        {
            return _context.ExecuteSqlStatementAsDataTable(sqlStatement);
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsDataTable(sqlStatement, timeoutValue);
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteSqlStatementAsDataTable(sqlStatement, parameters);
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteSqlStatementAsDataTable(sqlStatement, timeoutValue, parameters);
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement)
        {
            return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement);
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters);
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, timeoutValue);
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters, timeoutValue);
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, params SqlParameter[] parameters)
        {
            return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters);
        }

        public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
        {
            return _context.ExecuteSqlStatementAsScalar(sqlStatement, parameters);
        }

        public object ExecuteSqlStatementAsScalar(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsScalar(sqlStatement, parameters, timeoutValue);
        }

        public object ExecuteSqlStatementAsScalarWithInnerTransaction(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsScalarWithInnerTransaction(sqlStatement, parameters, timeoutValue);
        }

        public int ExecuteNonQuerySQLStatement(string sqlStatement)
        {
            return _context.ExecuteNonQuerySQLStatement(sqlStatement);
        }

        public int ExecuteNonQuerySQLStatement(string sqlStatement, int timeoutValue)
        {
            return _context.ExecuteNonQuerySQLStatement(sqlStatement, timeoutValue);
        }

        public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteNonQuerySQLStatement(sqlStatement, parameters);
        }

        public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
        {
            return _context.ExecuteNonQuerySQLStatement(sqlStatement, parameters, timeoutValue);
        }

        public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement)
        {
            return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement);
        }

        public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement, timeoutValue);
        }

        public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, IEnumerable<DbParameter> parameters)
        {
            return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement, parameters);
        }

        public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, IEnumerable<DbParameter> parameters, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement, parameters, timeoutValue);
        }

        public DataTable ExecuteSQLStatementGetSecondDataTable(string sqlStatement, int timeout = -1)
        {
            return _context.ExecuteSQLStatementGetSecondDataTable(sqlStatement, timeout);
        }

        public SqlDataReader ExecuteSQLStatementAsReader(string sqlStatement, int timeout = -1)
        {
            return _context.ExecuteSQLStatementAsReader(sqlStatement, timeout);
        }

        public IEnumerable<T> ExecuteSQLStatementAsEnumerable<T>(string sqlStatement, Func<SqlDataReader, T> converter, int timeout = -1)
        {
            return _context.ExecuteSQLStatementAsEnumerable(sqlStatement, converter, timeout);
        }

        public DbDataReader ExecuteProcedureAsReader(string procedureName, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteProcedureAsReader(procedureName, parameters);
        }

        public int ExecuteProcedureNonQuery(string procedureName, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteProcedureNonQuery(procedureName, parameters);
        }

        public SqlDataReader ExecuteParameterizedSQLStatementAsReader(string sqlStatement, IEnumerable<SqlParameter> parameters,
            int timeoutValue = -1, bool sequentialAccess = false)
        {
            return _context.ExecuteParameterizedSQLStatementAsReader(sqlStatement, parameters,
                timeoutValue, sequentialAccess);
        }

        public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement)
        {
            return _context.ExecuteSqlStatementAsDataSet(sqlStatement);
        }

        public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return _context.ExecuteSqlStatementAsDataSet(sqlStatement, parameters);
        }

        public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsDataSet(sqlStatement, timeoutValue);
        }

        public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
        {
            return _context.ExecuteSqlStatementAsDataSet(sqlStatement, parameters, timeoutValue);
        }

        public void ExecuteSqlBulkCopy(IDataReader dataReader, ISqlBulkCopyParameters bulkCopyParameters)
        {
            _context.ExecuteSqlBulkCopy(dataReader, bulkCopyParameters);
        }

        public Task<IDbConnection> GetConnectionAsync(CancellationToken cancelToken)
        {
            return _context.GetConnectionAsync(cancelToken);
        }

        public Task BeginTransactionAsync(CancellationToken cancelToken)
        {
            return _context.BeginTransactionAsync(cancelToken);
        }

        public Task ExecuteBulkCopyAsync(IDataReader source, ISqlBulkCopyParameters parameters, CancellationToken cancelToken)
        {
            return _context.ExecuteBulkCopyAsync(source, parameters, cancelToken);
        }

        public Task<DataTable> ExecuteDataTableAsync(IQuery query)
        {
            return _context.ExecuteDataTableAsync(query);
        }

        public Task<IDataReader> ExecuteReaderAsync(IQuery query)
        {
            return _context.ExecuteReaderAsync(query);
        }

        public Task<int> ExecuteNonQueryAsync(IQuery query)
        {
            return _context.ExecuteNonQueryAsync(query);
        }

        public Task<T> ExecuteObjectAsync<T>(IQuery query, Func<IDataReader, CancellationToken, Task<T>> converter)
        {
            return _context.ExecuteObjectAsync(query, converter);
        }

        public Task<T> ExecuteScalarAsync<T>(IQuery query)
        {
            return _context.ExecuteScalarAsync<T>(query);
        }

        public Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(IQuery query, Func<IDataRecord, CancellationToken, Task<T>> converter)
        {
            return _context.ExecuteEnumerableAsync(query, converter);
        }

        public string Database => _context.Database;

        public string ServerName => _context.ServerName;

        public bool IsMasterDatabase => _context.IsMasterDatabase;
    }
}
