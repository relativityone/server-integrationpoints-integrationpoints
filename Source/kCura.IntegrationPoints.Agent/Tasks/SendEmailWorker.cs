using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.Email.Dto;
using kCura.IntegrationPoints.Email.Exceptions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json.Linq;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class SendEmailWorker : ITask
    {
        private readonly IAPILog _logger;
        private readonly IEmailSender _emailSender;
        private readonly ISerializer _serializer;

        public SendEmailWorker(ISerializer serializer, IEmailSender emailSender, IAPILog logger)
        {
            _serializer = serializer;
            _emailSender = emailSender;
            _logger = logger.ForContext<SendEmailWorker>();
        }

        public void Execute(Job job)
        {
            long jobID = job.JobId;
            LogExecuteStart(jobID);

            EmailJobParameters jobParameters = GetEmailJobParametersFromJob(job);

            Execute(jobParameters, jobID);
        }

        private void Execute(EmailJobParameters details, long jobID)
        {
            var exceptions = new List<Exception>();
            IEnumerable<string> emails = details.Emails;
            foreach (string email in emails)
            {
                try
                {
                    EmailMessageDto message = new EmailMessageDto(
                        subject: details.Subject,
                        body: details.MessageBody,
                        toAddress: email
                    );
                    _emailSender.Send(message);

                    LogExecuteSuccessfulEnd(jobID);
                }
                catch (SendEmailException e)
                {
                    LogSendingEmailError(jobID, e);
                    exceptions.Add(new Exception(string.Format("Failed to send message to {0}", email), e));
                }
            }

            if (exceptions.Any())
            {
                LogErrorsDuringEmailSending(jobID);
                throw new AggregateException(exceptions);
            }
        }


        private EmailJobParameters GetEmailJobParametersFromJob(Job job)
        {
            TaskParameters emailTaskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);

            //We need this 'if' here to provide backwards compatibility with older email jobs
            //after changing the way EmailJobParameters are send here. JIRA: REL-354651
            EmailJobParameters details = emailTaskParameters.BatchParameters is JObject
                ? ((JObject)emailTaskParameters.BatchParameters).ToObject<EmailJobParameters>()
                : _serializer.Deserialize<EmailJobParameters>(job.JobDetails);
            return details;
        }

        #region Logging

        private void LogExecuteStart(long jobID)
        {
            _logger.LogInformation("Started executing send email worker, job: {JobId}", jobID);
        }
        private void LogExecuteSuccessfulEnd(long jobID)
        {
            _logger.LogInformation("Successfully sent email in worker, job: {JobId}", jobID);
        }

        private void LogSendingEmailError(long jobID, Exception e)
        {
            _logger.LogError(e, "Failed to send email message for job {JobId}.", jobID);
        }

        private void LogErrorsDuringEmailSending(long jobID)
        {
            _logger.LogError("Failed to send emails in SendEmailWorker for job {JobId}.", jobID);
        }

        #endregion
    }
}