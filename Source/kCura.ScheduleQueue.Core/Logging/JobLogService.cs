using System;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Logging
{
	public class JobLogService
	{
		private bool jobLogTableCreated = false;
		private IAgentHelper helper = null;
		public JobLogService(IAgentHelper helper)
		{
			this.helper = helper;
		}

		public void Log(AgentTypeInformation agentTypeInfo, Job job, JobLogState state, string details = null)
		{
			try
			{
				SetLocalQDBContext(agentTypeInfo, job);

				CreateLogTableOnce();

				new InsertJobLogEntry(qDBContext).Execute(
						job.JobId,
						job.RootJobId,
						job.ParentJobId,
						job.TaskType,
						(int)state,
						job.LockedByAgentID,
						job.WorkspaceID,
						job.RelatedObjectArtifactID,
						job.SubmittedBy,
						details
				);
			}
			catch (Exception ex)
			{
				SystemEventLoggingService.WriteErrorEvent(agentTypeInfo.Name, "Application", ex);
			}
		}


		private IQueueDBContext qDBContext = null;
		private int savedWorkspaceID = int.MinValue;
		private void SetLocalQDBContext(AgentTypeInformation agentInfo, Job job)
		{
			if (qDBContext == null
				|| !savedWorkspaceID.Equals(job.WorkspaceID))
			{
				qDBContext = null;
				IDBContext context = helper.GetDBContext(job.WorkspaceID);
				string logTableName = string.Format("AgentJobLog_{0}", agentInfo.GUID.ToString().ToUpper());
				qDBContext = new QueueDBContext(helper, logTableName);
				savedWorkspaceID = job.WorkspaceID;
				jobLogTableCreated = false;
			}
		}

		private void CreateLogTable()
		{
			new CreateJobLogTable(qDBContext).Execute();
		}

		private void CreateLogTableOnce()
		{
			if (!jobLogTableCreated)
			{
				CreateLogTable();
				jobLogTableCreated = true;
			}
		}
	}
}
