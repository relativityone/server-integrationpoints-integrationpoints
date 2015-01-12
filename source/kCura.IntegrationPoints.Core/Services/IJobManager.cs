using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Services
{
	public enum TaskType
	{
		None,
		SyncManager,
		SyncWorker
	}

	public interface IJobManager
	{
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, IScheduleRule rule);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID);
		//void CreateJob<T>(T jobDetails, TaskType task); //schedule rules
	}
}
