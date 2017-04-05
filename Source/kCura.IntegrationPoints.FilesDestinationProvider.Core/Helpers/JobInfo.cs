
using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
	public class JobInfo : IJobInfo
	{
		private readonly IJobHistoryService _jobHistoryService;
		private readonly Job _job;
		private readonly ISerializer _serializer;

		public JobInfo(IJobHistoryService jobHistoryService, Job job, ISerializer serializer)
		{
			_jobHistoryService = jobHistoryService;
			_job = job;
			_serializer = serializer;
		}

		public DateTime GetStartTimeUtc()
		{
			JobHistory jobHistoryRdo = GetJobHistorRdo();

			return jobHistoryRdo.StartTimeUTC ?? DateTime.UtcNow;
		}

		public string GetName()
		{
			JobHistory jobHistoryRdo = GetJobHistorRdo();
			return jobHistoryRdo.Name;
		}

		private JobHistory GetJobHistorRdo()
		{
			var taskParameters = _serializer.Deserialize<TaskParameters>(_job.JobDetails);
			JobHistory jobHistoryRdo = _jobHistoryService.GetRdo(taskParameters.BatchInstance);
			return jobHistoryRdo;
		}
	}
}
