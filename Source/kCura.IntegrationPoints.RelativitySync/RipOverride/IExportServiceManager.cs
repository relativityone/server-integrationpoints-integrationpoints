using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.RelativitySync.RipOverride
{
	public interface IExportServiceManager
	{
		void Execute(Job job);
	}
}