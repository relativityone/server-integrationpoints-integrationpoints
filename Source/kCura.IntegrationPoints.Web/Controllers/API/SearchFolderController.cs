using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Castle.Components.DictionaryAdapter;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Relativity.API;
using Relativity.Services.Folder;
using Folder = Relativity.Services.Folder.Folder;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
	public class SearchFolderController : ApiController
	{
		private readonly ICPHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IArtifactServiceFactory _artifactServiceFactory;
	    private readonly IFolderTreeBuilder _folderTreeCreator;

		public SearchFolderController(IArtifactServiceFactory artifactServiceFactory, IHelperFactory helperFactory, ICPHelper helper, IFolderTreeBuilder folderTreeCreator)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_artifactServiceFactory = artifactServiceFactory;
		    _folderTreeCreator = folderTreeCreator;
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage GetCurrentInstaceFolders(int destinationWorkspaceId)
		{
			var artifactService = _artifactServiceFactory.CreateArtifactService(_helper, _helper);
			return GetFolders(destinationWorkspaceId, artifactService);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve folders from destination workspace.")]
		public HttpResponseMessage GetFederatedInstaceFolders(int destinationWorkspaceId, int federatedInstanceId, [FromBody] object credentials)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials.ToString());
			var artifactService = _artifactServiceFactory.CreateArtifactService(_helper, targetHelper);
			return GetFolders(destinationWorkspaceId, artifactService);
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "")]
		public async Task<HttpResponseMessage> GetFullPathList([FromBody] object credentials, int destinationWorkspaceId, int folderArtifactId, int federatedInstanceId)
		{
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials?.ToString());
			using (var folderManager = targetHelper.GetServicesManager().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser))
			{
				var result = await folderManager.GetFullPathListAsync(destinationWorkspaceId, new List<int> {folderArtifactId});

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
		}

		[HttpPost]
		[LogApiExceptionFilter(Message = "Unable to retrieve data transfer location directory structure.")]
		public async Task<HttpResponseMessage> GetStructure([FromBody] object credentials, int destinationWorkspaceId, int federatedInstanceId, int folderArtifactId)
		{
		    Folder folder = null;
		    bool isRoot = false;

			var targetHelper = _helperFactory.CreateTargetHelper(_helper, federatedInstanceId, credentials?.ToString());

			using (IFolderManager folderManager = targetHelper.GetServicesManager()
				.CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser))
			{
				List<JsTreeItemDTO> tree = new EditableList<JsTreeItemDTO>();
				var currentNodeId = 0;
				if (folderArtifactId > 0)
				{
					currentNodeId = folderArtifactId;
					var children = await folderManager.GetChildrenAsync(destinationWorkspaceId, currentNodeId);

					tree = children.Select(x => _folderTreeCreator.CreateItemWithoutChildren(x)).ToList();
				}
				else
				{
					isRoot = true;

					Folder root = await folderManager
						.GetWorkspaceRootAsync(destinationWorkspaceId);

					currentNodeId = root.ArtifactID;
					folder = (await folderManager.GetFolderTreeAsync(destinationWorkspaceId, new List<int> {currentNodeId}))[0];

					JsTreeItemDTO currentNode = _folderTreeCreator.CreateItemWithChildren(folder, isRoot);
					tree.Add(currentNode);
				}

				return Request.CreateResponse(HttpStatusCode.OK, tree);
			}
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