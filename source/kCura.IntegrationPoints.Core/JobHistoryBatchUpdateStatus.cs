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

namespace kCura.IntegrationPoints.Core
{
	public class JobHistoryBatchUpdateStatus : IBatchStatus
	{
		private readonly IJobStatusUpdater _updater;
		private readonly ISerializer _serializer;
		private readonly IRSAPIService _service;

		public JobHistory JobHistory { set; get; }

		public JobHistoryBatchUpdateStatus(IJobStatusUpdater jobStatusUpdater, ISerializer serializer, IRSAPIService rsapiService)
		{
			_updater = jobStatusUpdater;
			_serializer = serializer;
			_service = rsapiService;
		}

		public void OnJobStart(Job job)
		{
			var result = GetHistory(job);
			result.JobStatus = JobStatusChoices.JobHistoryProcessing;
			_service.JobHistoryLibrary.Update(result);
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
				query.Fields = new List<FieldValue> { new FieldValue(Guid.Parse(JobHistoryFieldGuids.ItemsWithErrors)) };
				query.Condition = new TextCondition(Guid.Parse(JobHistoryFieldGuids.BatchInstance), TextConditionEnum.EqualTo, taskParameters.BatchInstance.ToString());
				var result = _service.JobHistoryLibrary.Query(query).First();
				JobHistory = result;
			}
			return JobHistory;
		}

	}
}
