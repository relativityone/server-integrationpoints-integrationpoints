using System;
using kCura.ScheduleQueueAgent.Services;
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
			
			int caseID1 = 1015641;
			int caseID2 = 1018513;
			int relatedObjectArtifactID = 1111111;
			string taskType = "MyTestTask";
			Guid agentGuid = new Guid("D65F5774-6572-49F0-91C4-28161A75DF0D");

			IDBContext eddsContext = new TestDBContextHelper().GetEDDSDBContext();
			IDBContext c1Context = new TestDBContextHelper().GetEDDSDBContext();
			IDBContext c2Context = new TestDBContextHelper().GetEDDSDBContext();
			//AgentInformation ai = jobService.GetAgentInformation(new Guid("08C0CE2D-8191-4E8F-B037-899CEAEE493D")); //Integration Points agent
			IAgentService agentService = new AgentService(dbContext, agentGuid); //RLH agent
			var jobService1 = new JobService(agentService, dbContext);
			var jobService2 = new JobService(agentService, dbContext);

			Job jobOld = jobService.GetJob(caseID1, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);
			jobOld = jobService.GetJob(caseID2, relatedObjectArtifactID, taskType);
			if (jobOld != null) jobService.DeleteJob(jobOld.JobId);

			Job job = jobService.CreateJob(workspaceID, relatedObjectArtifactID, taskType, DateTime.UtcNow, "My Test Job Detail", 1212121);
			Job job1 = jobService.GetJob(job.JobId);
			Job job2 = jobService.GetJob(workspaceID, job.RelatedObjectArtifactID, taskType);
			Job job3 = jobService.GetNextQueueJob(new int[] { 1015040 });
			jobService.UnlockJobs(agentService.AgentInformation.AgentID);
			jobService.DeleteJob(job3.JobId);
		}
}
}
