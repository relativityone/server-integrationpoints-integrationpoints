using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class ResourcePoolController : ApiController
	{
		#region Fields

		private readonly IResourcePoolManager _resourcePoolManager;
		private readonly IDirectoryTreeCreator _directoryTreeCreator;

		#endregion //Fields

		#region Constructors

		public ResourcePoolController(IResourcePoolManager resourcePoolManager, IDirectoryTreeCreator directoryTreeCreator)
		{
			_resourcePoolManager = resourcePoolManager;
			_directoryTreeCreator = directoryTreeCreator;
		}

		#endregion //Constructors

		#region Methods

		[HttpGet]
		public HttpResponseMessage GetProcessingSourceLocations(int workspaceId)
		{
			List<ProcessingSourceLocationDTO> processingSourceLocations =
				_resourcePoolManager.GetProcessingSourceLocation(workspaceId);
			
			return Request.CreateResponse(HttpStatusCode.OK, processingSourceLocations);
		}

		[HttpGet]
		public HttpResponseMessage GetProcessingSourceLocationStructure(int workspaceId, int artifactId)
		{
			List<ProcessingSourceLocationDTO> processingSourceLocations =
				_resourcePoolManager.GetProcessingSourceLocation(workspaceId);

			ProcessingSourceLocationDTO foundProcessingSourceLocation = processingSourceLocations.FirstOrDefault(
				processingSourceLocation => processingSourceLocation.ArtifactId == artifactId);

			if (foundProcessingSourceLocation == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound, "Cannot find processing source location artifact id");
			}

			DirectoryTreeItem rootFolderTreeDirectoryItem = _directoryTreeCreator.TraverseTree(foundProcessingSourceLocation.Location);
			return Request.CreateResponse(HttpStatusCode.OK, rootFolderTreeDirectoryItem);
		}

		#endregion //Methods
	}
}