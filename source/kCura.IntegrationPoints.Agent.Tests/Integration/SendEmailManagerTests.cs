using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Category("Integration Tests")]
	public class SendEmailManagerTests : RelativityProviderTemplate
	{
		private ISerializer _serializer;
		private IJobManager _jobManager;
		private SendEmailManager _sendEmailManager;
		private IQueueDBContext _queueContext;

		public SendEmailManagerTests()
			: base("IntegrationPointSource", null)
		{
		}

		[SetUp]
		[OneTimeSetUp]
		public void SuiteSetUp()
		{
			_serializer = Container.Resolve<ISerializer>();
			_jobManager = this.Container.Resolve<IJobManager>();
			_sendEmailManager = new SendEmailManager(this._serializer, this._jobManager);
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		/// <summary>
		///
		/// </summary>
		[Test]
		public void VerifyGetUnbatchedID()
		{
			string jobDetails =
				"{\"Subject\":\"testing stuff\",\"MessageBody\":\"Hello, this is GeeeRizzle \",\"Emails\":[\"testing1234@kcura.com\",\"kwu@kcura.com\"]}";
			string scheduleRule = "Rule";

			int jobId = JobExtensions.Execute(
				this._queueContext,
				SourceWorkspaceArtifactId,
				1,
				"SendEmailManager",
				DateTime.Now,
				1,
				null,
				scheduleRule,
				jobDetails,
				1,
				777,
				10101,
				1,
				1);

			Job tempJob = this._jobManager.GetJob(SourceWorkspaceArtifactId, 1, "SendEmailManager");

			//Act
			IEnumerable<string> list = _sendEmailManager.GetUnbatchedIDs(tempJob);

			//Assert
			Assert.AreEqual(2, list.Count());
			Assert.IsTrue(list.Contains("testing1234@kcura.com"));
			Assert.IsTrue(list.Contains("kwu@kcura.com"));
		}

		[Test]
		public void VerifyCreateBatchJob()
		{
			List<string> list = new List<string> { };
			list.Add("krua@kcura.com");
			list.Add("krua1@kcura.com");
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "test", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);
			Job tempJob = JobExtensions.CreateJobAgentTypeId(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, 12313, -1, 11111);
			this._sendEmailManager.CreateBatchJob(tempJob, list);

			list.Add("");
		}
	}
}