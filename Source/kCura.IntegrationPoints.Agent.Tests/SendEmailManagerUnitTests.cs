﻿using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class SendEmailManagerUnitTests : TestBase
	{
		private ISerializer _serializer;
		private IJobManager _jobManager;
		private SendEmailManager _sendEmailManager;
		private EmailMessage _emailMessage;
		private string _serializedEmailMessage;
		private static object[] _emailLists = new object[]
		{
			new List<string>() {"abc@relativity.com", "email2@relativity.com"},
			new List<string>() {"abc@relativity.com"},
			new List<string>() {""}
		};

		[SetUp]
		public override void SetUp()
		{
			_serializer = Substitute.For<ISerializer>();
			_jobManager = Substitute.For<IJobManager>();
			IHelper helper = Substitute.For<IHelper>();
			_sendEmailManager = new SendEmailManager(_serializer, _jobManager, helper);

			_emailMessage = new EmailMessage()
			{
				Subject = "email test",
				MessageBody = "hello."
			};

			_serializedEmailMessage = JsonConvert.SerializeObject(_emailMessage);
		}

		[Test]
		public void GetUnbatchedIDs_NullJob()
		{
			Assert.DoesNotThrow(() => this._sendEmailManager.GetUnbatchedIDs(null));
		}

		[Test, TestCaseSource(nameof(_emailLists))]
		public void CreateBatchJob_GoldFlow(List<string> list)
		{
			// arrange
			Job job = new JobBuilder()
				.WithWorkspaceId(1)
				.WithRelatedObjectArtifactId(1)
				.WithJobDetails(_serializedEmailMessage)
				.Build();
			_serializer.Deserialize<EmailMessage>(job.JobDetails).Returns(JsonConvert.DeserializeObject<EmailMessage>(job.JobDetails));

			// act
			_sendEmailManager.CreateBatchJob(job, list);

			// assert
			this._jobManager.Received(1).CreateJob(job, Arg.Is<EmailMessage>(email => email.Emails.SequenceEqual(list) && email.Subject.Equals(_emailMessage.Subject) && email.MessageBody.Equals(_emailMessage.MessageBody)), TaskType.SendEmailWorker);
		}
	}
}