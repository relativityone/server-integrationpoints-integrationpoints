using System.IO;
using System.Threading.Tasks;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer
{
    internal class FileShareService : IFileShareService
    {
        private readonly IDestinationServiceFactoryForUser _serviceFactory;

        public FileShareService(IDestinationServiceFactoryForUser serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public async Task<string> GetWorkspaceFileShareLocationAsync(int workspaceId)
        {
            using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
            {
                WorkspaceRef workspace = new WorkspaceRef() { ArtifactID = workspaceId };
                FileShareResourceServer server = await workspaceManager
                    .GetDefaultWorkspaceFileShareResourceServerAsync(workspace)
                    .ConfigureAwait(false);

                return Path.Combine(server.UNCPath, $"EDDS{workspaceId}");
            }
        }
    }
}
