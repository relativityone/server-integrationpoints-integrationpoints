using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
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
		[LogApiExceptionFilter(Message = "Unable to retrieve processing source location list.")]
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

		[HttpGet]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage Get(int destinationWorkspaceId)
		{
			try
			{
				var folderTree = _artifactTreeService.GetArtifactTreeWithWorkspaceSet(ArtifactTypeNames.Folder, destinationWorkspaceId);
				return Request.CreateResponse(HttpStatusCode.OK, folderTree);
			}
			catch (Exception)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, "Failed to retrieve search folders structure");
			}
		}
	}
}