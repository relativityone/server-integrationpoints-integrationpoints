using System.Collections.Generic;

namespace kCura.Agent.ScheduleQueueAgent
{
	public interface IJobService
	{
		string QueueTable { get; }
		AgentInformation GetAgentInformation(int agentID);
		Job GetNextJob(int agentId, IEnumerable<int> resourceGroupIds);
		ITask GetTask(Job job);
		void FinalizeJob(Job job, TaskResult taskResult);
		void UnlockJobs(int agentID);
		void CreateQueueTable();

		//TODO: Implement
		//Job CreateMethodAgentJob<T>(int userArtifactID, int workspcaseID, MethodJobType jobType, int? relatedObjectArtifactID, IScheduleRules scheduleRules, IJobArtifacts<T> jobDetail, MethodJobStatus initialJobStatus = MethodJobStatus.Pending, int jobFlags = 0);
		//void DeleteMethodAgentJob(int jobID);
		//void DeleteMethodAgentJob(int relatedObjectArtifactID, MethodJobType jobType, bool isScheduled);
		//bool IsWorkspaceActive(int workspaceID);
	}
}
