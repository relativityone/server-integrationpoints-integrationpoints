﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class OnClickEventConstructor : IOnClickEventConstructor
	{
		private readonly IManagerFactory _managerFactory;
		private readonly IContextContainer _contextContainer;

		public OnClickEventConstructor(IContextContainer contextContainer, IManagerFactory managerFactory)
		{
			_managerFactory = managerFactory;
			_contextContainer = contextContainer;
		}

		public OnClickEventDTO GetOnClickEventsForRelativityProvider(int workspaceId, int integrationPointId, ButtonStateDTO buttonStates)
		{
			string runNowEvent = buttonStates.RunNowButtonEnabled ? $"IP.importNow({integrationPointId},{workspaceId})" : String.Empty;
			string retryErrorsEvent = buttonStates.RetryErrorsButtonEnabled ? $"IP.retryJob({integrationPointId},{workspaceId})" : String.Empty;
			string viewErrorsEvent = buttonStates.ViewErrorsLinkEnabled ? GetViewErrorsLinkEvent(workspaceId, integrationPointId) : String.Empty;
			
			return new OnClickEventDTO()
			{
				RunNowOnClickEvent = runNowEvent,
				RetryErrorsOnClickEvent = retryErrorsEvent,
				ViewErrorsOnClickEvent = viewErrorsEvent,
				StopOnClickEvent	= "alert('OMG OMG CANCEL WAS CLICKED OMG OMG!!')"
			};
		}

		public OnClickEventDTO GetOnClickEventsForNonRelativityProvider(int workspaceId, int integrationPointId)
		{
			return new OnClickEventDTO()
			{
				RunNowOnClickEvent = $"IP.importNow({integrationPointId},{workspaceId})",
				RetryErrorsOnClickEvent = String.Empty,
				ViewErrorsOnClickEvent = String.Empty,
				StopOnClickEvent	= "alert('OMG OMG CANCEL WAS CLICKED OMG OMG!!')"
			};
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

			int jobHistoryErrorStatusArtifactViewFieldId = fieldManager.RetrieveArtifactViewFieldId(workspaceId, guidsAndArtifactIds[errorErrorStatusFieldGuid]).GetValueOrDefault();
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
