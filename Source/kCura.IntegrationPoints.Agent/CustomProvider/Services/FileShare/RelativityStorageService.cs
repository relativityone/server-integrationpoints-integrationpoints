using System.IO;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using Relativity.API;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using Relativity.Storage;
using Relativity.Storage.Extensions;
using Relativity.Storage.Extensions.Models;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.FileShare
{
    public class RelativityStorageService : IRelativityStorageService
    {
        private const string _TEAM_ID = "PTCI-2456712";

        private readonly IHelper _helper;
        private readonly IKeplerServiceFactory _serviceFactory;

        private IStorageAccess<string> _storageAccess;

        public RelativityStorageService(IHelper helper, IKeplerServiceFactory serviceFactory)
        {
            _helper = helper;
            _serviceFactory = serviceFactory;
        }

        public async Task<IStorageAccess<string>> GetStorageAccessAsync()
        {
            if (_storageAccess == null)
            {
                _storageAccess = await _helper.GetStorageAccessorAsync(StorageAccessPermissions.GenericReadWrite, new ApplicationDetails(_TEAM_ID)).ConfigureAwait(false);
            }

            return _storageAccess;
        }

        public async Task<StorageStream> CreateFileOrTruncateExistingAsync(string path)
        {
            IStorageAccess<string> storageAccess = await GetStorageAccessAsync().ConfigureAwait(false);
            StorageStream stream = await storageAccess.CreateFileOrTruncateExistingAsync(path).ConfigureAwait(false);
            return stream;
        }

        public async Task<string> GetWorkspaceDirectoryPathAsync(int workspaceId)
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
