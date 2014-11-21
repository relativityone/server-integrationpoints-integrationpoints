using System;
using System.Collections.Generic;
using kCura.ScheduleQueueAgent.ScheduleRules;

namespace kCura.ScheduleQueueAgent
{
	public interface IJobService
	{
		string QueueTable { get; }
		AgentInformation GetAgentInformation(int agentID);
		AgentInformation GetAgentInformation(Guid agentGuid);
		Job GetNextQueueJob(AgentInformation agentInfo, IEnumerable<int> resourceGroupIds);
		ITask GetTask(Job job);
		void FinalizeJob(Job job, TaskResult taskResult);
		void UnlockJobs(int agentID);
		void CreateQueueTable();
		Job CreateJob(AgentInformation agentInfo, int workspaceID, int relatedObjectArtifactID, string taskType,
			IScheduleRule scheduleRule, string jobDetail, int SubmittedBy);
		Job CreateJob(AgentInformation agentInfo, int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, string jobDetail, int SubmittedBy);
		void DeleteJob(long jobID);
		Job GetJob(long jobID);
		Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName);

		//TODO: Implement
		//void DeleteMethodAgentJob(int relatedObjectArtifactID, MethodJobType jobType, bool isScheduled);
		//bool IsWorkspaceActive(int workspaceID);
	}
}
