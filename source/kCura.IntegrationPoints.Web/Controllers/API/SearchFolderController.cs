using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SearchFolderController : ApiController
	{
		private readonly IHelperFactory _helperFactory;
		private readonly ICPHelper _helper;

		public SearchFolderController(IHelperFactory helperFactory, ICPHelper helper)
		{
			_helperFactory = helperFactory;
			_helper = helper;
		}

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage Get(int destinationWorkspaceId = 0)
		{
			var cookieContainer = new CookieContainer();
			var serviceInfo = _helperFactory.GetNetworkCredential(cookieContainer);

			IFolderManagerService folderManagerService = new FolderManagerService(cookieContainer, serviceInfo.NetworkCredential, serviceInfo.WebServiceUrl);
			var artifacts = folderManagerService.GetFolders(destinationWorkspaceId);

			return GetFolders(artifacts);
		}

		private HttpResponseMessage GetFolders(IEnumerable<Relativity.Client.Artifact> artifacts)
		{
			//TODO see if possible moving to DI container

			var treeCreator = new ArtifactTreeCreator(_helper);
			//var artifactTreeService = new ArtifactTreeService(artifactService, new ArtifactTreeCreator(_helper));
			//var folderTree = artifactTreeService.GetArtifactTreeWithWorkspaceSet(ArtifactTypeNames.Folder, destinationWorkspaceId);
			return Request.CreateResponse(HttpStatusCode.OK, treeCreator.Create(artifacts));
		}
	}
}