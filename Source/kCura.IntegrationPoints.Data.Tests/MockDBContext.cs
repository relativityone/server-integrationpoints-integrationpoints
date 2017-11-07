using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests
{
	public class MockDBContext : IDBContext
	{
		public const int DEFAULT_TIMEOUT = 600;
		private readonly string _connectionString;
		public MockDBContext(string connectionString)
		{
			_connectionString = connectionString;
		}

		public SqlConnection GetConnection()
		{
			return new SqlConnection(_connectionString);
		}

		public DbParameter CreateDbParameter()
		{
			throw new NotImplementedException();
		}

		public SqlConnection GetConnection(bool openConnectionIfClosed)
		{
			var cmd = GetConnection();
			if (cmd.State != ConnectionState.Open)
			{
				cmd.Open();
			}
			return cmd;
		}

		public SqlTransaction GetTransaction()
		{
			throw new NotImplementedException();
		}

		public void BeginTransaction()
		{
			throw new NotImplementedException();
		}

		public void CommitTransaction()
		{
			throw new NotImplementedException();
		}

		public void RollbackTransaction()
		{
			throw new NotImplementedException();
		}

		public void RollbackTransaction(Exception originatingException)
		{
			throw new NotImplementedException();
		}

		public void ReleaseConnection()
		{
			throw new NotImplementedException();
		}

		public void Cancel()
		{
			throw new NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement)
		{
			return this.ExecuteSqlStatementAsDataTable(sqlStatement, DEFAULT_TIMEOUT, new List<SqlParameter>());
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue)
		{
			return this.ExecuteSqlStatementAsDataTable(sqlStatement, timeoutValue, new List<SqlParameter>());
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return this.ExecuteSqlStatementAsDataTable(sqlStatement, DEFAULT_TIMEOUT, parameters);
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue, IEnumerable<SqlParameter> parameters)
		{
			using (var con = this.GetConnection(true))
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = sqlStatement;
					cmd.CommandTimeout = timeoutValue;
					cmd.Parameters.AddRange(parameters.ToArray());
					var dt = new DataTable();
					dt.Load(cmd.ExecuteReader());
					return dt;
				}
			}
		}

		
		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement)
		{
			return this.ExecuteSqlStatementAsScalar<T>(sqlStatement, new List<SqlParameter>(), DEFAULT_TIMEOUT);
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return this.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters, DEFAULT_TIMEOUT);
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, int timeoutValue)
		{
			return this.ExecuteSqlStatementAsScalar<T>(sqlStatement, new List<SqlParameter>(), timeoutValue);
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			return (T)this.ExecuteSqlStatementAsScalar(sqlStatement, parameters, timeoutValue);
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, params SqlParameter[] parameters)
		{
			return this.ExecuteSqlStatementAsScalar<T>(sqlStatement, parameters, DEFAULT_TIMEOUT);
		}

		public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
		{
			return this.ExecuteSqlStatementAsScalar(sqlStatement, parameters, DEFAULT_TIMEOUT);
		}

		public object ExecuteSqlStatementAsScalar(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			using (var con = this.GetConnection(true))
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = sqlStatement;
					cmd.Parameters.AddRange(parameters.ToArray());
					cmd.CommandTimeout = timeoutValue;
					return cmd.ExecuteScalar();
				}
			}
		}

		public object ExecuteSqlStatementAsScalarWithInnerTransaction(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement)
		{
			using (var con = this.GetConnection(true))
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = sqlStatement;
					return cmd.ExecuteNonQuery();
				}
			}
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, int timeoutValue)
		{
			return this.ExecuteNonQuerySQLStatement(sqlStatement, new List<SqlParameter>(), DEFAULT_TIMEOUT);
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			return this.ExecuteNonQuerySQLStatement(sqlStatement, parameters, DEFAULT_TIMEOUT);
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			using (var con = this.GetConnection(true))
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = sqlStatement;
					cmd.Parameters.AddRange(parameters.ToArray());
					cmd.CommandTimeout = timeoutValue;
					return cmd.ExecuteNonQuery();
				}
			}
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, IEnumerable<DbParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, IEnumerable<DbParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DataTable ExecuteSQLStatementGetSecondDataTable(string sqlStatement, int timeout = -1)
		{
			throw new NotImplementedException();
		}

		public SqlDataReader ExecuteSQLStatementAsReader(string sqlStatement, int timeout = -1)
		{
			throw new NotImplementedException();
		}

	    public IEnumerable<T> ExecuteSQLStatementAsEnumerable<T>(string sqlStatement, Func<SqlDataReader, T> converter, int timeout = -1)
	    {
	        throw new NotImplementedException();
	    }

	    public DbDataReader ExecuteProcedureAsReader(string procedureName, IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public int ExecuteProcedureNonQuery(string procedureName, IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public SqlDataReader ExecuteParameterizedSQLStatementAsReader(string sqlStatement, IEnumerable<SqlParameter> parameters,
			int timeoutValue = -1, bool sequentialAccess = false)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public string Database { get; private set; }
		public string ServerName { get; private set; }
		public bool IsMasterDatabase { get; private set; }
	}
}
