using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public enum TaskType
	{
		None,
		SyncManager,
		SyncWorker,
		SyncCustodianManagerWorker,
		SendEmailManager,
		SendEmailWorker,
		ExportService
	}

	public interface IJobManager
	{
		void CreateJobOnBehalfOfAUser<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, int userId, long? rootJobId = null, long? parentJobId = null);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, IScheduleRule rule, long? rootJobID = null, long? parentJobID = null);
		void CreateJob<T>(T jobDetails, TaskType task, int workspaceId, int integrationPointId, long? rootJobId = null, long? parentJobId = null);
		void CreateJob<T>(Job parentJob, T jobDetails, TaskType task);
		void DeleteJob(long jobID);
		Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName);
		void CreateJobWithTracker<T>(Job parentJob, T jobDetails, TaskType type, string batchId);
		bool CheckBatchOnJobComplete(Job job, string batchId);

	}
}
