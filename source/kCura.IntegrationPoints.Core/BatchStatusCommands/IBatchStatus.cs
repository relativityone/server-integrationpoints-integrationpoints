using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core
{
	public interface IBatchStatus
	{
		void OnJobStart(Job job);
		void OnJobComplete(Job job);
	}
}
