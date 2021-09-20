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
using FluentAssertions;
using kCura.IntegrationPoints.Email.Exceptions;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	[TestFixture, Category("Unit")]
	public class SendEmailWorkerTests
	{
		private SendEmailWorker _sut;
		private Mock<IEmailSender> _emailSender;
		private readonly ISerializer _serializer = new JSONSerializer();
		private readonly JobBuilder _jobBuilder = new JobBuilder();
		private readonly List<string> _emails = new List<string>
		{
			"first@email.rip",
			"second@email.rip"
		};

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
			Job job = CreateSendEmailJob(emailJobParameters);

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
			Job job = CreateSendEmailJob(emailJobParameters);

			//ACT
			_sut.Execute(job);

			//ASSERT
			VerifyEmailHasBeenSent(emailJobParameters);
		}

		[Test]
		public void ShouldCatchEmailExceptionAndProceedWithSendingOtherEmails()
		{
			//ARRANGE
			EmailJobParameters emailJobParameters = GenerateEmailJobParameters(_emails);
			string emailToPass = _emails.First();
			string emailToFail = _emails.Skip(1).First();
			Job job = CreateSendEmailJob(emailJobParameters);
			_emailSender
				.Setup(x => x.Send(It.Is<EmailMessageDto>(y => y.ToAddress == emailToFail)))
				.Throws(new SendEmailException(""));
			
			//ACT
			Action act = () => _sut.Execute(job);

			//ASSERT
			act.ShouldThrowExactly<AggregateException>().Where(x => x.InnerExceptions.Count == 1);
			_emailSender.Verify(x => x.Send(It.Is<EmailMessageDto>(y => y.ToAddress == emailToPass)));
		}

		[Test]
		public void ShouldNotCatchGenericExceptionAndStopExecution()
		{
			//ARRANGE
			EmailJobParameters emailJobParameters = GenerateEmailJobParameters(_emails);
			string emailToFail = _emails.First();
			Job job = CreateSendEmailJob(emailJobParameters);
			_emailSender
				.Setup(x => x.Send(It.Is<EmailMessageDto>(y => y.ToAddress == emailToFail)))
				.Throws<NullReferenceException>();

			//ACT
			Action act = () => _sut.Execute(job);

			//ASSERT
			act.ShouldThrowExactly<NullReferenceException>();
			_emailSender.Verify(x => x.Send(
				It.IsAny<EmailMessageDto>()),
				Times.Once,
				"We expected single call to emailSender because that call threw an unexpected exception.'"
				);
		}

		private Job CreateSendEmailJob(EmailJobParameters emailJobParameters)
		{
			string serializedJobDetails = _serializer.Serialize(emailJobParameters);
			Job job = _jobBuilder.WithJobDetails(serializedJobDetails).Build();
			return job;
		}

		private EmailJobParameters GenerateEmailJobParameters(List<string> emails)
		{
			string emailSubject = "emailSubject";
			string emailMessage = "emailMessage";

			EmailJobParameters emailJobParameters = new EmailJobParameters()
			{
				Emails = emails,
				MessageBody = emailMessage,
				Subject = emailSubject
			};
			return emailJobParameters;
		}

		private EmailJobParameters GenerateEmailJobParameters()
		{

			return GenerateEmailJobParameters(_emails.Take(1).ToList());
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
