﻿using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class BatchEmail : IntegrationPointTaskBase, IBatchStatus
	{
		private readonly IJobStatusUpdater _jobStatusUpdater;
		private readonly KeywordConverter _converter;
		public BatchEmail(ICaseServiceContext caseServiceContext,
		  IHelper helper,
		  IDataProviderFactory dataProviderFactory,
		  kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
		  ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
		  IJobHistoryService jobHistoryService,
		  JobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager,
		  IJobStatusUpdater jobStatusUpdater,
		  KeywordConverter converter,
		  IManagerFactory managerFactory) : base(caseServiceContext,
		   helper,
		   dataProviderFactory,
		   serializer,
		   appDomainRdoSynchronizerFactoryFactory,
		   jobHistoryService,
		   jobHistoryErrorService,
		   jobManager,
		   managerFactory)
		{
			_jobStatusUpdater = jobStatusUpdater;
			_converter = converter;
		}

		public void OnJobStart(Job job) { }

		public void OnJobComplete(Job job)
		{
			SetIntegrationPoint(job);

			List<string> emails = GetRecipientEmails();

			if (emails!= null && emails.Any())
			{
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				kCura.Relativity.Client.Choice choice = _jobStatusUpdater.GenerateStatus(taskParameters.BatchInstance);

				EmailMessage message = GenerateEmail(choice);

				SendEmail(job, message, emails);
			}
		}

		public EmailMessage GenerateEmail(kCura.Relativity.Client.Choice choice)
		{
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
			return message;
		}

		private void SendEmail(Job parentJob, EmailMessage message, IEnumerable<string> emails)
		{
			message.Emails = emails;
			message.Subject = _converter.Convert(message.Subject);
			message.MessageBody = _converter.Convert(message.MessageBody);
			_jobManager.CreateJob(parentJob, message, TaskType.SendEmailManager);
		}
	}
}
