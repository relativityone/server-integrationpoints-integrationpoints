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
		public HttpResponseMessage GetProcessingSourceLocationStructure(int workspaceId, int artifactId, int includeFiles = 0)
		{
			try
			{
				List<ProcessingSourceLocationDTO> processingSourceLocations =
					_resourcePoolManager.GetProcessingSourceLocation(workspaceId);

				ProcessingSourceLocationDTO foundProcessingSourceLocation = processingSourceLocations.FirstOrDefault(
					processingSourceLocation => processingSourceLocation.ArtifactId == artifactId);

				if (foundProcessingSourceLocation == null)
				{
					return Request.CreateResponse(HttpStatusCode.NotFound, $"Cannot find processing source location {artifactId}");
				}

				JsTreeItemDTO rootFolderJsTreeDirectoryItem = _directoryTreeCreator.TraverseTree(foundProcessingSourceLocation.Location, includeFiles!=0);
				return Request.CreateResponse(HttpStatusCode.OK, rootFolderJsTreeDirectoryItem);
			}
			catch (Exception ex)
			{
				string errMsg =
					$"Unable to retrieve processing source location folder structure {artifactId} (Folder is not accessible). Please contact system administrator.";
				this.HandleError(workspaceId, _errorRepository, ex, errMsg);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, errMsg);
			}
		}

		[HttpPost]
		public HttpResponseMessage GetSubItems(int workspaceId, bool isRoot, [FromBody] string path)
		{
			try
			{
				List<JsTreeItemDTO> subItems = _directoryTreeCreator.GetChildren(path, isRoot);
				return Request.CreateResponse(HttpStatusCode.OK, subItems);
			}
			catch (Exception ex)
			{
				string errMsg =
					string.Format($"Unable to retrieve folder structure for {0} path {path} {1}. Please contact system administrator.",
						isRoot ? "Processing Source Location" : "",
						isRoot ? "(Folder is not accessible)" : "");
				this.HandleError(workspaceId, _errorRepository, ex, errMsg);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, errMsg);
			}
		}

		#endregion //Methods
	}
}