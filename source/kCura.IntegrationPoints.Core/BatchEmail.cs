using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Keywords;
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
		private readonly KeywordConverter _converter;
		public BatchEmail(IJobStatusUpdater jobStatusUpdater, ISerializer serializer, IntegrationPointService pointService, IJobManager manager, KeywordConverter converter)
		{
			_updater = jobStatusUpdater;
			_serializer = serializer;
			_pointService = pointService;
			_manager = manager;
			_converter = converter;
		}

		public void JobStarted(Job job) { }

		public void JobComplete(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			var choice = _updater.GenerateStatus(taskParameters.BatchInstance);
			EmailMessage message = new EmailMessage();

			if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryCompletedWithErrors))
			{
				message.Subject = Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_SUBJECT;
				message.MessageBody = Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_BODY;
			}
			else if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryErrorJobFailed))
			{
				message.Subject = Properties.JobStatusMessages.JOB_FAILED_SUBJECT;
				message.MessageBody = Properties.JobStatusMessages.JOB_FAILED_BODY;
			}
			else
			{
				message.Subject = Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_SUBJECT;
				message.MessageBody = Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_BODY;
			}
			SendEmail(job, message);
		}

		private void SendEmail(Job parentJob, EmailMessage message)
		{
			var emails = _pointService.GetRecipientEmails(parentJob.RelatedObjectArtifactID).ToList();
			if (emails.Any())
			{
				message.Emails = emails;
				message.Subject = _converter.Convert(message.Subject);
				message.MessageBody = _converter.Convert(message.MessageBody);
				_manager.CreateJob(parentJob, message, TaskType.SendEmailManager);
			}
		}

	}
}
