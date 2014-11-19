using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.ScheduleQueueAgent.Helpers;
using kCura.ScheduleQueueAgent.Data;
using kCura.ScheduleQueueAgent.Data.Queries;
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

		public Job CreateJob(int workspaceID, int? relatedObjectArtifactID, string taskType, IScheduleRule scheduleRules, string jobDetail, int SubmittedBy)
		{
			CreateQueueTableOnce();
			throw new NotImplementedException();
		}

		public void DeleteJob(long jobID)
		{
			throw new NotImplementedException();
		}

		public void GetJob(long jobID)
		{
			CreateQueueTableOnce();
			throw new NotImplementedException();
		}

		public void GetJob(int workspaceID, int relatedObjectArtifactID)
		{
			CreateQueueTableOnce();
			throw new NotImplementedException();
		}
	}
}
