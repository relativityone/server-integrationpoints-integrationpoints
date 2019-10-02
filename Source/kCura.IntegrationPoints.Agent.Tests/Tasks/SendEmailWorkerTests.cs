using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Query.Dynamic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.Email.Dto;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class SendEmailWorkerTests
	{
		private SendEmailWorker _sut;
		private Mock<IEmailSender> _emailSender;
		private readonly ISerializer _serializer = new JSONSerializer();

		[SetUp]
		public void SetUp()
		{
			Mock<IAPILog> logger = new Mock<IAPILog>(MockBehavior.Loose);
			_emailSender = new Mock<IEmailSender>();
			_sut = new SendEmailWorker(
				_serializer,
				_emailSender.Object,
				logger.Object);
		}

		[Test]
		public void Execute_ShouldProperlySerializeEmailJobDetailsFromTaskParameters()
		{
			//ARRANGE
			EmailJobParameters emailJobParameters = GenerateEmailJobParameters();

			Guid guid = Guid.NewGuid();
			TaskParameters taskParameters = new TaskParameters
			{
				BatchInstance = guid,
				BatchParameters = emailJobParameters
			};
			string serializedJobDetails = _serializer.Serialize(taskParameters);
			Job job = JobHelper.CreateJob(1, 0, 0, 0, 0, 0, 0, TaskType.SendEmailWorker, DateTime.UtcNow, null,
				serializedJobDetails, 0, DateTime.UtcNow, 9, null, null, StopState.None);
			//ACT
			_sut.Execute(job);
			//ASSERT
			_emailSender.Verify(x => x.Send(It.Is<EmailMessageDto>(actual => ValidateEmailsAreSame(emailJobParameters, actual))));
		}		
		
		[Test]
		public void Execute_ShouldProperlySerializeEmailJobDetails()
		{
			//ARRANGE
			EmailJobParameters emailJobParameters = GenerateEmailJobParameters();

			string serializedJobDetails = _serializer.Serialize(emailJobParameters);
			Job job = JobHelper.CreateJob(1, 0, 0, 0, 0, 0, 0, TaskType.SendEmailWorker, DateTime.UtcNow, null,
				serializedJobDetails, 0, DateTime.UtcNow, 9, null, null, StopState.None);
			//ACT
			_sut.Execute(job);
			//ASSERT
			_emailSender.Verify(x => x.Send(It.Is<EmailMessageDto>(actual => ValidateEmailsAreSame(emailJobParameters, actual))));
		}

		private static EmailJobParameters GenerateEmailJobParameters()
		{
			string emailSubject = "emailSubject";
			string emailMessage = "emailMessage";
			string emailAddress = "emailAddress@rip.com";

			EmailJobParameters emailJobParameters = new EmailJobParameters()
			{
				Emails = new List<string> {emailAddress},
				MessageBody = emailMessage,
				Subject = emailSubject
			};
			return emailJobParameters;
		}

		private bool ValidateEmailsAreSame(EmailJobParameters expected, EmailMessageDto actual)
		{
			return expected.Subject == actual.Subject && expected.MessageBody == actual.Body && expected.Emails.First() == actual.ToAddress;
		}
	}
}
