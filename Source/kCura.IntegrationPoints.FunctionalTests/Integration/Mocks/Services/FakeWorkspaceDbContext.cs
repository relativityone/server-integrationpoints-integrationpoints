using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data;
using OutsideIn.Options;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeWorkspaceDbContext : IWorkspaceDBContext
    {
        public string ServerName { get; }

        private readonly int _workspaceId;
        private readonly RelativityInstanceTest _relativityInstance;

        public FakeWorkspaceDbContext(int workspaceId, RelativityInstanceTest relativityInstance)
        {
            _workspaceId = workspaceId;
            _relativityInstance = relativityInstance;
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
            return new DataTable();
        }

        public DataTable ExecuteSqlStatementAsDataTable(string sqlStatement, IEnumerable<SqlParameter> parameters)
        {
            DataTable dataTable = new DataTable();
            if (sqlStatement.Contains("select q.LockedByAgentID, q.StopState")) // GetProcessingSyncWorkerBatches.sql
            {
                dataTable.Columns.AddRange(new DataColumn[]{new DataColumn("LockedByAgentID", typeof(int)), new DataColumn("StopState", typeof(int))});

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
            if (sqlStatement.Contains("IF @batchIsFinished = 1")) // RemoveEntryAndCheckBatchStatus.sql
            {
                // return number of other jobs from queue
                int jobId = (int)(Int64)parameters.First(p => p.ParameterName == "@jobID").Value;

                return _relativityInstance.JobsInQueue.Any(x => x.JobId != jobId) ? 1 : 0;
            }

            return null;
        }

        public IDataReader ExecuteSQLStatementAsReader(string sql)
        {
            throw new System.NotImplementedException();
        }
    }
}