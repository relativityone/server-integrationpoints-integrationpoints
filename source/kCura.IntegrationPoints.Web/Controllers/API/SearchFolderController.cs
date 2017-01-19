using System;
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
		private readonly IServiceFactory _serviceFactory;
		private readonly IHelperFactory _helperFactory;
		private readonly ICPHelper _helper;

		public SearchFolderController(IServiceFactory serviceFactory, IHelperFactory helperFactory, ICPHelper helper)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_serviceFactory = serviceFactory;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage Get(int destinationWorkspaceId = 0, int? federatedInstanceId = null)
		{
			var targetHelper = federatedInstanceId.HasValue ? _helperFactory.CreateOAuthClientHelper(_helper, federatedInstanceId.Value) : _helper;
			var artifactService = _serviceFactory.CreateArtifactService(_helper, targetHelper);
			//TODO see if possible moving to DI container
			var artifactTreeService = new ArtifactTreeService(artifactService, new ArtifactTreeCreator(_helper));
			var folderTree = artifactTreeService.GetArtifactTreeWithWorkspaceSet(ArtifactTypeNames.Folder, destinationWorkspaceId);
			return Request.CreateResponse(HttpStatusCode.OK, folderTree);
		}
	}
}