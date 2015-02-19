using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public enum TaskType
	{
		None,
		SyncManager,
		SyncWorker,
		SyncCustodianManagerWorker
	}

	public interface IJobManager
	{
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, IScheduleRule rule);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID);
		void CreateJob(int workspaceID, int integrationPointID, TaskType task, string serializedDetails);

		//void CreateJob<T>(T jobDetails, TaskType task); //schedule rules
	}
}
