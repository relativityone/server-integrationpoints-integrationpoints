using kCura.ScheduleQueue.Core;
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
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, IScheduleRule rule, long? rootJobID = null, long? parentJobID = null);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceID, int integrationPointID, long? rootJobID = null, long? parentJobID = null);
		void CreateJob<T>(Job parentJob, T jobDetails, TaskType task);
	}
}
