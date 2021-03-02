using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class JobHelper : HelperBase
	{
		public JobHelper(InMemoryDatabase database, ProxyMock proxyMock) : base(database, proxyMock)
		{
		}

		public Job ScheduleEmptyJob(Agent agent, DateTime nextRunTime)
		{
			Job job = new Job
			{
				JobId = JobId.Next,
				AgentTypeID = agent.AgentTypeId,
				NextRunTime = nextRunTime,
			};

			Database.JobsInQueue.Add(job);

			return job;
		}
	}
}
