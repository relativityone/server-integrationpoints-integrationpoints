using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class DataTransferLocationController : ApiController
    {
        private readonly IRepositoryFactory _respositoryFactory;
        private readonly IRelativePathDirectoryTreeCreator<JsTreeItemDTO> _relativePathDirectoryTreeCreator;
        private readonly IDataTransferLocationService _dataTransferLocationService;

        public DataTransferLocationController(IRepositoryFactory respositoryFactory,
            IRelativePathDirectoryTreeCreator<JsTreeItemDTO> relativePathDirectoryTreeCreator, IDataTransferLocationService dataTransferLocationService)
        {
            _respositoryFactory = respositoryFactory;
            _relativePathDirectoryTreeCreator = relativePathDirectoryTreeCreator;
            _dataTransferLocationService = dataTransferLocationService;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve data transfer location directory structure.")]
        public HttpResponseMessage GetStructure(int workspaceId, Guid integrationPointTypeIdentifier, [FromBody] string path, bool isRoot = true, bool includeFiles = false)
        {
            if (HasPermissions(workspaceId))
            {
                List<JsTreeItemDTO> tree = _relativePathDirectoryTreeCreator.GetChildren(path, isRoot, workspaceId, integrationPointTypeIdentifier, includeFiles);

                return Request.CreateResponse(HttpStatusCode.OK, tree);
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve data transfer root location.")]
        public HttpResponseMessage GetRoot(int workspaceId, Guid integrationPointTypeIdentifier)
        {
            if (HasPermissions(workspaceId))
            {
                string relativeDataTransferLocation = _dataTransferLocationService.GetDefaultRelativeLocationFor(integrationPointTypeIdentifier);

                return Request.CreateResponse(HttpStatusCode.OK, relativeDataTransferLocation);
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        private bool HasPermissions(int workspaceId)
        {
            try
            {
                IPermissionRepository permissionRepository = _respositoryFactory.GetPermissionRepository(workspaceId);
                var integrationPointGuid = new Guid(ObjectTypeGuids.IntegrationPoint);

                return (permissionRepository.UserCanExport() || permissionRepository.UserCanImport())
                    && permissionRepository.UserHasPermissionToAccessWorkspace()
                    && (permissionRepository.UserHasArtifactTypePermission(integrationPointGuid, ArtifactPermission.Edit) || permissionRepository.UserHasArtifactTypePermission(integrationPointGuid, ArtifactPermission.Create));
            }
            catch (Exception ex)
            {
                throw new Exception(Constants.PERMISSION_CHECKING_UNEXPECTED_ERROR, ex);
            }
        }
    }
}
