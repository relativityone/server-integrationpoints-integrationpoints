using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client.DTOs;
using kCura.WinEDDS.Api;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SearchFolderController : ApiController
	{
		private readonly ICPHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IArtifactServiceFactory _artifactServiceFactory;
		private ISerializer _serializer;

		public SearchFolderController(IArtifactServiceFactory artifactServiceFactory, IHelperFactory helperFactory, ICPHelper helper,
			ISerializer serializer)
		{
			_helper = helper;
			_serializer = serializer;
			_helperFactory = helperFactory;
			_artifactServiceFactory = artifactServiceFactory;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage GetCurrentInstaceFolders(int destinationWorkspaceId)
		{
			//var artifactService = _artifactServiceFactory.CreateArtifactService(_helper, _helper);

			var cookieContainer = new CookieContainer();
			var serviceInfo = _helperFactory.GetNetworkCredential(_helper, null, null, cookieContainer);

			IFolderManagerService folderManagerService = new FolderManagerService(cookieContainer, serviceInfo.NetworkCredential, serviceInfo.WebServiceUrl);
			var artifacts = folderManagerService.GetFolders(destinationWorkspaceId);

			return GetFolders(artifacts);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage GetFederatedInstaceFolders(int destinationWorkspaceId, int federatedInstanceId, [FromBody] object credentials)
		{
			//var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials.ToString());

			var cookieContainer = new CookieContainer();
			var serviceInfo = _helperFactory.GetNetworkCredential(_helper, federatedInstanceId, credentials.ToString(), cookieContainer);

			IFolderManagerService folderManagerService = new FolderManagerService(cookieContainer, serviceInfo.NetworkCredential, serviceInfo.WebServiceUrl);


			//var artifactService = _artifactServiceFactory.CreateArtifactService(_helper, targetHelper);

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