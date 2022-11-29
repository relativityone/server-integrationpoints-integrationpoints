using System;
using System.Collections.Generic;
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
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core
{
    public class BatchEmail : IntegrationPointTaskBase, IBatchStatus
    {
        private readonly IJobStatusUpdater _jobStatusUpdater;
        private readonly IEmailFormatter _converter;

        public BatchEmail(ICaseServiceContext caseServiceContext,
            IHelper helper,
            IDataProviderFactory dataProviderFactory,
            Apps.Common.Utils.Serializers.ISerializer serializer,
            ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IJobManager jobManager,
            IJobStatusUpdater jobStatusUpdater,
            IEmailFormatter converter,
            IManagerFactory managerFactory,
            IJobService jobService,
            IIntegrationPointRepository integrationPointRepository,
            IDiagnosticLog diagnosticLog)
            : base(
                caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactoryFactory,
                jobHistoryService,
                jobHistoryErrorService,
                jobManager,
                managerFactory,
                jobService,
                integrationPointRepository,
                diagnosticLog)
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
            ChoiceRef jobStatus = _jobStatusUpdater.GenerateStatus(taskParameters.BatchInstance);
        
            EmailJobParameters jobParameters = GenerateEmailJobParameters(jobStatus, emails);            
            TaskParameters emailTaskParameters = new TaskParameters
            {
                BatchInstance = taskParameters.BatchInstance,
                BatchParameters = jobParameters
            };            
            Job sendEmailJob = JobManager.CreateJob(job, emailTaskParameters, TaskType.SendEmailWorker);
            JobService.UpdateStopState(new List<long> { sendEmailJob.JobId }, StopState.None);
        }

        private EmailJobParameters GenerateEmailJobParameters(ChoiceRef choice, List<string> emails)
        {
            (string Subject, string MessageBody) subjectAndBody = GetEmailSubjectAndBodyForJobStatus(choice);
            EmailJobParameters jobParameters = new EmailJobParameters
            {
                Emails = emails,
                Subject = subjectAndBody.Subject,
                MessageBody = subjectAndBody.MessageBody
            };
            return jobParameters;
        }

        public (string Subject, string MessageBody) GetEmailSubjectAndBodyForJobStatus(ChoiceRef choice)
        {
            string messageBody;
            string messageSubject;
            if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryCompletedWithErrors))
            {
                messageSubject = Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_SUBJECT;
                messageBody = Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_BODY;
            }
            else if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryErrorJobFailed))
            {
                messageSubject = Properties.JobStatusMessages.JOB_FAILED_SUBJECT;
                messageBody = Properties.JobStatusMessages.JOB_FAILED_BODY;
            }
            else if (choice.EqualsToChoice(Data.JobStatusChoices.JobHistoryStopped))
            {
                messageSubject = Properties.JobStatusMessages.JOB_STOPPED_SUBJECT;
                messageBody = Properties.JobStatusMessages.JOB_STOPPED_BODY;
            }
            else
            {
                messageSubject = Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_SUBJECT;
                messageBody = Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_BODY;
            }

            messageSubject = _converter.Format(messageSubject);
            messageBody = _converter.Format(messageBody);

            return (Subject: messageSubject, MessageBody: messageBody);
        }
    }
}
