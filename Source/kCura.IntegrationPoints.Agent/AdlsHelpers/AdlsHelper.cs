using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.Storage;
using Relativity.API;
using Relativity.Environment.V1.Workspace;
using Relativity.Environment.V1.Workspace.Models;
using Relativity.Storage;

namespace kCura.IntegrationPoints.Agent.AdlsHelpers
{
    public class AdlsHelper : IAdlsHelper
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly IRelativityStorageService _storageService;
        private readonly IAPILog _logger;

        public AdlsHelper(IKeplerServiceFactory serviceFactory, IRelativityStorageService storageService, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<bool> IsWorkspaceMigratedToAdlsAsync(int workspaceId)
        {
            try
            {
                using (IWorkspaceManager workspaceManager = await _serviceFactory.CreateProxyAsync<IWorkspaceManager>().ConfigureAwait(false))
                {
                    WorkspaceResponse response = await workspaceManager.ReadAsync(workspaceId).ConfigureAwait(false);
                    string fileShareName = response.DefaultFileRepository.Value.Name.ToLowerInvariant();
                    Uri fileShareUri = new Uri(fileShareName);

                    _logger.LogInformation("Workspace ID: {workspaceId} default file share host name: {fileShare}", workspaceId, fileShareUri.Host);

                    List<string> adlsFileShares = await GetAdlsFilesharesAsync().ConfigureAwait(false);

                    bool isOnAdls = adlsFileShares.Any(x => x == fileShareUri.Host);

                    return isOnAdls;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when checking if workspace ID: {workspaceId} is migrated to ADLS", workspaceId);
                throw;
            }
        }

        private async Task<List<string>> GetAdlsFilesharesAsync()
        {
            List<StorageEndpoint> storageEndpoints = (await _storageService.GetStorageEndpointsAsync().ConfigureAwait(false)).ToList();

            _logger.LogInformation("All available ADLS storage endpoints: {@adlsEndpoints}", storageEndpoints);

            List<string> fqdns = storageEndpoints
                .Where(x => x.StorageInterface == StorageInterface.Adls2 && !string.IsNullOrWhiteSpace(x.EndpointFqdn))
                .Select(x => x.EndpointFqdn.ToLowerInvariant())
                .ToList();

            return fqdns;
        }
    }
}