using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SearchFolderController : ApiController
	{
		private readonly ICPHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IServiceFactory _serviceFactory;

		public SearchFolderController(IServiceFactory serviceFactory, IHelperFactory helperFactory, ICPHelper helper)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_serviceFactory = serviceFactory;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage GetCurrentInstaceFolders(int destinationWorkspaceId)
		{
			var artifactService = _serviceFactory.CreateArtifactService(_helper, _helper);
			return GetFolders(destinationWorkspaceId, artifactService);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage GetFederatedInstaceFolders(int destinationWorkspaceId, int federatedInstanceId, [FromBody] object credentials)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials.ToString());
			var artifactService = _serviceFactory.CreateArtifactService(_helper, targetHelper);
			return GetFolders(destinationWorkspaceId, artifactService);
		}

		private HttpResponseMessage GetFolders(int destinationWorkspaceId, IArtifactService artifactService)
		{
			//TODO see if possible moving to DI container
			var artifactTreeService = new ArtifactTreeService(artifactService, new ArtifactTreeCreator(_helper));
			var folderTree = artifactTreeService.GetArtifactTreeWithWorkspaceSet(ArtifactTypeNames.Folder, destinationWorkspaceId);
			return Request.CreateResponse(HttpStatusCode.OK, folderTree);
		}
	}
}