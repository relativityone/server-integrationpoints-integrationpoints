using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[kCura.EventHandler.CustomAttributes.Description("Deletes any corresponding jobs")]
	[System.Runtime.InteropServices.Guid("5EA14201-EEBE-4D1D-99FA-2E28C9FAB7F4")]
	public class DeleteEventHandler : kCura.EventHandler.PreDeleteEventHandler
	{

		private IAgentService _agentService;
		public IAgentService AgentService
		{
			get
			{
				if (_agentService == null)
				{ _agentService = new AgentService(this.Helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)); }
				return _agentService;
			}
		}

		private IJobService _jobService;
		public IJobService JobService
		{
			get
			{
				if (_jobService == null)
				{
					_jobService = new JobService(this.AgentService, this.Helper);
				}
				return _jobService;
			}
		}
		public override void Commit()
		{
			//Do nothing
		}

		public override EventHandler.Response Execute()
		{
			kCura.EventHandler.Response eventResponse = new kCura.EventHandler.Response();
			eventResponse.Success = true;
			eventResponse.Message = String.Empty;

			try
			{
				int workspaceID = this.Helper.GetActiveCaseID();
				int integrationPointID = this.ActiveArtifact.ArtifactID;
				Job job = JobService.GetJob(workspaceID, integrationPointID, TaskType.SyncManager.ToString());
				if (job != null)
				{
					this.JobService.DeleteJob(job.JobId);
				}
			}
			catch (Exception ex)
			{
				eventResponse.Success = false;
				eventResponse.Message = String.Format("Failed to delete corresponding job. Error: {0}", ex.Message);
			}

			return eventResponse;
		}

		public override EventHandler.FieldCollection RequiredFields
		{
			get { return null; }
		}

		public override void Rollback()
		{
			//Do nothing
		}
	}
}
