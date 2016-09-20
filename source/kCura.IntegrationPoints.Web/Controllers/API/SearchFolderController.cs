using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SearchFolderController : ApiController
	{
		private readonly IArtifactTreeService _artifactTreeService;

		public SearchFolderController(IArtifactTreeService artifactTreeService)
		{
			_artifactTreeService = artifactTreeService;
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			try
			{
				var folderTree = _artifactTreeService.GetArtifactTree(ArtifactTypeNames.Folder);
				return Request.CreateResponse(HttpStatusCode.OK, folderTree);
			}
			catch (Exception)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to retrieve search folders structure");
			}
		}
	}
}