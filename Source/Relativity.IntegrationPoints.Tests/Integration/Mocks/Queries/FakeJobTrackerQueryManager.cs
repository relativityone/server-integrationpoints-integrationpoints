using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
				if(!_relativity.JobTrackerResourceTables.ContainsKey(tableName))
				{
					_relativity.JobTrackerResourceTables[tableName] = new List<JobTrackerTest>();
				}

				if(!_relativity.JobTrackerResourceTables[tableName].Exists(x => x.JobId == jobId))
				{
					_relativity.JobTrackerResourceTables[tableName].Add(new JobTrackerTest { JobId = jobId });
				}
			});
		}

		public IQuery<DataTable> GetProcessingSyncWorkerBatches(string tableName, int workspaceId, long rootJobId)
		{
			DataTable dt = new DataTable();

			dt.Columns.AddRange(new DataColumn[]
			{
				new DataColumn("LockedByAgentId", typeof(int)) { AllowDBNull = true },
				new DataColumn("StopState", typeof(int)),
			});

			var processingSyncWorkerBatches = _relativity.JobsInQueue.Join(_relativity.JobTrackerResourceTables[tableName],
				x => x.JobId, x => x.JobId, (job, batch) => new { LockedByAgentId = job.LockedByAgentID, StopState = job.StopState });
			foreach(var processingBatch in processingSyncWorkerBatches)
			{
				DataRow row = dt.NewRow();
				row["LockedByAgentId"] = processingBatch.LockedByAgentId;
				row["StopState"] = processingBatch.StopState;

				dt.ImportRow(row);
			}

			return new ValueReturnQuery<DataTable>(dt);
		}

		public IQuery<int> RemoveEntryAndCheckBatchStatus(string tableName, int workspaceId, long jobId, bool isBatchFinished)
		{
			if(isBatchFinished)
			{
				JobTrackerTest jobBatch = _relativity.JobTrackerResourceTables[tableName].Single(x => x.JobId == jobId);
				jobBatch.Completed = true;
			}

			int result;
			if (_relativity.JobTrackerResourceTables[tableName].Any(x => x.Completed == false))
			{
				result = 1;
			}
			else
			{
				_relativity.JobTrackerResourceTables.Remove(tableName);
				result = 0;
			}

			return new ValueReturnQuery<int>(result);
		}
	}
}
