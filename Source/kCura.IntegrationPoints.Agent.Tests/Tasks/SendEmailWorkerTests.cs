using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Email;
using kCura.IntegrationPoints.Email.Dto;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture]
	public class SendEmailWorkerTests
	{
		private SendEmailWorker _sut;
		private Mock<IEmailSender> _emailSender;
		private readonly ISerializer _serializer = new JSONSerializer();
		private readonly JobBuilder _jobBuilder = new JobBuilder();

		[SetUp]
		public void SetUp()
		{
			Mock<IAPILog> logger = new Mock<IAPILog>(MockBehavior.Loose){DefaultValue = DefaultValue.Mock};
			_emailSender = new Mock<IEmailSender>();
			_sut = new SendEmailWorker(
				_serializer,
				_emailSender.Object,
				logger.Object);
		}

		[Test]
		public void ShouldSendEmailWIthDetailsFromTaskParameters()
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
			Job job = _jobBuilder.WithJobDetails(serializedJobDetails).Build();

			//ACT
			_sut.Execute(job);
			
			//ASSERT
			VerifyEmailHasBeenSent(emailJobParameters);
		}

		[Test]
		public void ShouldSendEmailWithDetailsFromEmailJobDetails()
		{
			//ARRANGE
			EmailJobParameters emailJobParameters = GenerateEmailJobParameters();
			string serializedJobDetails = _serializer.Serialize(emailJobParameters);
			Job job = _jobBuilder.WithJobDetails(serializedJobDetails).Build();

			//ACT
			_sut.Execute(job);

			//ASSERT
			VerifyEmailHasBeenSent(emailJobParameters);
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

		private void VerifyEmailHasBeenSent(EmailJobParameters emailJobParameters)
		{
			_emailSender.Verify(
				x => x.Send(It.Is<EmailMessageDto>(actual => ValidateEmailsAreSame(emailJobParameters, actual))));
		}

		private bool ValidateEmailsAreSame(EmailJobParameters expected, EmailMessageDto actual)
		{
			return expected.Subject == actual.Subject && expected.MessageBody == actual.Body && expected.Emails.First() == actual.ToAddress;
		}
	}
}
