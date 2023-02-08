using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public class FileShareService : IFileShareService
    {
        private readonly IKeplerServiceFactory _serviceFactory;

        public FileShareService(IKeplerServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<string> GetWorkspaceFileShareLocationAsync(int workspaceId)
        {
            using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
            {
                WorkspaceRef workspace = new WorkspaceRef()
                {
                    ArtifactID = workspaceId
                };

                FileShareResourceServer server = await workspaceManager
                    .GetDefaultWorkspaceFileShareResourceServerAsync(workspace)
                    .ConfigureAwait(false);

                return Path.Combine(server.UNCPath, $"EDDS{workspaceId}");
            }
        }
    }
}