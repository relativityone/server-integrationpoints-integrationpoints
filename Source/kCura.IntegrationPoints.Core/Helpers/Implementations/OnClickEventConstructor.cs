using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class OnClickEventConstructor : IOnClickEventConstructor
	{
		private readonly IContextContainer _contextContainer;
		private readonly IManagerFactory _managerFactory;

		public OnClickEventConstructor(IContextContainer contextContainer, IManagerFactory managerFactory)
		{
			_managerFactory = managerFactory;
			_contextContainer = contextContainer;
		}

		public OnClickEventDTO GetOnClickEvents(int workspaceId, int integrationPointId, ButtonStateDTO buttonStates)
		{
			string runEvent = buttonStates.RunButtonEnabled ? GetActionButtonRunEvent(integrationPointId, workspaceId) : string.Empty;
			string retryErrorsEvent = buttonStates.RetryErrorsButtonEnabled ? GetActionButtonRetryEvent(integrationPointId, workspaceId) : string.Empty;
			string viewErrorsEvent = buttonStates.ViewErrorsLinkEnabled ? GetViewErrorsLinkEvent(workspaceId, integrationPointId) : string.Empty;
			string stopEvent = buttonStates.StopButtonEnabled ? GetActionButtonStopEvent(integrationPointId, workspaceId) : string.Empty;
			string saveAsProfileEvent = GetActionButtonSaveAsProfile(integrationPointId, workspaceId);

			return new OnClickEventDTO
			{
				RunOnClickEvent = runEvent,
				RetryErrorsOnClickEvent = retryErrorsEvent,
				ViewErrorsOnClickEvent = viewErrorsEvent,
				StopOnClickEvent = stopEvent,
				SaveAsProfileOnClickEvent = saveAsProfileEvent
			};
		}

		private string GetActionButtonRunEvent(int integrationPointId, int workspaceId)
		{
			return $"IP.importNow({integrationPointId},{workspaceId})";
		}

		private string GetActionButtonStopEvent(int integrationPointId, int workspaceId)
		{
			return $"IP.stopJob({integrationPointId},{workspaceId})";
		}

		private string GetActionButtonRetryEvent(int integrationPointId, int workspaceId)
		{
			return $"IP.retryJob({integrationPointId},{workspaceId})";
		}

		private string GetActionButtonSaveAsProfile(int integrationPointId, int workspaceId)
		{
			return $"IP.saveAsProfile({integrationPointId},{workspaceId})";
		}

		private string GetViewErrorsLinkEvent(int workspaceId, int integrationPointId)
		{
			IFieldManager fieldManager = _managerFactory.CreateFieldManager(_contextContainer);
			IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager(_contextContainer);
			IArtifactGuidManager artifactGuidManager = _managerFactory.CreateArtifactGuidManager(_contextContainer);
			IObjectTypeManager objectTypeManager = _managerFactory.CreateObjectTypeManager(_contextContainer);

			var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.ErrorStatus);
			var jobHistoryFieldGuid = new Guid(JobHistoryErrorDTO.FieldGuids.JobHistory);

			Dictionary<Guid, int> guidsAndArtifactIds = artifactGuidManager.GetArtifactIdsForGuids(workspaceId, new[]
			{
				JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New,
				errorErrorStatusFieldGuid,
				jobHistoryFieldGuid
			});

			int jobHistoryErrorStatusArtifactViewFieldId =
				fieldManager.RetrieveArtifactViewFieldId(workspaceId, guidsAndArtifactIds[errorErrorStatusFieldGuid]).GetValueOrDefault();
			int jobHistoryErrorStatusNewChoiceArtifactId = guidsAndArtifactIds[JobHistoryErrorDTO.Choices.ErrorStatus.Guids.New];
			int jobHistoryErrorDescriptorArtifactTypeId = objectTypeManager.RetrieveObjectTypeDescriptorArtifactTypeId(workspaceId, new Guid(JobHistoryErrorDTO.ArtifactTypeGuid));
			int jobHistoryArtifactViewFieldId = fieldManager.RetrieveArtifactViewFieldId(workspaceId, guidsAndArtifactIds[jobHistoryFieldGuid]).GetValueOrDefault();
			int jobHistoryInstanceArtifactId = jobHistoryManager.GetLastJobHistoryArtifactId(workspaceId, integrationPointId);

			string onClickEvent = $"window.location='../../Case/IntegrationPoints/ErrorsRedirect.aspx?ErrorStatusArtifactViewFieldID={jobHistoryErrorStatusArtifactViewFieldId}"
								+ $"&ErrorStatusNewChoiceArtifactId={jobHistoryErrorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorDescriptorArtifactTypeId}"
								+ $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}'; return false;";

			return onClickEvent;
		}
	}
}