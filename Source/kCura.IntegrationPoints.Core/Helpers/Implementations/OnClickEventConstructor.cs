using System;
using System.Collections.Generic;
using System.Web;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class OnClickEventConstructor : IOnClickEventConstructor
    {
        private readonly IManagerFactory _managerFactory;

        public OnClickEventConstructor(IManagerFactory managerFactory)
        {
            _managerFactory = managerFactory;
        }

        public OnClickEventDTO GetOnClickEvents(int workspaceId, int integrationPointId, string integrationPointName, ButtonStateDTO buttonStates)
        {
            string runEvent = buttonStates.RunButtonEnabled ? GetActionButtonRunEvent(integrationPointId, workspaceId) : string.Empty;
            string retryErrorsEvent = buttonStates.RetryErrorsButtonEnabled ? GetActionButtonRetryEvent(integrationPointId, workspaceId) : string.Empty;
            string viewErrorsEvent = buttonStates.ViewErrorsLinkEnabled ? GetViewErrorsLinkEvent(workspaceId, integrationPointId) : string.Empty;
            string stopEvent = buttonStates.StopButtonEnabled ? GetActionButtonStopEvent(integrationPointId, workspaceId) : string.Empty;
            string saveAsProfileEvent = GetActionButtonSaveAsProfile(integrationPointId, integrationPointName, workspaceId);
            string downloadErrorFileEvent = buttonStates.DownloadErrorFileLinkEnabled ? GetActionButtonDownloadErrorFile(integrationPointId, workspaceId) : string.Empty;

            return new OnClickEventDTO
            {
                RunOnClickEvent = runEvent,
                RetryErrorsOnClickEvent = retryErrorsEvent,
                ViewErrorsOnClickEvent = viewErrorsEvent,
                StopOnClickEvent = stopEvent,
                SaveAsProfileOnClickEvent = saveAsProfileEvent,
                DownloadErrorFileOnClickEvent = downloadErrorFileEvent
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

        private string GetActionButtonSaveAsProfile(int integrationPointId, string integrationPointName, int workspaceId)
        {
            return $"IP.saveAsProfile({integrationPointId},{workspaceId},'{HttpUtility.JavaScriptStringEncode(integrationPointName)}')";
        }

        private string GetActionButtonDownloadErrorFile(int integrationPointId, int workspaceId)
        {
            return $"IP.downloadErrorFile({integrationPointId},{workspaceId})";
        }

        private string GetViewErrorsLinkEvent(int workspaceId, int integrationPointId)
        {
            IFieldManager fieldManager = _managerFactory.CreateFieldManager();
            IJobHistoryManager jobHistoryManager = _managerFactory.CreateJobHistoryManager();
            IArtifactGuidManager artifactGuidManager = _managerFactory.CreateArtifactGuidManager();
            IObjectTypeManager objectTypeManager = _managerFactory.CreateObjectTypeManager();

            var errorErrorStatusFieldGuid = new Guid(JobHistoryErrorFieldGuids.ErrorStatus);
            var jobHistoryFieldGuid = new Guid(JobHistoryErrorFieldGuids.JobHistory);

            Dictionary<Guid, int> guidsAndArtifactIds = artifactGuidManager.GetArtifactIdsForGuids(workspaceId, new[]
            {
                ErrorStatusChoices.JobHistoryErrorNewGuid,
                errorErrorStatusFieldGuid,
                jobHistoryFieldGuid
            });

            int jobHistoryErrorStatusArtifactViewFieldId =
                fieldManager.RetrieveArtifactViewFieldId(workspaceId, guidsAndArtifactIds[errorErrorStatusFieldGuid]).GetValueOrDefault();
            int jobHistoryErrorStatusNewChoiceArtifactId = guidsAndArtifactIds[ErrorStatusChoices.JobHistoryErrorNewGuid];
            int jobHistoryErrorDescriptorArtifactTypeId = objectTypeManager.RetrieveObjectTypeDescriptorArtifactTypeId(workspaceId, new Guid(ObjectTypeGuids.JobHistoryError));
            int jobHistoryArtifactViewFieldId = fieldManager.RetrieveArtifactViewFieldId(workspaceId, guidsAndArtifactIds[jobHistoryFieldGuid]).GetValueOrDefault();
            int jobHistoryInstanceArtifactId = jobHistoryManager.GetLastJobHistoryArtifactId(workspaceId, integrationPointId);

            string newLocation = $"../../Case/IntegrationPoints/ErrorsRedirect.aspx?ErrorStatusArtifactViewFieldID={jobHistoryErrorStatusArtifactViewFieldId}"
                             + $"&ErrorStatusNewChoiceArtifactId={jobHistoryErrorStatusNewChoiceArtifactId}&JobHistoryErrorArtifactTypeId={jobHistoryErrorDescriptorArtifactTypeId}"
                             + $"&JobHistoryArtifactViewFieldID={jobHistoryArtifactViewFieldId}&JobHistoryInstanceArtifactId={jobHistoryInstanceArtifactId}";

            string onClickEvent = $"window.location='{UrlVersionDecorator.AppendVersion(newLocation)}'; return false;";

            return onClickEvent;
        }
    }
}