using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer.ADF
{
	internal class ADLSMigrationStatus : IADLSMigrationStatus
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly IHelperWrapper _helperWrapper;
		private readonly IAPILog _logger;

        public ADLSMigrationStatus(
            ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
            IHelperWrapper helperWrapper,
            IAPILog logger)
        {
            _serviceFactoryForAdmin = serviceFactoryForAdmin;
            _helperWrapper = helperWrapper;
            _logger = logger;
        }

        public async Task<bool> IsTenantFullyMigratedAsync()
        {
            _logger.LogInformation("Checking if tenant is fully migrated to ADLS");
            try
            {
                List<string> filesharesFromResourceServer =
                    await GetListOfFileSharesFromResourceServerAsync().ConfigureAwait(false);
                List<string> filesharesFromBedrock = await GetListOfFilesharesFromBedrockAsync().ConfigureAwait(false);

                var nonMigratedFileshares = filesharesFromResourceServer.Except(filesharesFromBedrock).ToList();
                bool isTenantFullyMigrated = nonMigratedFileshares.Count == 0;

                if (!isTenantFullyMigrated)
                {
                    foreach (var nonMigratedFileshare in nonMigratedFileshares)
                    {
                        _logger.LogInformation("Non migrated fileshare name: {fileshareName}", nonMigratedFileshare);
                    }
                }

                _logger.LogInformation("IsTenantFullMigrated check: {isTenantFullyMigrated}", isTenantFullyMigrated);
                return isTenantFullyMigrated;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception when checking if tenant is fully migrated to ADLS.");
                return false;
            }
        }

        private async Task<List<string>> GetListOfFileSharesFromResourceServerAsync()
        {
            using (var resourceServer = await _serviceFactoryForAdmin.CreateProxyAsync<IFileShareServerManager>()
                       .ConfigureAwait(false))
            {
                var resultSet = await resourceServer.QueryAsync(new Services.Query()).ConfigureAwait(false);
                _logger.LogInformation("Retrieved {fileServersResourceServerCount} file server(s)", resultSet.TotalCount);
                List<string> serverList = new List<string>();
                foreach (Result<FileShareResourceServer> result in resultSet.Results)
                {
                    _logger.LogInformation("Name: {FileServerName} UNC: {fileServerUNC}", result.Artifact.Name, result.Artifact.UNCPath);
                    Uri resultUri = new Uri(result.Artifact.UNCPath);
                    if (!serverList.Contains(resultUri.Host))
                    {
                        serverList.Add(resultUri.Host);
                    }
                }

                return serverList;
            }
        }

        private async Task<List<string>> GetListOfFilesharesFromBedrockAsync()
        {
            var bedrockEndpoints =
                await _helperWrapper.GetStorageEndpointsAsync().ConfigureAwait(false);

            _logger.LogInformation("Retrieved {fileServersBedrockCount} bedrock server(s)", bedrockEndpoints.Length);

            foreach (var endpoint in bedrockEndpoints)
            {
                _logger.LogInformation("EndpointFqdn: {EndpointFqdn} StorageInterface: {StorageInterface}",
                    endpoint.EndpointFqdn, endpoint.StorageInterface);
            }

            return bedrockEndpoints.Select(x => x.EndpointFqdn).ToList();
        }
    }
}
