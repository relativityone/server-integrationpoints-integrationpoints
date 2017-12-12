using System;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public interface ITaskExceptionService
	{
		void EndTaskWithError(ITask task, Exception ex);
	}
}
