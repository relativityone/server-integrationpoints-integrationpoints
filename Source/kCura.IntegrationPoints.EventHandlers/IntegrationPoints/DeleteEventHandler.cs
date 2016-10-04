using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
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

		public override Response Execute()
		{
			Response eventResponse = new Response
			{
				Success = true,
				Message = string.Empty
			};

			try
			{
				int workspaceId = this.Helper.GetActiveCaseID();
				int integrationPointId = this.ActiveArtifact.ArtifactID;
				IEnumerable<Job> jobs = JobService.GetScheduledJob(workspaceId, integrationPointId, 
					TaskTypeHelper.GetManagerTypes()
					.Select(taskType => taskType.ToString())
					.ToList());
				
				jobs.ForEach(job => JobService.DeleteJob(job.JobId));
			}
			catch (Exception ex)
			{
				eventResponse.Success = false;
				eventResponse.Message = $"Failed to delete corresponding job(s). Error: {ex.Message}";
			}

			return eventResponse;
		}

		public override FieldCollection RequiredFields => 
			new FieldCollection {new Field(Guid.Parse(IntegrationPointFieldGuids.JobHistory))};

		public override void Rollback()
		{
			//Do nothing
		}
	}
}
