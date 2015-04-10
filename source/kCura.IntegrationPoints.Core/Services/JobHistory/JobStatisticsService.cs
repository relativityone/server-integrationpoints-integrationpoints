using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class NullReporter : IBatchReporter
	{
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;
	}

	public class JobStatisticsService
	{
		private readonly JobStatisticsQuery _query;
		private Job _job;
		private readonly TaskParameterHelper _helper;
		private readonly JobHistoryService _service;
		public JobStatisticsService(JobStatisticsQuery query, TaskParameterHelper helper, JobHistoryService service)
		{
			_query = query;
			_helper = helper;
			_service = service;
		}

		public void Subscribe(IBatchReporter reporter, Job job)
		{
			if (reporter == null)
			{
				reporter = new NullReporter();
			}
			_job = job;
			reporter.OnBatchComplete += this.JobComplete;
		}
		private void JobComplete(DateTime start, DateTime end, int total, int errorCount)
		{
			var tableName = JobTracker.GenerateTableTempTableName(_job, _helper.GetBatchInstance(_job).ToString());
			var stats = _query.UpdateAndRetreiveStats(tableName, _job.JobId, new JobStatistics { Completed = total, Errored = errorCount });
			var historyRdo = _service.GetRDO(_helper.GetBatchInstance(_job));
			historyRdo.RecordsImported = stats.Imported;
			historyRdo.RecordsWithErrors = stats.Errored;
			_service.UpdateRDO(historyRdo);
		}

	}
}
