using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using kCura.Apps.Common.Config.Sections;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Email;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailWorker : ITask
	{
		private readonly IAPILog _logger;
		private readonly ISendable _sendable;
		private readonly ISerializer _serializer;

		public SendEmailWorker(ISerializer serializer, ISendable sendable, IHelper helper)
		{
			_serializer = serializer;
			_sendable = sendable;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SendEmailManager>();
		}

		public void Execute(Job job)
		{
		    LogExecuteStart(job);

            var details = _serializer.Deserialize<EmailMessage>(job.JobDetails);

			var exceptions = new List<Exception>();
			var emails = details.Emails;
			foreach (var email in emails)
			{
				try
				{
					var message = new MailMessage();
					message.Body = details.MessageBody;
					message.Subject = details.Subject;
					message.To.Add(email);
					message.From = new MailAddress(NotificationConfig.EmailFrom);
					_sendable.Send(message);
				    LogExecuteSuccesfulEnd(job);
                }
				catch (Exception e)
				{
					LogSendingEmailError(job, e, email);
					exceptions.Add(new Exception(string.Format("Failed to send message to {0}", email), e));
				}
			}
			if (exceptions.Any())
			{
				LogErrorsDuringEmailSending(job);
				throw new AggregateException(exceptions);
			}
		}

		#region Logging

	    private void LogExecuteStart(Job job)
	    {
	        _logger.LogInformation("Started executing send email worker, job: {JobId}", job.JobId);
	    }
	    private void LogExecuteSuccesfulEnd(Job job)
	    {
	        _logger.LogInformation("Succesfully sent email in worker, job: {Job}", job);
	    }

        private void LogSendingEmailError(Job job, Exception e, string email)
		{
			_logger.LogError(e, "Failed to send message to {Email} for job {JobId}.", email, job.JobId);
		}

		private void LogErrorsDuringEmailSending(Job job)
		{
			_logger.LogError("Failed to send emails in SendEmailWorker for job {JobId}.", job.JobId);
		}

		#endregion
	}
}