using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;
using System.Data;
using Relativity.API;
using Constants = kCura.IntegrationPoint.Tests.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class SendEmailManagerTests : RelativityProviderTemplate
	{
		private ISerializer _serializer;
		private IJobManager _jobManager;
		private SendEmailManager _sendEmailManager;
		private IQueueDBContext _queueContext;

		private long _jobId;

		public SendEmailManagerTests()
			: base("IntegrationPointSource", null)
		{
		}

		public override void TestTeardown()
		{
			_jobManager.DeleteJob(_jobId);
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_serializer = Container.Resolve<ISerializer>();
			_jobManager = this.Container.Resolve<IJobManager>();
			IHelper helper = Container.Resolve<IHelper>();
			_sendEmailManager = new SendEmailManager(this._serializer, this._jobManager, helper);
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		[Test]
		public void VerifyGetUnbatchedId()
		{
			string jobDetails =
				"{\"Subject\":\"testing stuff\",\"MessageBody\":\"Hello, this is GeeeRizzle \",\"Emails\":[\"testing1234@kcura.com\",\"kwu@kcura.com\"]}";

			DataRow row = new CreateScheduledJob(this._queueContext).Execute(
				SourceWorkspaceArtifactId,
				1,
				"SendEmaiManager",
				DateTime.Now.AddDays(30),
				1,
				null,
				null,
				jobDetails,
				0,
				777,
				1,
				1);
			Job tempJob = new Job(row);
			_jobId = tempJob.JobId;

			//Act
			IEnumerable<string> list = _sendEmailManager.GetUnbatchedIDs(tempJob);

			//Assert
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.Contains("testing1234@kcura.com"));
			Assert.IsTrue(list.Contains("kwu@kcura.com"));
		}
	}
}