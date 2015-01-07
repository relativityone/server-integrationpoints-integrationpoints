using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Installers
{
	public class Context : IDBContext
	{
		public SqlConnection GetConnection()
		{
			throw new NotImplementedException();
		}

		public DbParameter CreateDbParameter()
		{
			throw new NotImplementedException();
		}

		public SqlConnection GetConnection(bool openConnectionIfClosed)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement)
		{
			throw new NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, int timeoutValue)
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

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public string Database { get; private set; }
		public string ServerName { get; private set; }
		public bool IsMasterDatabase { get; private set; }

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, IEnumerable parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public SqlDataReader ExecuteParameterizedSQLStatementAsReader(string sqlStatement, IEnumerable parameters,
			int timeoutValue = -1, bool sequentialAccess = false)
		{
			throw new NotImplementedException();
		}

		public int ExecuteProcedureNonQuery(string procedureName, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteProcedureAsReader(string procedureName, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, IEnumerable parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public object ExecuteSqlStatementAsScalarWithInnerTransaction(string sqlStatement, IEnumerable parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public object ExecuteSqlStatementAsScalar(string sqlStatement, IEnumerable parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, params SqlParameter[] parameters)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable parameters)
		{
			throw new NotImplementedException();
		}


		public int ExecuteNonQuerySQLStatement(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public int ExecuteNonQuerySQLStatement(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public SqlDataReader ExecuteParameterizedSQLStatementAsReader(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters, int timeoutValue = -1, bool sequentialAccess = false)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteProcedureAsReader(string procedureName, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public int ExecuteProcedureNonQuery(string procedureName, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DataSet ExecuteSqlStatementAsDataSet(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, int timeoutValue, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, System.Collections.Generic.IEnumerable<DbParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement, System.Collections.Generic.IEnumerable<DbParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public object ExecuteSqlStatementAsScalar(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}

		public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public object ExecuteSqlStatementAsScalarWithInnerTransaction(string sqlStatement, System.Collections.Generic.IEnumerable<SqlParameter> parameters, int timeoutValue)
		{
			throw new NotImplementedException();
		}
	}

	[TestFixture]
	public class ServiceInstallerTests
	{
		[Test]
		[Explicit]
		public void Test()
		{
			IWindsorContainer container = new WindsorContainer();
			container.Register(Component.For<IDBContext>().UsingFactoryMethod<IDBContext>((x) => new Context()));
			var installer = new Core.Installers.ServicesInstaller();
			installer.Install(container, null);

			var result = container.Resolve<IAgentService>();

		}
	}
}
