using Castle.Windsor;
using global::Relativity.API;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Category("Integration Tests")]
	public class ITaskFactoryTests
	{
		[OneTimeSetUp]
		[Test]
		public void UpdateJobHistory()
		{
			// arrange
			Job tempJob = JobExtensions.CreateJob();
			IAgentHelper helper = Substitute.For<IAgentHelper>();
			kCura.IntegrationPoints.Data.IntegrationPoint integrationPoint = new kCura.IntegrationPoints.Data.IntegrationPoint();
			TaskParameters paramerters = new TaskParameters();
			JobHistory jobHistory = new JobHistory() { ArtifactId = 666 };

			ScheduleQueueAgentBase agentBase = new TestAgentBase(Guid.NewGuid());
			IWindsorContainer container = Substitute.For<IWindsorContainer>();
			ICaseServiceContext caseServiceContext = Substitute.For<ICaseServiceContext>();
			ISerializer serializer = Substitute.For<ISerializer>();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();

			caseServiceContext.RsapiService.IntegrationPointLibrary.Read(Arg.Any<int>()).Returns(integrationPoint);
			serializer.Deserialize<TaskParameters>(Arg.Any<String>()).Returns(paramerters);
			jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(jobHistory);

			container.Resolve<SyncManager>().Throws(new Exception("Error message."));
			container.Resolve<ISerializer>().Returns(serializer);
			container.Resolve<ICaseServiceContext>().Returns(caseServiceContext);
			container.Resolve<IJobHistoryService>().Returns(jobHistoryService);

			TaskFactory taskFactory = new TaskFactory(helper, container);

			// act
			Assert.Throws<Exception>(() => taskFactory.CreateTask(tempJob, agentBase), "Error message.");

			// assert
			caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(1).Create(Arg.Any<List<JobHistoryError>>());
		}

		public class TestAgentBase : ScheduleQueueAgentBase
		{
			public TestAgentBase(Guid agentGuid, IDBContext dbContext = null, IAgentService agentService = null, IJobService jobService = null, IScheduleRuleFactory scheduleRuleFactory = null)
				: base(agentGuid, dbContext, agentService, jobService, scheduleRuleFactory)
			{
			}

			public override string Name { get; }
		}
	}
}