using System;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
	public interface ITaskFactoryJobHistoryService
	{
		void SetJobIdOnJobHistory(Job job);
		void UpdateJobHistoryOnFailure(Job job, Exception e);
		void RemoveJobHistoryFromIntegrationPoint(Job job);
	}
}
