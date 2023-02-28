using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries
{
    public class FakeJobTrackerQueryManager : IJobTrackerQueryManager
    {
        private readonly RelativityInstanceTest _relativity;

        public FakeJobTrackerQueryManager(RelativityInstanceTest relativity)
        {
            _relativity = relativity;
        }

        public ICommand CreateJobTrackingEntry(string tableName, int workspaceId, long jobId)
        {
            return new ActionCommand(() =>
            {
                if (!_relativity.JobTrackerResourceTables.ContainsKey(tableName))
                {
                    _relativity.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
                }

                if (!_relativity.JobTrackerResourceTables[tableName].Exists(x => x.JobId == jobId))
                {
                    _relativity.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = jobId });
                }
            });
        }

        public IQuery<DataTable> GetJobIdsFromTrackingEntry(string tableName, int workspaceId, long rootJobId)
        {
            DataTable dt = new DataTable();

            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn() {ColumnName = "JobID", DataType = typeof(long)}
            });

            foreach (JobTrackerTest jobTrackerRow in _relativity.JobTrackerResourceTables[tableName])
            {
                DataRow row = dt.NewRow();
                row["JobID"] = jobTrackerRow.JobId;

                dt.Rows.Add(row);
            }

            return new ValueReturnQuery<DataTable>(dt);
        }

        public IQuery<int> RemoveEntryAndCheckBatchStatus(string tableName, int workspaceId, long jobId, bool isBatchFinished)
        {
            int result = 0;
            if (_relativity.JobTrackerResourceTables.IsNullOrEmpty())
            {
                return new ValueReturnQuery<int>(0);
            }

            if (isBatchFinished)
            {
                JobTrackerTest jobBatch = _relativity.JobTrackerResourceTables[tableName].Single(x => x.JobId == jobId);
                jobBatch.Completed = true;
            }

            if (_relativity.JobTrackerResourceTables[tableName].Any(x => x.Completed == false))
            {
                result = 1;
            }

            return new ValueReturnQuery<int>(result);
        }
    }
}
