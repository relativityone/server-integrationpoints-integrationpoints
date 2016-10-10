using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.IntegrationPoints.Web.Extensions;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ResourcePoolController : ApiController
	{
		#region Fields

		private readonly IResourcePoolManager _resourcePoolManager;
		private readonly IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreator;
		private readonly IErrorRepository _errorRepository;

		#endregion //Fields

		#region Constructors

		public ResourcePoolController(IResourcePoolManager resourcePoolManager, IDirectoryTreeCreator<JsTreeItemDTO> directoryTreeCreator,
			IRepositoryFactory repositoryFactory)
		{
			_resourcePoolManager = resourcePoolManager;
			_directoryTreeCreator = directoryTreeCreator;
			_errorRepository = repositoryFactory.GetErrorRepository();
		}

		#endregion //Constructors

		#region Methods

		[HttpGet]
		public HttpResponseMessage GetProcessingSourceLocations(int workspaceId)
		{
			try
			{
				List<ProcessingSourceLocationDTO> processingSourceLocations = _resourcePoolManager.GetProcessingSourceLocation(workspaceId);
				return Request.CreateResponse(HttpStatusCode.OK, processingSourceLocations);
			}
			catch (Exception ex)
			{
				string errMsg =
					$"Unable to retrieve processing source location for {workspaceId} workspace. Please contact system administrator.";
				this.HandleError(workspaceId, _errorRepository, ex, errMsg);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, errMsg);
			}
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve processing source location folder structure (Folder is not accessible).")]
		public HttpResponseMessage GetProcessingSourceLocationStructure(int workspaceId, int artifactId)
		{
			List<ProcessingSourceLocationDTO> processingSourceLocations =
					_resourcePoolManager.GetProcessingSourceLocation(workspaceId);

			ProcessingSourceLocationDTO foundProcessingSourceLocation = processingSourceLocations.FirstOrDefault(
				processingSourceLocation => processingSourceLocation.ArtifactId == artifactId);

			if (foundProcessingSourceLocation == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound, $"Cannot find processing source location {artifactId}");
			}
			JsTreeItemDTO rootFolderJsTreeDirectoryItem = _directoryTreeCreator.TraverseTree(foundProcessingSourceLocation.Location);
			return Request.CreateResponse(HttpStatusCode.OK, rootFolderJsTreeDirectoryItem);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve processing source location subfolders info.")]
		public HttpResponseMessage GetSubItems(int workspaceId, bool isRoot, [FromBody] string path)
		{
			List<JsTreeItemDTO> subItems = _directoryTreeCreator.GetChildren(path, isRoot);
			return Request.CreateResponse(HttpStatusCode.OK, subItems);
		}

		#endregion //Methods
	}
}