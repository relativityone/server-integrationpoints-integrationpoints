using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Core
{
	public class JobHistoryBatchUpdateStatus : IBatchStatus
	{
		private readonly IJobStatusUpdater _updater;
		private readonly IJobService _jobService;
		private readonly ISerializer _serializer;
		private readonly IRSAPIService _service;

		public JobHistory JobHistory { set; get; }

		public JobHistoryBatchUpdateStatus(IJobStatusUpdater jobStatusUpdater, IJobService jobService, ISerializer serializer, IRSAPIService rsapiService)
		{
			_updater = jobStatusUpdater;
			_jobService = jobService;
			_serializer = serializer;
			_service = rsapiService;
		}

		public void OnJobStart(Job job)
		{
			var updatedJob = _jobService.GetJob(job.JobId);
			if (updatedJob.StopState == StopState.None)
			{
				var result = GetHistory(job);
				result.JobStatus = JobStatusChoices.JobHistoryProcessing;
				_service.JobHistoryLibrary.Update(result);
			}
		}

		public void OnJobComplete(Job job)
		{
			var result = GetHistory(job);
			result.JobStatus = _updater.GenerateStatus(result);
			result.EndTimeUTC = DateTime.UtcNow;
			_service.JobHistoryLibrary.Update(result);
		}

		private JobHistory GetHistory(Job job)
		{
			if (JobHistory == null)
			{
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				var query = new Query<RDO>();
				query.Fields = new List<FieldValue> { new FieldValue(Guid.Parse(JobHistoryFieldGuids.ItemsWithErrors)), new FieldValue(Guid.Parse(JobHistoryFieldGuids.JobStatus)) };
				query.Condition = new TextCondition(Guid.Parse(JobHistoryFieldGuids.BatchInstance), TextConditionEnum.EqualTo, taskParameters.BatchInstance.ToString());
				JobHistory result = _service.JobHistoryLibrary.Query(query).First();
				JobHistory = result;
			}
			return JobHistory;
		}
	}
}