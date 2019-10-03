using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
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
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class BatchEmail : IntegrationPointTaskBase, IBatchStatus
	{
		private readonly IJobStatusUpdater _jobStatusUpdater;
		private readonly IKeywordConverter _converter;

		public BatchEmail(ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			Apps.Common.Utils.Serializers.ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IJobStatusUpdater jobStatusUpdater,
			IKeywordConverter converter,
			IManagerFactory managerFactory,
			IJobService jobService,
			IIntegrationPointRepository integrationPointRepository)
			: base(caseServiceContext,
				helper,
				dataProviderFactory,
				serializer,
				appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobManager,
				managerFactory,
				jobService,
				integrationPointRepository)
		{
			_jobStatusUpdater = jobStatusUpdater;
			_converter = converter;
		}

		public void OnJobStart(Job job) { }

		public void OnJobComplete(Job job)
		{
			SetIntegrationPoint(job);

			List<string> emails = GetRecipientEmails();

			if (!emails.IsNullOrEmpty())
			{
				SendEmails(job, emails);
			}
		}

		private void SendEmails(Job job, List<string> emails)
		{
			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			Relativity.Client.DTOs.Choice choice = _jobStatusUpdater.GenerateStatus(taskParameters.BatchInstance);

			EmailJobParameters jobParameters = GenerateEmailJobParameters(choice);
			ConvertMessage(jobParameters, emails, _converter);

			TaskParameters emailTaskParameters = new TaskParameters
			{
				BatchInstance = taskParameters.BatchInstance,
				BatchParameters = jobParameters
			};
			JobManager.CreateJob(job, emailTaskParameters, TaskType.SendEmailWorker);
		}

		public static EmailJobParameters GenerateEmailJobParameters(Relativity.Client.DTOs.Choice choice)
		{
			EmailJobParameters jobParameters = new EmailJobParameters();

			if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryCompletedWithErrors))
			{
				jobParameters.Subject = Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_SUBJECT;
				jobParameters.MessageBody = Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_BODY;
			}
			else if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryErrorJobFailed))
			{
				jobParameters.Subject = Properties.JobStatusMessages.JOB_FAILED_SUBJECT;
				jobParameters.MessageBody = Properties.JobStatusMessages.JOB_FAILED_BODY;
			}
			else if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryStopped))
			{
				jobParameters.Subject = Properties.JobStatusMessages.JOB_STOPPED_SUBJECT;
				jobParameters.MessageBody = Properties.JobStatusMessages.JOB_STOPPED_BODY;
			}
			else
			{
				jobParameters.Subject = Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_SUBJECT;
				jobParameters.MessageBody = Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_BODY;
			}
			return jobParameters;
		}

		private static void ConvertMessage(EmailJobParameters jobParameters, IEnumerable<string> emails, IKeywordConverter converter)
		{
			jobParameters.Emails = emails;
			jobParameters.Subject = converter.Convert(jobParameters.Subject);
			jobParameters.MessageBody = converter.Convert(jobParameters.MessageBody);
		}
	}
}
