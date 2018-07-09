using System.Collections.Generic;
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
using kCura.ScheduleQueue.Core.Core;
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
		  Apps.Common.Utils.Serializers.ISerializer serializer,
		  ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
		  IJobHistoryService jobHistoryService,
		  IJobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager,
		  IJobStatusUpdater jobStatusUpdater,
		  KeywordConverter converter,
		  IManagerFactory managerFactory,
		  IContextContainerFactory contextContainerFactory,
		  IJobService jobService) : base(caseServiceContext,
		   helper,
		   dataProviderFactory,
		   serializer,
		   appDomainRdoSynchronizerFactoryFactory,
		   jobHistoryService,
		   jobHistoryErrorService,
		   jobManager,
		   managerFactory,
		   contextContainerFactory,
		   jobService)
		{
			_jobStatusUpdater = jobStatusUpdater;
			_converter = converter;
		}

		public void OnJobStart(Job job) { }

		public void OnJobComplete(Job job)
		{
			SetIntegrationPoint(job);

			List<string> emails = GetRecipientEmails();

			if (emails != null && emails.Any())
			{
				TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
				Relativity.Client.DTOs.Choice choice = _jobStatusUpdater.GenerateStatus(taskParameters.BatchInstance, job.WorkspaceID);

				EmailMessage message = GenerateEmail(choice);

				SendEmail(job, message, emails);
			}
		}

		public EmailMessage GenerateEmail(Relativity.Client.DTOs.Choice choice)
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
			else if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryStopped))
			{
				message.Subject = Properties.JobStatusMessages.JOB_STOPPED_SUBJECT;
				message.MessageBody = Properties.JobStatusMessages.JOB_STOPPED_BODY;
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
			JobManager.CreateJob(parentJob, message, TaskType.SendEmailManager);
		}
	}
}
