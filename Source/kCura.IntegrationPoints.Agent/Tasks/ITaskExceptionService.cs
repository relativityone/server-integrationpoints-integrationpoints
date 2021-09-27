using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public interface ITaskExceptionService
	{
		void EndTaskWithError(ITask task, Exception ex);
		void EndJobWithError(Job job, Exception ex);
	}
}
