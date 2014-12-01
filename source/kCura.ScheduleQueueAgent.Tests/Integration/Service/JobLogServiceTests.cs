using System;
using kCura.ScheduleQueueAgent.Logging;
using kCura.ScheduleQueueAgent.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.Tests.Integration.Service
{
	[TestFixture]
	public class JobLogServiceTests
	{
		[Test]
		[Explicit]
		public void jobService_CreateUnscheduledJob()
		{
			var agentHelper = NSubstitute.Substitute.For<Relativity.API.IAgentHelper>();

			int caseID1 = 1015641;
			int caseID2 = 1018513;
			int relatedObjectArtifactID = 1111111;
			string taskType = "MyTestTask";
			Guid agentGuid = new Guid("D65F5774-6572-49F0-91C4-28161A75DF0D");

			IDBContext eddsContext = new TestDBContextHelper().GetEDDSDBContext();
			IDBContext c1Context = new TestDBContextHelper().GetDBContext(caseID1);
			IDBContext c2Context = new TestDBContextHelper().GetDBContext(caseID2);
			IAgentService agentService = new AgentService(eddsContext, agentGuid); //RLH agent
			AgentInformation ai = agentService.AgentInformation;
			var jobService = new JobService(agentService, eddsContext);

			Job jobOld = jobService.GetJob(caseID1, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			jobOld = jobService.GetJob(caseID2, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			Job job1 = jobService.CreateJob(caseID1, relatedObjectArtifactID, taskType, DateTime.UtcNow, "My Test Job Detail", 1212121);
			Job job2 = jobService.CreateJob(caseID2, relatedObjectArtifactID, taskType, DateTime.UtcNow, "My Test Job Detail", 1212121);

			agentHelper.GetDBContext(Arg.Is(caseID1)).Returns(c1Context);
			agentHelper.GetDBContext(Arg.Is(caseID2)).Returns(c2Context);

			JobLogService log= new JobLogService(agentHelper);

			log.Log(ai, job1, JobLogState.Created, caseID1.ToString());
			log.Log(ai, job1, JobLogState.Error, "Test error for job1 in case" + caseID1.ToString());

			log.Log(ai, job2, JobLogState.Created, caseID2.ToString());
			log.Log(ai, job2, JobLogState.Error, "Test error for job1 in case" + caseID2.ToString());

			log.Log(ai, job1, JobLogState.Finished, "Test finished for job1 in case" + caseID1.ToString());
			log.Log(ai, job2, JobLogState.Finished, "Test finished for job1 in case" + caseID2.ToString());

			jobService.DeleteJob(job1.JobId);
			jobService.DeleteJob(job2.JobId);
		}
	}
}
