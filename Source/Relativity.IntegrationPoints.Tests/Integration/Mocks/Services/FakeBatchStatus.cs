using kCura.IntegrationPoints.Core;
using kCura.ScheduleQueue.Core;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeBatchStatus : IBatchStatus
	{
		public void OnJobStart(Job job)
		{
		}

		public void OnJobComplete(Job job)
		{
		}
	}
}