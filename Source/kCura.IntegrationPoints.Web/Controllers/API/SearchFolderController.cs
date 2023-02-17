using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Attributes;
using Relativity.API;
using Relativity.Services.Folder;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class SearchFolderController : ApiController
    {
        private readonly ICPHelper _helper;
        private readonly IFolderTreeBuilder _folderTreeCreator;

        public SearchFolderController(ICPHelper helper, IFolderTreeBuilder folderTreeCreator)
        {
            _helper = helper;
            _folderTreeCreator = folderTreeCreator;
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "")]
        public async Task<HttpResponseMessage> GetFullPathList([FromBody] object credentials, int destinationWorkspaceId, int folderArtifactId, int federatedInstanceId)
        {
            using (var folderManager = _helper.GetServicesManager().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser))
            {
                List<FolderPath> result = await folderManager.GetFullPathListAsync(destinationWorkspaceId, new List<int> { folderArtifactId }).ConfigureAwait(true);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }

        [HttpPost]
        [LogApiExceptionFilter(Message = "Unable to retrieve data transfer location directory structure.")]
        public async Task<HttpResponseMessage> GetStructure([FromBody] object credentials, int destinationWorkspaceId, int federatedInstanceId, int folderArtifactId)
        {
            using (IFolderManager folderManager = _helper.GetServicesManager().CreateProxy<IFolderManager>(ExecutionIdentity.CurrentUser))
            {
                var tree = new List<JsTreeItemDTO>();
                int currentNodeId = 0;
                if (folderArtifactId > 0)
                {
                    currentNodeId = folderArtifactId;
                    List<Folder> children = await folderManager.GetChildrenAsync(destinationWorkspaceId, currentNodeId).ConfigureAwait(true);

                    tree = children.Select(x => _folderTreeCreator.CreateItemWithoutChildren(x)).ToList();
                }
                else
                {
                    Folder root = await folderManager.GetWorkspaceRootAsync(destinationWorkspaceId).ConfigureAwait(true);

                    currentNodeId = root.ArtifactID;
                    Folder folder = (await folderManager.GetFolderTreeAsync(destinationWorkspaceId, new List<int> { currentNodeId }).ConfigureAwait(true))[0];

                    JsTreeItemDTO currentNode = _folderTreeCreator.CreateItemWithChildren(folder, isRoot: true);
                    tree.Add(currentNode);
                }

                return Request.CreateResponse(HttpStatusCode.OK, tree);
            }
        }
    }
}
