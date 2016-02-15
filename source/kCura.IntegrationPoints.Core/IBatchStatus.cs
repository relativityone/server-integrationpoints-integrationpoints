using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core
{
	public interface IBatchStatus
	{
		void JobStarted(Job job);
		void JobComplete(Job job);
	}
}
