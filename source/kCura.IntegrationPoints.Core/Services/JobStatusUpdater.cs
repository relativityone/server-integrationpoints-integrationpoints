using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Choice = kCura.Relativity.Client.Choice;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services
{
	public class JobStatusUpdater : IJobStatusUpdater
	{
		private readonly GetRecentJobHistory _service;
		public JobStatusUpdater(GetRecentJobHistory service)
		{
			_service = service;
		}

		public Choice GenerateStatus(JobHistory jobHistory)
		{
			if (jobHistory == null)
			{
				throw new ArgumentNullException("jobHistory");
			}

			//TODO: check to see if this is the last job
			//if(lastJob){
			// return JobStatusChoices.JobHistoryProcessing
			//}

			var recent = _service.Execute(jobHistory.ArtifactId);
			if (recent != null)
			{
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorItem))
				{
					return Data.JobStatusChoices.JobHistoryCompletedWithErrors;
				}
				if (recent.ErrorType.EqualsToChoice(Data.ErrorTypeChoices.JobHistoryErrorItem))
				{
					return Data.JobStatusChoices.JobHistoryErrorJobFailed;
				}
			}
			return Data.JobStatusChoices.JobHistoryCompleted;
		}
	}
}
