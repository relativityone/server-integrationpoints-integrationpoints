using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;
using Relativity.API.Context;
using Relativity.Data;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class TestDbContext : IDBContext
	{
		private readonly Data.RowDataGateway.BaseContext _context;

		/// <summary>
		/// Initializes a new instance of the DBContext class.
		/// </summary>
		/// <param name="context">Predefined context object based on which DBContext instance is created.</param>
		public TestDbContext(kCura.Data.RowDataGateway.BaseContext context)
		{
			_context = context;
		}

		#region "Connection"
		/// <summary>
		/// Gets a database connection.
		/// </summary>
		/// <returns>Returns a database connection.</returns>
		public SqlConnection GetConnection()
		{
			return _context.GetConnection();
		}

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> converter, IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a database name.
		/// </summary>
		public String Database
		{
			get
			{
				return _context.Database;
			}
		}

		/// <summary>
		/// Gets a database server name.
		/// </summary>
		public String ServerName
		{
			get
			{
				return _context.ServerName;
			}
		}

		/// <summary>
		/// Checks if database is Master context
		/// </summary>
		public Boolean IsMasterDatabase
		{
			get
			{
				return _context.IsMasterDatabase;
			}
		}

		/// <summary>
		/// Returns a database Parameter
		/// </summary>
		/// <returns>returns a newly created DbParameter object</returns>
		public System.Data.Common.DbParameter CreateDbParameter()
		{
			return _context.CreateDbParameter();
		}

		/// <summary>
		/// Gets a database connection, with option to open if closed.
		/// </summary>
		/// <param name="openConnectionIfClosed">Indicates whether to open the connection if closed</param>
		/// <returns>Returns a database connection.</returns>
		public SqlConnection GetConnection(Boolean openConnectionIfClosed)
		{
			return _context.GetConnection(openConnectionIfClosed);
		}

		/// <summary>
		/// Gets a database transaction.
		/// </summary>
		/// <returns>Returns a database transaction.</returns>
		public SqlTransaction GetTransaction()
		{
			return _context.GetTransaction();
		}

		/// <summary>
		/// Starts a database transaction.
		/// </summary>
		public void BeginTransaction()
		{
			_context.BeginTransaction();
		}

		/// <summary>
		/// Commits a database transaction.
		/// </summary>
		public void CommitTransaction()
		{
			_context.CommitTransaction();
		}

		/// <summary>
		/// Rolls back a database transaction.
		/// </summary>
		public void RollbackTransaction()
		{
			_context.RollbackTransaction();
		}

		/// <summary>
		/// Rolls back a database transaction.  Pass in inner exception if rollback times out.
		/// </summary>
		/// <param name="originatingException">The exception details that caused this rollback to be called</param>
		public void RollbackTransaction(System.Exception originatingException)
		{
			_context.RollbackTransaction(originatingException);
		}

		/// <summary>
		/// Releases a database connection.
		/// </summary>
		public void ReleaseConnection()
		{
			_context.ReleaseConnection();
		}

		/// <summary>
		/// Tries to cancel a SQL command execution.
		/// </summary>
		public void Cancel()
		{
			_context.Cancel();
		}

		#endregion

		#region "Execution"

		#region Execute DataTable
		/// <summary>
		/// Executes a Transact-SQL statement agains the connection and returns a table of in-memory data.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <returns>Returns a table of in-memory data.</returns>
		public System.Data.DataTable ExecuteSqlStatementAsDataTable(string sqlStatement)
		{
			return _context.ExecuteSqlStatementAsDataTable(sqlStatement);
		}

		/// <summary>
		/// Executes a Transact-SQL statement agains the connection and returns a table of in-memory data.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns a table of in-memory data.</returns>
		public System.Data.DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsDataTable(sqlStatement, timeoutValue);
		}

		/// <summary>
		/// Executes a parameterized Transact-SQL statement against the connection and returns a table of in-memory data.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns a table of in-memory data.</returns>
		public System.Data.DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<System.Data.SqlClient.SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsDataTable(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes a Transact-SQL statement agains the connection and returns a table of in-memory data.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns a table of in-memory data.</returns>
		public System.Data.DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, Int32 timeoutValue, IEnumerable<System.Data.SqlClient.SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsDataTable(sqlStatement, parameters, timeoutValue);
		}
		#endregion

		#region Execute Scalar
		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <typeparam name="T">Type used to define returned object.</typeparam>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement)
		{
			return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <typeparam name="T">Type used to define returned object.</typeparam>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<System.Data.SqlClient.SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <typeparam name="T">Type used to define returned object.</typeparam>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, timeoutValue);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <typeparam name="T">Type used to define returned object.</typeparam>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<System.Data.SqlClient.SqlParameter> parameters, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters, timeoutValue);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <typeparam name="T">Type used to define returned object.</typeparam>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, params SqlParameter[] parameters)
		{
			return _context.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
		{
			return _context.ExecuteSqlStatementAsScalar(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public object ExecuteSqlStatementAsScalar(string sqlStatement, IEnumerable<System.Data.SqlClient.SqlParameter> parameters, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsScalar(sqlStatement, parameters, timeoutValue);
		}

		/// <summary>
		/// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
		/// ExecuteSqlStatementAsScalarWithInnerTransaction method can be called if context.transaction is not set
		/// This method opens transaction internally. This functionality is implemented to enable query rerun in case on the problems
		/// with Sql optimizer hints
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns the first column of the first row in the result set returned by the query.</returns>
		public object ExecuteSqlStatementAsScalarWithInnerTransaction(string sqlStatement, IEnumerable<System.Data.SqlClient.SqlParameter> parameters, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsScalar(sqlStatement, parameters, timeoutValue);
		}
		#endregion

		#region "Execute Non-Query"
		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <returns>Returns the number of rows affected.</returns>
		public Int32 ExecuteNonQuerySQLStatement(String sqlStatement)
		{
			return _context.ExecuteNonQuerySQLStatement(sqlStatement);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns the number of rows affected.</returns>
		public Int32 ExecuteNonQuerySQLStatement(String sqlStatement, Int32 timeoutValue)
		{
			return _context.ExecuteNonQuerySQLStatement(sqlStatement, timeoutValue);
		}

		/// <summary>
		/// Executes a parameterized Transact-SQL statement against the connection and returns the number of rows affected.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns the number of rows affected.</returns>
		public Int32 ExecuteNonQuerySQLStatement(String sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteNonQuerySQLStatement(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns the number of rows affected.</returns>
		public Int32 ExecuteNonQuerySQLStatement(String sqlStatement, IEnumerable<SqlParameter> parameters, Int32 timeoutValue)
		{
			return _context.ExecuteNonQuerySQLStatement(sqlStatement, parameters, timeoutValue);
		}
		#endregion

		#region Execute DbDataReader
		/// <summary>
		/// Executes a Transact-SQL statement against the connection and builds a DbDataReader.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <returns>Returns a DbDataReader object.</returns>
		public System.Data.Common.DbDataReader ExecuteSqlStatementAsDbDataReader(String sqlStatement)
		{
			return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and builds a DbDataReader.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns a DbDataReader object.</returns>
		public System.Data.Common.DbDataReader ExecuteSqlStatementAsDbDataReader(String sqlStatement, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement, timeoutValue);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and builds a DbDataReader.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <returns>Returns a DbDataReader object.</returns>
		public System.Data.Common.DbDataReader ExecuteSqlStatementAsDbDataReader(String sqlStatement, IEnumerable<System.Data.Common.DbParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and builds a DbDataReader.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		/// <returns>Returns a DbDataReader object.</returns>
		public System.Data.Common.DbDataReader ExecuteSqlStatementAsDbDataReader(String sqlStatement, IEnumerable<System.Data.Common.DbParameter> parameters, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsDbDataReader(sqlStatement, parameters, timeoutValue);
		}
		#endregion

		/// <summary>
		/// Executes a Transact-SQL statement agains the connection and returns a table of in-memory data.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="timeout">A timeout value in seconds for the query</param>
		/// <returns>Returns a table of in-memory data.</returns>
		public System.Data.DataTable ExecuteSQLStatementGetSecondDataTable(String sqlStatement, Int32 timeout = -1)
		{
			return _context.ExecuteSQLStatementGetSecondDataTable(sqlStatement, timeout);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and builds a DbDataReader.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="timeout">A timeout value in seconds for the query</param>
		/// <returns>Returns a SqlDataReader object.</returns>
		public System.Data.SqlClient.SqlDataReader ExecuteSQLStatementAsReader(String sqlStatement, Int32 timeout = -1)
		{
			return _context.ExecuteSQLStatementAsReader(sqlStatement, timeout);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns the result as an <see cref="IEnumerable{T}"/> using the converter delegate.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="converter">Delegate to retrieve item from <see cref="System.Data.SqlClient.SqlDataReader"/> </param>
		/// <param name="timeout">A timeout value in seconds for the query</param>
		/// <returns>Returns an <see cref="IEnumerable{T}"/>.</returns>
		public IEnumerable<T> ExecuteSQLStatementAsEnumerable<T>(string sqlStatement, Func<System.Data.SqlClient.SqlDataReader, T> converter, Int32 timeout = -1)
		{
			List<T> result = _context.ExecuteSqlStatementAsList(sqlStatement, converter);
			return result;
		}

		public IEnumerable<T> ExecuteSqlStatementAsEnumerable<T>(string sqlStatement, Func<SqlDataReader, T> converter, IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		#region Procedures
		/// <summary>
		/// Executes a stored procedure against the connection and builds a DbDataReader.
		/// </summary>
		/// <param name="procedureName">String value indicating a name of stored procedure to be executed.</param>
		/// <param name="parameters">List of SQL parameters passed in to the stored procedure.</param>
		/// <returns>Returns a DbDataReader object.</returns>
		public System.Data.Common.DbDataReader ExecuteProcedureAsReader(String procedureName, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteProcedureAsReader(procedureName, parameters);
		}

		/// <summary>
		/// Executes a stored procedure against the connection and returns the number of rows affected.
		/// </summary>
		/// <param name="procedureName">String value indicating a name of stored procedure to be executed.</param>
		/// <param name="parameters">List of SQL parameters passed in to the stored procedure.</param>
		/// <returns>Returns the number of rows affected.</returns>
		public Int32 ExecuteProcedureNonQuery(String procedureName, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteProcedureNonQuery(procedureName, parameters);
		}
		#endregion

		/// <summary>
		/// Executes a parameterized Transact-SQL statement against the connection and returns a sqlDataReader.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		///  <param name="sequentialAccess">Determins if command is used with System.Data.CommandBehavior.SequentialAccess</param>
		/// <returns>Returns a SqlDataReader object.</returns>
		public System.Data.SqlClient.SqlDataReader ExecuteParameterizedSQLStatementAsReader(String sqlStatement, IEnumerable<SqlParameter> parameters, Int32 timeoutValue = -1, Boolean sequentialAccess = false)
		{
			return _context.ExecuteParameterizedSQLStatementAsReader(sqlStatement, parameters, timeoutValue, sequentialAccess);
		}

		#region ExecuteSqlStatementAsDataSet
		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns an in-memory cache of data.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		///<returns>Returns an in-memory cache of data.</returns> 
		public System.Data.DataSet ExecuteSqlStatementAsDataSet(String sqlStatement)
		{
			return _context.ExecuteSqlStatementAsDataSet(sqlStatement);
		}

		/// <summary>
		/// Executes a parameterized Transact-SQL statement against the connection and returns an in-memory cache of data.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		///<returns>Returns an in-memory cache of data.</returns>
		public System.Data.DataSet ExecuteSqlStatementAsDataSet(String sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsDataSet(sqlStatement, parameters);
		}

		/// <summary>
		/// Executes a Transact-SQL statement against the connection and returns an in-memory cache of data.
		/// </summary>
		/// <param name="sqlStatement">String containing SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		///<returns>Returns an in-memory cache of data.</returns> 
		public System.Data.DataSet ExecuteSqlStatementAsDataSet(String sqlStatement, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsDataSet(sqlStatement, timeoutValue);
		}

		/// <summary>
		/// Executes a parameterized Transact-SQL statement against the connection and returns an in-memory cache of data.
		/// </summary>
		/// <param name="sqlStatement">String containing parameterized SQL statement.</param>
		/// <param name="parameters">List of SQL parameters passed in to SQL statement.</param>
		/// <param name="timeoutValue">A timeout value in seconds for the query</param>
		///<returns>Returns an in-memory cache of data.</returns>
		public System.Data.DataSet ExecuteSqlStatementAsDataSet(String sqlStatement, IEnumerable<SqlParameter> parameters, Int32 timeoutValue)
		{
			return _context.ExecuteSqlStatementAsDataSet(sqlStatement, parameters, timeoutValue);
		}

		public void ExecuteSqlBulkCopy(IDataReader dataReader, ISqlBulkCopyParameters bulkCopyParameters)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> converter)
		{
			return _context.ExecuteSqlStatementAsObject(sqlStatement, converter);
		}

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> convertor, int timeout = -1)
		{
			return _context.ExecuteSqlStatementAsObject(sqlStatement, convertor, timeout);
		}

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> converter, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsObject(sqlStatement, converter, parameters);
		}

		#endregion
		#endregion

		

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> converter)
		{
			return _context.ExecuteSqlStatementAsObject(sqlStatement, converter);
		}

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> converter, int timeout = -1)
		{
			return _context.ExecuteSqlStatementAsObject(sqlStatement, converter, timeout);
		}

		public T ExecuteSqlStatementAsObject<T>(string sqlStatement, Func<SqlDataReader, T> converter, IEnumerable<SqlParameter> parameters)
		{
			return _context.ExecuteSqlStatementAsObject(sqlStatement, converter, parameters);
		}
	}
}
