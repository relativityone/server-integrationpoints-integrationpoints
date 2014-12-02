using System;
using System.Collections.Generic;
using System.Threading;
using kCura.Apps.Common.Data;
using kCura.Data.RowDataGateway;
using kCura.ScheduleQueueAgent.ScheduleRules;
using kCura.ScheduleQueueAgent.Services;
using kCura.ScheduleQueueAgent.TimeMachine;
using NSubstitute;
using NUnit.Framework;
using IDBContext = Relativity.API.IDBContext;
using kCura.Apps.Common.Config;

namespace kCura.ScheduleQueueAgent.Tests.Integration.Services
{
	[TestFixture]
	public class JobServiceTests
	{
		[Test]
		[Explicit]
		public void jobService_CreateUnscheduledJob()
		{
			int workspaceID = 1015641;
			int relatedObjectArtifactID = 1111111;
			string taskType = "MyTestTask";
			Guid agentGuid = new Guid("D65F5774-6572-49F0-91C4-28161A75DF0D");

			IDBContext dbContext = new TestDBContextHelper().GetEDDSDBContext();
			//AgentInformation ai = jobService.GetAgentInformation(new Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")); //Integration Points agent
			IAgentService agentService = new AgentService(dbContext, agentGuid); //RLH agent
			var jobService = new JobService(agentService, dbContext);

			Job jobOld = jobService.GetJob(workspaceID, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			Job job = jobService.CreateJob(workspaceID, relatedObjectArtifactID, taskType, DateTime.UtcNow, "My Test Job Detail", 1212121);
			Job job1 = jobService.GetJob(job.JobId);
			Job job2 = jobService.GetJob(workspaceID, job.RelatedObjectArtifactID, taskType);
			Job job3 = jobService.GetNextQueueJob(new int[] { 1015040 });
			jobService.UnlockJobs(agentService.AgentInformation.AgentID);
			jobService.DeleteJob(job3.JobId);
		}

		[Test]
		[Explicit]
		public void jobService_CreateScheduledJob()
		{
			int workspaceID = 1015641;
			int relatedObjectArtifactID = 1111111;
			string taskType = "MyTestTask";
			IScheduleRule sr = new PeriodicScheduleRule(ScheduleInterval.Immediate, DateTime.Parse("1/1/2001"), DateTime.Parse("1/1/2001 10:30:00").TimeOfDay);
			Guid agentGuid = new Guid("D65F5774-6572-49F0-91C4-28161A75DF0D");

			IDBContext dbContext = new TestDBContextHelper().GetEDDSDBContext();
			//AgentInformation ai = jobService.GetAgentInformation(new Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")); //Integration Points agent
			IAgentService agentService = new AgentService(dbContext, agentGuid); //RLH agent
			var jobService = new JobService(agentService, dbContext);

			Job jobOld = jobService.GetJob(workspaceID, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			Job job = jobService.CreateJob(workspaceID, relatedObjectArtifactID, taskType, sr, "My Test Job Detail", 1212121);
			Job job2 = null;
			DateTime dt = DateTime.Now;
			while (DateTime.Now.Subtract(dt).Minutes < 4)
			{
				job2 = jobService.GetNextQueueJob(new int[] { 1015040 });
				if (job2 != null) jobService.FinalizeJob(job2, new TaskResult() { Status = TaskStatusEnum.Success, Exceptions = new List<Exception>() });
				Thread.Sleep(1000);
			}
			jobService.DeleteJob(job2.JobId);
		}

		[Test]
		[Explicit]
		public void AgentTimeMachineProvider_Test1()
		{
			int caseID1 = 1015641;
			var agentHelper = NSubstitute.Substitute.For<Relativity.API.IAgentHelper>();

			Manager.Settings.Factory = new HelperConfigSqlServiceFactory(agentHelper);
			IDBContext c1Context = new TestDBContextHelper().GetDBContext(caseID1);
			agentHelper.GetDBContext(Arg.Is(caseID1)).Returns(c1Context);

			AgentTimeMachineProvider.Current = new DefaultAgentTimeMachineProvider(Guid.Parse("08C0CE2D-8191-4E8F-B037-899CEAEE493D"));

			DateTime utcNow1 = AgentTimeMachineProvider.Current.UtcNow;
			Thread.Sleep(1000);
			DateTime utcNow2 = AgentTimeMachineProvider.Current.UtcNow;

		}
	}
}
