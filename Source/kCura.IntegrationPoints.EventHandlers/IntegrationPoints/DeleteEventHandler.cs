using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Factories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("Deletes any corresponding jobs")]
	[Guid("5EA14201-EEBE-4D1D-99FA-2E28C9FAB7F4")]
	public class DeleteEventHandler : PreDeleteEventHandler
	{
		private ICorrespondingJobDelete _correspondingJobDelete;
		private IIntegrationPointSecretDelete _integrationPointSecretDelete;

		internal ICorrespondingJobDelete CorrespondingJobDelete
		{
			get { return _correspondingJobDelete ?? (_correspondingJobDelete = CorrespondingJobDeleteFactory.Create(Helper)); }
			set { _correspondingJobDelete = value; }
		}

		internal IIntegrationPointSecretDelete IntegrationPointSecretDelete
		{
			get { return _integrationPointSecretDelete ?? (_integrationPointSecretDelete = IntegrationPointSecretDeleteFactory.Create(Helper)); }
			set { _integrationPointSecretDelete = value; }
		}

		public override FieldCollection RequiredFields => null;

		public override void Commit()
		{
			//Do nothing
		}

		public override Response Execute()
		{
			int workspaceId = Helper.GetActiveCaseID();
			int integrationPointId = ActiveArtifact.ArtifactID;

			try
			{
				CorrespondingJobDelete.DeleteCorrespondingJob(workspaceId, integrationPointId);
			}
			catch (Exception ex)
			{
				LogDeletingJobsError(ex);
				return new Response
				{
					Success = false,
					Message = $"Failed to delete corresponding job(s). Error: {ex.Message}",
					Exception = ex
				};
			}

			try
			{
				IntegrationPointSecretDelete.DeleteSecret(integrationPointId);
			}
			catch (Exception ex)
			{
				LogDeletingSecretError(ex);
				return new Response
				{
					Success = false,
					Message = $"Failed to delete corresponding secret. Error: {ex.Message}",
					Exception = ex
				};
			}

			return new Response
			{
				Success = true,
				Message = "Integration Point successfully deleted."
			};
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

		private void LogDeletingSecretError(Exception ex)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<DeleteEventHandler>();
			logger.LogError(ex, "Failed to delete corresponding secret.");
		}

		#endregion
	}
}