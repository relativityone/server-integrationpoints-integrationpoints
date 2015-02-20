using System;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit
{
	[TestFixture]
	public class AgentJobManagerTest
	{
		[Test]
		public void GetRootJobId_RootJobIDIsNull_ParentJobID()
		{
			//ARRANGE
			Job parentJob = GetJob(222, null);

			//ACT
			long? rootJobID = AgentJobManager.GetRootJobId(parentJob);

			//ASSERT
			Assert.AreEqual(222, rootJobID);
		}

		[Test]
		public void GetRootJobId_RootJobIDIsNotNull_ParentRootJobID()
		{
			//ARRANGE
			Job parentJob = GetJob(222, 101);

			//ACT
			long? rootJobID = AgentJobManager.GetRootJobId(parentJob);

			//ASSERT
			Assert.AreEqual(101, rootJobID);
		}

		private Job GetJob(long jobID, long? rootJobID)
		{
			return JobHelper.GetJob(jobID, rootJobID, null, 1, 1, 111, 222, TaskType.SyncCustodianManagerWorker, new DateTime(), null, "",
				0, new DateTime(), 1, null, null);
		}
	}
}
