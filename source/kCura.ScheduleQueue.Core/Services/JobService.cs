﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Services
{
	public class JobService : IJobService
	{
		public JobService(IAgentService agentService, IHelper dbHelper)
		{
			this.AgentService = agentService;
			this.QDBContext = new QueueDBContext(dbHelper, this.AgentService.QueueTable);
		}

		public IAgentService AgentService { get; private set; }
		public IQueueDBContext QDBContext { get; private set; }


		public AgentTypeInformation AgentTypeInformation
		{
			get { return AgentService.AgentTypeInformation; }
		}

		public Job GetNextQueueJob(IEnumerable<int> resourceGroupIds, int agentID)
		{
			Job job = null;
			DataRow row = new GetNextJob(QDBContext).Execute(agentID, AgentTypeInformation.AgentTypeID, resourceGroupIds.ToArray());
			if (row != null)
			{
				job = new Job(row);
			}
			return job;
		}

		public ITask GetTask(Job job)
		{
			//TODO: possibly implement generic way through reflection
			return null;
		}

		public DateTime? GetJobNextUtcRunDateTime(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
		{
			FinalizeJobResult result = new FinalizeJobResult();

			IScheduleRule scheduleRule = scheduleRuleFactory.Deserialize(job);
			DateTime? nextUtcRunDateTime = null;
			if (scheduleRule != null)
			{
#if TIME_MACHINE
				scheduleRule.TimeService = new TimeMachineService(job.WorkspaceID);
#endif
				nextUtcRunDateTime = scheduleRule.GetNextUTCRunDateTime(DateTime.UtcNow, taskResult.Status);
			}
			return nextUtcRunDateTime;
		}

		public FinalizeJobResult FinalizeJob(Job job, IScheduleRuleFactory scheduleRuleFactory, TaskResult taskResult)
		{
			FinalizeJobResult result = new FinalizeJobResult();

			DateTime? nextUtcRunDateTime = GetJobNextUtcRunDateTime(job, scheduleRuleFactory, taskResult);
			if (nextUtcRunDateTime.HasValue)
			{
				new Data.Queries.UpdateScheduledJob(QDBContext).Execute(job.JobId, nextUtcRunDateTime.Value);
				result.JobState = JobLogState.Modified;
				result.Details = string.Format("Job is re-scheduled for {0}", nextUtcRunDateTime.ToString());
			}
			else
			{
				DeleteJob(job.JobId);
				result.JobState = JobLogState.Deleted;
			}
			return result;
		}

		public void UnlockJobs(int agentID)
		{
			new UnlockScheduledJob(QDBContext).Execute(agentID);
		}

		public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			IScheduleRule scheduleRule, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
		{
			AgentService.CreateQueueTableOnce();

			Job job = null;
#if TIME_MACHINE
			scheduleRule.TimeService = new TimeMachineService(workspaceID);
#endif
			DateTime? nextRunTime = scheduleRule.GetNextUTCRunDateTime(null, null);
			string serializedScheduleRule = scheduleRule.ToSerializedString();
			if (nextRunTime.HasValue)
			{
				DataRow row = new CreateScheduledJob(QDBContext).Execute(
					workspaceID,
					relatedObjectArtifactID,
					taskType,
					nextRunTime.Value,
					AgentTypeInformation.AgentTypeID,
					scheduleRule.GetType().AssemblyQualifiedName,
					serializedScheduleRule,
					jobDetails,
					0,
					SubmittedBy,
					rootJobID,
					parentJobID);

				if (row != null) job = new Job(row);
			}
			else
			{
				job = GetScheduledJob(workspaceID, relatedObjectArtifactID, taskType);
				DeleteJob(job.JobId);
			}
			return job;
		}

		public Job CreateJob(int workspaceID, int relatedObjectArtifactID, string taskType,
			DateTime nextRunTime, string jobDetails, int SubmittedBy, long? rootJobID, long? parentJobID)
		{
			AgentService.CreateQueueTableOnce();

			Job job = null;
			DataRow row = new CreateScheduledJob(QDBContext).Execute(
				workspaceID,
				relatedObjectArtifactID,
				taskType,
				nextRunTime,
				AgentTypeInformation.AgentTypeID,
				null,
				null,
				jobDetails,
				0,
				SubmittedBy,
				rootJobID,
				parentJobID);

			if (row != null) job = new Job(row);

			return job;
		}

		public void DeleteJob(long jobID)
		{
			new DeleteJob(QDBContext).Execute(jobID);
		}

		public Job GetJob(long jobID)
		{
			AgentService.CreateQueueTableOnce();

			Job job = null;
			DataRow row = new GetJob(QDBContext).Execute(jobID);
			if (row != null) job = new Job(row);

			return job;
		}

		public Job GetScheduledJob(int workspaceID, int relatedObjectArtifactID, string taskName)
		{
			AgentService.CreateQueueTableOnce();

			Job job = null;
			DataRow row = new GetJob(QDBContext).Execute(workspaceID, relatedObjectArtifactID, taskName);
			if (row != null) job = new Job(row);

			return job;
		}
	}
}
