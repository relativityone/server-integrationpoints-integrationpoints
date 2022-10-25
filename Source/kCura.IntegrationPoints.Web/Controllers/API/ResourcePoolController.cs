using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ResourcePoolController : ApiController
    {
        private readonly IResourcePoolManager _resourcePoolManager;
        private readonly IRepositoryFactory _respositoryFactory;
        private readonly IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreator;

        public ResourcePoolController(
            IResourcePoolManager resourcePoolManager,
            IRepositoryFactory respositoryFactory,
            IDirectoryTreeCreator<JsTreeItemDTO> directoryTreeCreator)
        {
            _resourcePoolManager = resourcePoolManager;
            _respositoryFactory = respositoryFactory;
            _directoryTreeCreator = directoryTreeCreator;
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to retrieve processing source location list.")]
        public HttpResponseMessage GetProcessingSourceLocations(int workspaceId)
        {
            if (HasPermissions(workspaceId))
            {
                List<ProcessingSourceLocationDTO> processingSourceLocations = _resourcePoolManager.GetProcessingSourceLocation(workspaceId);
                return Request.CreateResponse(HttpStatusCode.OK, processingSourceLocations);
            }
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve processing source location subfolders info.")]
        public HttpResponseMessage GetSubItems(int workspaceId, bool isRoot, [FromBody] string path, bool includeFiles = false)
        {
            if (HasPermissions(workspaceId))
            {
                List<JsTreeItemDTO> subItems = _directoryTreeCreator.GetChildren(path, isRoot, includeFiles);
                return Request.CreateResponse(HttpStatusCode.OK, subItems);
            }
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to determine if processing source location is enabled.")]
        public HttpResponseMessage IsProcessingSourceLocationEnabled(int workspaceId)
        {
            if (HasPermissions(workspaceId))
            {
                return Request.CreateResponse(HttpStatusCode.OK, false);
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        private bool HasPermissions(int workspaceId)
        {
            try
            {
                IPermissionRepository permissionRepository = _respositoryFactory.GetPermissionRepository(workspaceId);
                return (
                            permissionRepository.UserCanExport() || permissionRepository.UserCanImport()
                        )
                        && permissionRepository.UserHasPermissionToAccessWorkspace()
                        &&
                        (
                            permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Edit)
                            || permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create)
                        );

            }
            catch (Exception ex)
            {
                throw new Exception(Constants.PERMISSION_CHECKING_UNEXPECTED_ERROR, ex);
            }
        }
    }
}
