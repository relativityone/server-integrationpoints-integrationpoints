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
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class DataTransferLocationController : ApiController
	{
		private readonly IDataTransferLocationService _dataTransferLocationService;
		private readonly IRepositoryFactory _respositoryFactory;
		private readonly IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreator;

		public DataTransferLocationController(IDataTransferLocationService dataTransferLocationService, IRepositoryFactory respositoryFactory, IDirectoryTreeCreator<JsTreeItemDTO> directoryTreeCreator)
		{
			_dataTransferLocationService = dataTransferLocationService;
			_respositoryFactory = respositoryFactory;
			_directoryTreeCreator = directoryTreeCreator;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve data transfer location directory structure.")]
		public HttpResponseMessage GetStructure(int workspaceId, Guid integrationPointTypeIdentifier, [FromBody] string path, bool isRoot = true, bool includeFiles = false)
		{
			if (HasPermissions(workspaceId))
			{
				if (String.IsNullOrWhiteSpace(path))
				{
					path = _dataTransferLocationService.GetLocationFor(workspaceId, integrationPointTypeIdentifier);
				}

				List<JsTreeItemDTO> tree = _directoryTreeCreator.GetChildren(path, isRoot, includeFiles);

				return Request.CreateResponse(HttpStatusCode.OK, tree);
			}

			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		private bool HasPermissions(int workspaceId)
		{
			try
			{
				var permissionRepository = _respositoryFactory.GetPermissionRepository(workspaceId);
				var integrationPointGuid = new Guid(ObjectTypeGuids.IntegrationPoint);

				return (permissionRepository.UserCanExport() || permissionRepository.UserCanImport())
					&& permissionRepository.UserHasPermissionToAccessWorkspace()
					&& (permissionRepository.UserHasArtifactTypePermission(integrationPointGuid, ArtifactPermission.Edit) || permissionRepository.UserHasArtifactTypePermission(integrationPointGuid, ArtifactPermission.Create));
			}
			catch (Exception ex)
			{
				throw new Exception("Unexpected error occured when cheching user permissions", ex);
			}
		}
	}
}