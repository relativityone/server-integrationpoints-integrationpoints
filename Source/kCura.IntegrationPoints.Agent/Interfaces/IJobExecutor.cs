using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
	public interface IJobExecutor
	{
		event ExceptionEventHandler JobExecutionError;

		TaskResult ProcessJob(Job job);
	}
}
