using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
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
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class SendEmailManagerUnitTests : TestBase
	{
		private ISerializer _serializer;
		private IJobManager _jobManager;
		private SendEmailManager _sendEmailManager;
		private IJobService _jobService;
		private EmailMessage _emailMessage;
		private string _serializedEmailMessage;


		[SetUp]
		public override void SetUp()
		{
			_jobService = Substitute.For<IJobService>();
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

		private static object[] emailLists = new object[]
		{
			new List<string>() {"abc@kcura.com", "email2@kcura.com"},
			new List<string>() {"abc@kcura.com"},
			new List<string>() {""}
		};

		[Test, TestCaseSource(nameof(emailLists))]
		public void CreateBatchJob_GoldFlow(List<string> list)
		{
			// arrange
			Job job = JobExtensions.CreateJob(1, 1, _serializedEmailMessage);
			_serializer.Deserialize<EmailMessage>(job.JobDetails).Returns(JsonConvert.DeserializeObject<EmailMessage>(job.JobDetails));

			// act
			_sendEmailManager.CreateBatchJob(job, list);

			// assert
			this._jobManager.Received(1).CreateJob(job, Arg.Is<EmailMessage>(email => email.Emails.SequenceEqual(list) && email.Subject.Equals(_emailMessage.Subject) && email.MessageBody.Equals(_emailMessage.MessageBody)), TaskType.SendEmailWorker);
		}
	}
}