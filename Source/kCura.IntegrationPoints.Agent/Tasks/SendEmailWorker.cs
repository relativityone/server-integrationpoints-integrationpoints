using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.RelativitySync;
using kCura.IntegrationPoints.RelativitySync.RipOverride;
using kCura.IntegrationPoints.Email.Dto;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailWorker : ITask, ISendEmailWorker
	{
		private readonly IAPILog _logger;
		private readonly IEmailSender _emailSender;
		private readonly ISerializer _serializer;

		public SendEmailWorker(ISerializer serializer, IEmailSender emailSender, IHelper helper)
		{
			_serializer = serializer;
			_emailSender = emailSender;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SendEmailManager>();
		}

		public void Execute(Job job)
		{
			var jobId = job.JobId;
			LogExecuteStart(jobId);

			EmailMessage details = _serializer.Deserialize<EmailMessage>(job.JobDetails);

			Execute(details, jobId);
		}

		public void Execute(EmailMessage details, long jobId)
		{
			var exceptions = new List<Exception>();
			IEnumerable<string> emails = details.Emails;
			foreach (string email in emails)
			{
				try
				{
					var message = new EmailMessageDto(
						subject: details.Subject,
						body: details.MessageBody,
						toAddress: email
					);
					_emailSender.Send(message);

					LogExecuteSuccesfulEnd(jobId);
				}
				catch (Exception e)
				{
					LogSendingEmailError(jobId, e, email);
					exceptions.Add(new Exception(string.Format("Failed to send message to {0}", email), e));
				}
			}

			if (exceptions.Any())
			{
				LogErrorsDuringEmailSending(jobId);
				throw new AggregateException(exceptions);
			}
		}

		#region Logging

	    private void LogExecuteStart(long jobId)
	    {
	        _logger.LogInformation("Started executing send email worker, job: {JobId}", jobId);
	    }
	    private void LogExecuteSuccesfulEnd(long jobId)
	    {
	        _logger.LogInformation("Succesfully sent email in worker, job: {JobId}", jobId);
	    }

        private void LogSendingEmailError(long jobId, Exception e, string email)
		{
			_logger.LogError(e, "Failed to send message to {Email} for job {JobId}.", email, jobId);
		}

		private void LogErrorsDuringEmailSending(long jobId)
		{
			_logger.LogError("Failed to send emails in SendEmailWorker for job {JobId}.", jobId);
		}

		#endregion
	}
}