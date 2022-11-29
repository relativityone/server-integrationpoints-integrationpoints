using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using kCura.IntegrationPoints.Data.DbContext;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeWorkspaceDbContext : IWorkspaceDBContext
    {
        private readonly RelativityInstanceTest _relativityInstance;

        public FakeWorkspaceDbContext(RelativityInstanceTest relativityInstance)
        {
            _relativityInstance = relativityInstance;
        }

        public string ServerName { get; }

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
            return new DataTable();
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            DataTable dataTable = new DataTable();
            if (sqlStatement.Contains("select q.LockedByAgentID, q.StopState"))
            {
                dataTable.Columns.AddRange(new DataColumn[] { new DataColumn("LockedByAgentID", typeof(int)), new DataColumn("StopState", typeof(int)) });

                foreach (JobTest job in _relativityInstance.JobsInQueue)
                {
                    dataTable.Rows.Add(job.LockedByAgentID, (int)job.StopState);
                }
            }

            return dataTable;
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            return default(T);
        }

        public object ExecuteSqlStatementAsScalar(string sqlStatement, params SqlParameter[] parameters)
        {
            if (sqlStatement.Contains("IF @batchIsFinished = 1"))
            {
                int jobId = (int)(long)parameters.First(p => p.ParameterName == "@jobID").Value;

                return _relativityInstance.JobsInQueue.Any(x => x.JobId != jobId) ? 1 : 0;
            }

            return null;
        }

        public IDataReader ExecuteSQLStatementAsReader(string sql)
        {
            throw new NotImplementedException();
        }

        public SqlConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        public DbDataReader ExecuteSqlStatementAsDbDataReader(string sqlStatement)
        {
            throw new NotImplementedException();
        }

        public T ExecuteSqlStatementAsScalar<T>(string sqlStatement)
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public SqlDataReader ExecuteSQLStatementAsReader(string sqlStatement, int timeout = -1)
        {
            throw new NotImplementedException();
        }
    }
}
