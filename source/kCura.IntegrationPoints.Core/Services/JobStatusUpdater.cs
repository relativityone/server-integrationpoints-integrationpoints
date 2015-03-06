using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Choice = kCura.Relativity.Client.Choice;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobStatusUpdater : IJobStatusUpdater
	{
		private readonly JobHistoryErrorQuery _service;
		private readonly IRSAPIService _rsapiService;
		public JobStatusUpdater(JobHistoryErrorQuery service, IRSAPIService rsapiService)
		{
			_service = service;
			_rsapiService = rsapiService;
		}

		public Choice GenerateStatus(Guid batchId)
		{
			var query = new Query<RDO>();
			query.Fields = new List<FieldValue>();
			query.Condition = new TextCondition(Guid.Parse(JobHistoryFieldGuids.BatchInstance), TextConditionEnum.EqualTo, batchId.ToString());
			var result = _rsapiService.JobHistoryLibrary.Query(query).First();
			return GenerateStatus(result);
		}

		public Choice GenerateStatus(JobHistory jobHistory)
		{
			if (jobHistory == null)
			{
				throw new ArgumentNullException("jobHistory");
			}
			var recent = _service.GetJobErrorFailedStatus(jobHistory.ArtifactId);
			if (recent != null)
			{
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorItem))
				{
					return Data.JobStatusChoices.JobHistoryCompletedWithErrors;
				}
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorJob))
				{
					return Data.JobStatusChoices.JobHistoryErrorJobFailed;
				}
			}
			return Data.JobStatusChoices.JobHistoryCompleted;
		}
	}
}
