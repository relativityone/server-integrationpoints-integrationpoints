using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Common.Context;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Interfaces.TextSanitizer;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Utils;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class WorkspaceFinderController : ApiController
    {
        private readonly ITextSanitizer _textSanitizer;
        private readonly IManagerFactory _managerFactory;
        private readonly IWorkspaceContext _workspaceIdProvider;

        public WorkspaceFinderController(
            IManagerFactory managerFactory,
            ITextSanitizer textSanitizer,
            IWorkspaceContext workspaceIdProvider)
        {
            _managerFactory = managerFactory;
            _textSanitizer = textSanitizer;
            _workspaceIdProvider = workspaceIdProvider;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
        public HttpResponseMessage GetCurrentInstanceWorkspaces()
        {
            IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();
            return GetWorkspaces(workspaceManager);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve the workspace information.")]
        public HttpResponseMessage GetFederatedInstanceWorkspaces(int federatedInstanceId, [FromBody] object credentials)
        {
            IWorkspaceManager workspaceManager = _managerFactory.CreateWorkspaceManager();
            return GetWorkspacesFromFederatedInstance(workspaceManager);
        }

        private HttpResponseMessage GetWorkspaces(IWorkspaceManager workspaceManager)
        {
            int currentWorkspaceId = _workspaceIdProvider.GetWorkspaceID();
            IEnumerable<WorkspaceDTO> userWorkspaces = workspaceManager.GetUserAvailableDestinationWorkspaces(currentWorkspaceId);
            return CreateResponseFromWorkspaceDTOs(userWorkspaces);
        }

        private HttpResponseMessage GetWorkspacesFromFederatedInstance(IWorkspaceManager workspaceManager)
        {
            IEnumerable<WorkspaceDTO> userWorkspaces =
                workspaceManager.GetUserActiveWorkspaces();
            return CreateResponseFromWorkspaceDTOs(userWorkspaces);
        }

        private HttpResponseMessage CreateResponseFromWorkspaceDTOs(IEnumerable<WorkspaceDTO> userWorkspaces)
        {
            WorkspaceModel[] workspaceModels = userWorkspaces.Select(x =>
                new WorkspaceModel
                {
                    DisplayName = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(
                        _textSanitizer.Sanitize(x.Name).SanitizedText,
                        x.ArtifactId
                    ),
                    Value = x.ArtifactId
                }
            ).ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, workspaceModels);
        }
    }
}