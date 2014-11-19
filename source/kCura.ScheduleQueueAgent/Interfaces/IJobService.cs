using System.Collections.Generic;

namespace kCura.ScheduleQueueAgent
{
	public interface IJobService
	{
		string QueueTable { get; }
		AgentInformation GetAgentInformation(int agentID);
		Job GetNextQueueJob(AgentInformation agentInfo, IEnumerable<int> resourceGroupIds);
		ITask GetTask(Job job);
		void FinalizeJob(Job job, TaskResult taskResult);
		void UnlockJobs(int agentID);
		void CreateQueueTable();
		Job CreateJob(int workspaceID, int? relatedObjectArtifactID, string taskType, IScheduleRule scheduleRules, string jobDetail, int SubmittedBy);
		void DeleteJob(long jobID);
		void GetJob(long jobID);
		void GetJob(int workspaceID, int relatedObjectArtifactID);

		//TODO: Implement
		//void DeleteMethodAgentJob(int relatedObjectArtifactID, MethodJobType jobType, bool isScheduled);
		//bool IsWorkspaceActive(int workspaceID);
	}
}
