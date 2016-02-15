using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Email;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SendEmailWorker : ITask
	{
		private readonly ISerializer _serializer;
		private readonly ISendable _sendable;
		public SendEmailWorker(ISerializer serializer, ISendable sendable)
		{
			_serializer = serializer;
			_sendable = sendable;
		}

		public void Execute(Job job)
		{
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
					message.From = new MailAddress(kCura.Apps.Common.Config.Sections.NotificationConfig.EmailFrom);
					_sendable.Send(message);
				}
				catch (Exception e)
				{
					exceptions.Add(new Exception(string.Format("Failed to send message to {0}", email), e));
				}
			}
			if (exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}
	}
}
