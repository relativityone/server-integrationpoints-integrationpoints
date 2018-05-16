using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	internal class TestingAgent : Agent
	{
		public new ITask GetTask(Job job)
		{
			using (JobContextProvider.StartJobContext(job))
			{
				return base.GetTask(job);
			}
		}
	}
}
