using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Castle.Core.Internal;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Contracts.Helpers;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("Deletes any corresponding jobs")]
	[Guid("5EA14201-EEBE-4D1D-99FA-2E28C9FAB7F4")]
	public class DeleteEventHandler : PreDeleteEventHandler
	{
		private IAgentService _agentService;

		private IJobService _jobService;

		public IAgentService AgentService
		{
			get
			{
				if (_agentService == null)
				{
					_agentService = new AgentService(Helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
				}
				return _agentService;
			}
		}

		public IJobService JobService
		{
			get
			{
				if (_jobService == null)
				{
					_jobService = new JobService(AgentService, Helper);
				}
				return _jobService;
			}
		}

		public override FieldCollection RequiredFields =>
			new FieldCollection {new Field(Guid.Parse(IntegrationPointFieldGuids.JobHistory))};

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
				int workspaceId = Helper.GetActiveCaseID();
				int integrationPointId = ActiveArtifact.ArtifactID;
				IEnumerable<Job> jobs = JobService.GetScheduledJobs(workspaceId, integrationPointId,
					TaskTypeHelper.GetManagerTypes()
						.Select(taskType => taskType.ToString())
						.ToList());

				jobs.ForEach(job => JobService.DeleteJob(job.JobId));
			}
			catch (Exception ex)
			{
				LogDeletingJobsError(ex);
				eventResponse.Success = false;
				eventResponse.Message = $"Failed to delete corresponding job(s). Error: {ex.Message}";
			}

			return eventResponse;
		}

		public override void Rollback()
		{
			//Do nothing
		}

		#region Logging

		private void LogDeletingJobsError(Exception ex)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<DeleteEventHandler>();
			logger.LogError(ex, "Failed to delete corresponding job(s).");
		}

		#endregion
	}
}