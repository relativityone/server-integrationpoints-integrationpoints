using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ResourcePoolController : ApiController
	{
		#region Fields

		private readonly IResourcePoolManager _resourcePoolManager;
		private readonly IDirectoryTreeCreator<JsTreeItemDTO> _directoryTreeCreator;

		#endregion //Fields

		#region Constructors

		public ResourcePoolController(IResourcePoolManager resourcePoolManager, IDirectoryTreeCreator<JsTreeItemDTO> directoryTreeCreator)
		{
			_resourcePoolManager = resourcePoolManager;
			_directoryTreeCreator = directoryTreeCreator;
		}

		#endregion //Constructors

		#region Methods

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve processing source location list.")]
		public HttpResponseMessage GetProcessingSourceLocations(int workspaceId)
		{
			List<ProcessingSourceLocationDTO> processingSourceLocations = _resourcePoolManager.GetProcessingSourceLocation(workspaceId);
			return Request.CreateResponse(HttpStatusCode.OK, processingSourceLocations);
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