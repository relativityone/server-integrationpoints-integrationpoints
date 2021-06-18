using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
	internal interface IJobExecutor
	{
		event ExceptionEventHandler JobExecutionError;

		//event JobPostExecuteEventHandler JobPostExecute;

		TaskResult ProcessJob(Job job);
	}
}
