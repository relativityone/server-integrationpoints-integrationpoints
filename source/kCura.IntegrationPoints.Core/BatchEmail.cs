using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core
{
	public class BatchEmail : IBatchStatus
	{
		private readonly IJobStatusUpdater _updater;
		private readonly ISerializer _serializer;
		private readonly IntegrationPointService _pointService;
		private readonly IJobManager _manager;
		public BatchEmail(IJobStatusUpdater jobStatusUpdater, ISerializer serializer, IntegrationPointService pointService, IJobManager manager)
		{
			_updater = jobStatusUpdater;
			_serializer = serializer;
			_pointService = pointService;
			_manager = manager;
		}

		public void JobStarted(Job job){}

		public void JobComplete(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			var choice = _updater.GenerateStatus(taskParameters.BatchInstance);
			var emails = _pointService.GetRecipientEmails(job.RelatedObjectArtifactID);
			EmailMessage message = null;
			if (!choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryCompleted))
			{
				message = GetFailEmail();
			}
			else
			{
				message = GetSuccessEmail();
			}
			SendEmail(job, message);
		}

		private EmailMessage GetSuccessEmail()
		{
			var message = string.Empty;
			return null;
		}

		private EmailMessage GetFailEmail()
		{
			return null;
		}


		private void SendEmail(Job parentJob, EmailMessage message)
		{
			_manager.CreateJob(parentJob, message, TaskType.SendEmailManager);
		}

	}
}
