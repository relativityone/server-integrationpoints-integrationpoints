using System;
using System.Collections.Generic;
using System.Data;
using Relativity.API;

namespace kCura.Agent.ScheduleQueueAgent
{
	public abstract class ScheduleQueueAgentBase : AgentBase
	{
		private IJobService jobService = null;
		public ScheduleQueueAgentBase()
		{
			//TODO: load default services
			//this.jobService = defaultJobService;
		}

		public DBContext DBContext { get; private set; }

		public virtual ITask GetTask(Job job)
		{
			ITask task = jobService.GetTask(job);
			return task;
		}

		public ScheduleQueueAgentBase(DBContext dbContext, IJobService jobService)
		{
			this.jobService = jobService;
			DBContext = dbContext;
		}


		//TODO: implement
		//public virtual bool UseDedicatedQueue{
		//	get { return false; }
		//}

		public sealed override void Execute()
		{
			base.RaiseMessage("Started.", 10);
			//errorRaised = false;
			//var message = string.Format("Connecting to {0}", base.Helper.GetServicesManager().GetServicesURL().PathAndQuery);
			//base.RaiseMessage(message, 20);
			//kCura.Injection.InjectionUtility.InitializeInjection();

			base.RaiseMessage("Check for Queue Table", 20);
			// - checks if queue table exists and creates it if doesn't
			CheckQueueTable();

			base.RaiseMessage("Process jobs", 20);
			ProcessQueueJobs();

			//if (!errorRaised) base.RaiseMessage("Completed.", 10);
		}

		private void CheckQueueTable()
		{
			//TODO: Implement.
			//throw new NotImplementedException();
		}

		public void ProcessQueueJobs()
		{
			Job nextJob = jobService.GetNextJob(base.AgentID, base.GetResourceGroupIDs());
			while (nextJob != null)
			{
				TaskResult taskResult = ExecuteTask(nextJob);

				FinalizeJob(nextJob, taskResult);

				nextJob = jobService.GetNextJob(base.AgentID, base.GetResourceGroupIDs());
			}

			if (base.ToBeRemoved)
			{
				jobService.UnlockJobs(base.AgentID);
			}
		}

		private TaskResult ExecuteTask(Job job)
		{
			TaskResult result = new TaskResult() { Status = TaskStatusEnum.Success, Exceptions = null };
			try
			{
				//Log: Task Started
				ITask task = GetTask(job);
				if (task != null)
				{
					task.Execute(job);
				}
				else
				{
					//??
					throw new Exception("Could not find corresponding Task.");
				}
				//Log: Task Ended
			}
			catch (Exception ex)
			{
				result.Status = TaskStatusEnum.Fail;
				result.Exceptions = new List<Exception>() { ex };
				//LogError(ex, nextJob.WorkspaceArtifactID);
				//Keep this after JobHistoryLog error so we can get it to the error tab before trying to log
				//because this statement could have thrown the exception
				//log.Log(nextJob, JobHistoryState.Error, ex);
			}
			return result;
		}

		private void FinalizeJob(Job job, TaskResult taskResult)
		{
			try
			{
				jobService.FinalizeJob(job, taskResult);
			}
			catch (Exception ex)
			{
				//LogError(ex, nextJob.WorkspaceArtifactID);
			}
		}
	}
}
