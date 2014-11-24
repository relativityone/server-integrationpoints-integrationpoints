using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.ScheduleQueueAgent.Helpers;
using kCura.ScheduleQueueAgent.Data;
using kCura.ScheduleQueueAgent.Data.Queries;
using kCura.ScheduleQueueAgent.ScheduleRules;
using Relativity.API;

namespace kCura.ScheduleQueueAgent.Services
{
	public class JobService : IJobService
	{
		private bool creationOfQTableHasRun = false;
		public JobService(IDBContext dbContext)
		{
			this.QueueTable = new QueueTableHelper().GetQueueTableName();
			this.QDBContext = new QueueDBContext(dbContext, QueueTable);
		}

		public string QueueTable { get; private set; }
		public IQueueDBContext QDBContext { get; private set; }

		public Job GetNextQueueJob(AgentInformation agentInfo, IEnumerable<int> resourceGroupIds)
		{
			Job job = null;
			DataRow row = new GetNextJob(QDBContext).Execute(agentInfo.AgentID, agentInfo.AgentTypeID, resourceGroupIds.ToArray());
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

		public void FinalizeJob(Job job, TaskResult taskResult)
		{
			IScheduleRule scheduleRule = job.ScheduleRule;
			DateTime? nextUtcRunDateTime = null;

#if TIME_MACHINE
			//TODO: implement
#endif

			if (scheduleRule != null)
			{
				nextUtcRunDateTime = scheduleRule.GetNextUTCRunDateTime(DateTime.UtcNow, taskResult.Status);
			}

			if (nextUtcRunDateTime.HasValue)
			{
				new Data.Queries.UpdateScheduledJob(QDBContext).Execute(job.JobId, nextUtcRunDateTime.Value);
				//TODO: implement logging
				//log.Log(job, JobHistoryState.Modified, null, string.Format("Job is re-scheduled for {0}", nextRunTime.ToString()));
			}
			else
			{
				DeleteJob(job.JobId);
				//TODO: implement logging
				//log.Log(job, JobHistoryState.Deleted);
			}
		}

		public void UnlockJobs(int agentID)
		{
			throw new NotImplementedException();
		}

		public void CreateQueueTable()
		{
			new CreateScheduleQueueTable(QDBContext).Execute();
		}

		private void CreateQueueTableOnce()
		{
			if (!creationOfQTableHasRun) CreateQueueTable();
			creationOfQTableHasRun = true;
		}

		public AgentInformation GetAgentInformation(int agentID)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(QDBContext).Execute(agentID);
			if (row != null)
			{
				agentInformation = new AgentInformation(row);
			}
			return agentInformation;
		}

		public AgentInformation GetAgentInformation(Guid agentGuid)
		{
			AgentInformation agentInformation = null;
			DataRow row = new GetAgentInformation(QDBContext).Execute(agentGuid);
			if (row != null)
			{
				agentInformation = new AgentInformation(row);
			}
			return agentInformation;
		}

		public Job CreateJob(AgentInformation agentInfo, int workspaceID, int relatedObjectArtifactID, string taskType,
													IScheduleRule scheduleRule, string jobDetail, int SubmittedBy)
		{
			CreateQueueTableOnce();

			Job job = null;
			DateTime? nextRunTime = scheduleRule.GetNextUTCRunDateTime(null, null);
			string serializedScheduleRule = scheduleRule.ToSerializedString();
			if (nextRunTime.HasValue)
			{
				DataRow row = new CreateScheduledJob(QDBContext).Execute(
					workspaceID,
					relatedObjectArtifactID,
					taskType,
					nextRunTime.Value,
					agentInfo.AgentTypeID,
					serializedScheduleRule,
					jobDetail,
					0,
					SubmittedBy);

				if (row != null) job = new Job(row);
			}
			return job;
		}

		public Job CreateJob(AgentInformation agentInfo, int workspaceID, int relatedObjectArtifactID, string taskType,
													DateTime nextRunTime, string jobDetail, int SubmittedBy)
		{
			CreateQueueTableOnce();

			Job job = null;
			DataRow row = new CreateScheduledJob(QDBContext).Execute(
				workspaceID,
				relatedObjectArtifactID,
				taskType,
				nextRunTime,
				agentInfo.AgentTypeID,
				null,
				jobDetail,
				0,
				SubmittedBy);

			if (row != null) job = new Job(row);

			return job;
		}

		public void DeleteJob(long jobID)
		{
			new DeleteJob(QDBContext).Execute(jobID);
		}

		public Job GetJob(long jobID)
		{
			CreateQueueTableOnce();

			Job job = null;
			DataRow row = new GetJob(QDBContext).Execute(jobID);
			if (row != null) job = new Job(row);

			return job;
		}

		public Job GetJob(int workspaceID, int relatedObjectArtifactID, string taskName)
		{
			CreateQueueTableOnce();

			Job job = null;
			DataRow row = new GetJob(QDBContext).Execute(workspaceID, relatedObjectArtifactID, taskName);
			if (row != null) job = new Job(row);

			return job;
		}
	}
}
