﻿using System;
using System.Collections.Generic;
using kCura.ScheduleQueue.Core.ScheduleRules;

namespace kCura.ScheduleQueue.Core
{
	public interface IJobService
	{
		Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID);
		ITask GetTask(Job job);
		DateTime? GetJobNextUtcRunDateTime(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult);
		FinalizeJobResult FinalizeJob(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult);
		void UnlockJobs(int agentID);
		Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			IScheduleRule scheduleRule, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID);
		Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID);
		void DeleteJob(long jobID);
		Job GetJob(long jobID);
		Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName);

		//TODO: Implement
		//bool IsWorkspaceActive(int workspaceID);
	}
}
