using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Web.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ErrorController : ApiController
    {
        private readonly IManagerFactory _managerFactory;

        public ErrorController(IManagerFactory managerFactory)
        {
            _managerFactory = managerFactory;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to build path")]
        public IHttpActionResult GetViewErrorsLink(int integrationPointId, int workspaceId)
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

            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("ErrorViewRedirect", UrlVersionDecorator.AppendVersion(newLocation)));
            return Ok(result);
        }
    }
}
