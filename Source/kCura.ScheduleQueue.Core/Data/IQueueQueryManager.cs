﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.ScheduleQueue.Core.Data
{
	public interface IQueueQueryManager
	{
		ICommand CreateScheduleQueueTable();

		ICommand AddStopStateColumnToQueueTable();

		IQuery<DataRow> GetAgentTypeInformation(Guid agentGuid);

		IQuery<DataTable> GetNextJob(int agentId, int agentTypeId, int[] resourceGroupArtifactId);

		IQuery<DataTable> GetNextJob(int agentId, int agentTypeId);

		ICommand UpdateScheduledJob(long jobId, DateTime nextUtcRunTime);

		ICommand UnlockScheduledJob(int agentId);

		ICommand UnlockJob(long jobId);

		ICommand DeleteJob(long jobId);

		IQuery<DataTable> CreateScheduledJob(int workspaceID, int relatedObjectArtifactID,
			string taskType, DateTime nextRunTime, int AgentTypeID, string scheduleRuleType,
			string serializedScheduleRule, string jobDetails, int jobFlags, int SubmittedBy, long? rootJobID, long? parentJobID = null);

		ICommand CreateNewAndDeleteOldScheduledJob(long oldScheduledJobId, int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, int AgentTypeID, string scheduleRuleType, string serializedScheduleRule,
			string jobDetails, int jobFlags, int SubmittedBy, long? rootJobID, long? parentJobID = null);

		ICommand CleanupJobQueueTable();

		IQuery<DataTable> GetAllJobs();
		
		IQuery<int> UpdateStopState(IList<long> jobIds, StopState state);

		IQuery<DataTable> GetJobByRelatedObjectIdAndTaskType(int workspaceId, int relatedObjectArtifactId,
			List<string> taskTypes);

		IQuery<DataTable> GetJobsByIntegrationPointId(long integrationPointId);

		IQuery<DataTable> GetJob(long jobId);

		ICommand UpdateJobDetails(long jobId, string jobDetails);
	}
}
