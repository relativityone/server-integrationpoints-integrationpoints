using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;
using Relativity.Storage;
using Relativity.Sync.KeplerFactory;


namespace Relativity.Sync.Transfer.ADF
{
	internal class MigrationStatus : IMigrationStatus
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly IInstanceSettings _instanceSettings;
		private readonly IAPILog _logger;
		private const string ADLER_SIEBEN_TEAM_ID = "PTCI-2456712";
		private const string RELATIVITY_SYNC_SERVICE_NAME = "relativity-sync";

		public MigrationStatus(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IInstanceSettings instanceSettings, IAPILog logger)
		{
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_instanceSettings = instanceSettings;
			_logger = logger;
		}
		public async Task<bool> IsTenantFullyMigratedAsync()
		{
			List<string> filesharesFromResourceServer =
				await GetListOfFileSharesFromResourceServerAsync().ConfigureAwait(false);
			List<string> filesharesFromBedrock = await GetListOfFilesharesFromBedrockAsync().ConfigureAwait(false);
			
			return filesharesFromResourceServer.Except(filesharesFromBedrock).Count() == 0;
		}

		private async Task<List<string>> GetListOfFileSharesFromResourceServerAsync()
		{
			using (var resourceServer = await _serviceFactoryForAdmin.CreateProxyAsync<IFileShareServerManager>().ConfigureAwait(false))
			{
				var resultSet = await resourceServer.QueryAsync(new Services.Query()).ConfigureAwait(false);
				_logger.LogInformation("Retrieved {fileServersResourceServerCount} file server(s)", resultSet.TotalCount);
				
				foreach (Result<FileShareResourceServer> result in resultSet.Results)
				{
					_logger.LogInformation("Name: {FileServerName} UNC: {fileServerUNC}", result.Artifact.Name, result.Artifact.UNCPath);
				}

				return resultSet.Results.Select(r => r.Artifact.UNCPath).ToList();
			}
		}

		private async Task<List<string>> GetListOfFilesharesFromBedrockAsync()
		{
			using var storageDiscovery = await StorageAccessFactory.CreateStorageDiscoveryAsync(
				teamId: ADLER_SIEBEN_TEAM_ID,
				serviceName: RELATIVITY_SYNC_SERVICE_NAME
			);
			Guid tenantId = await _instanceSettings.GetInstanceIdGuidAsync().ConfigureAwait(false);
			_logger.LogInformation("StorageDiscovery TenantId: {tenantId}", tenantId );
			var bedrockEndpoints = await storageDiscovery.GetStorageEndpointsAsync(R1Environment.CommercialRegression, tenantId: tenantId).ConfigureAwait(false);
			_logger.LogInformation("Retrieved {fileServersBedrockCount} file server(s)", bedrockEndpoints.Length);
			
			foreach (var endpoint in bedrockEndpoints)
			{
				_logger.LogInformation("EndpointFqdn: {EndpointFqdn} StorageInterface: {StorageInterface}", endpoint.EndpointFqdn, endpoint.StorageInterface );
			}
			return bedrockEndpoints.Select(x => x.EndpointFqdn).ToList();
		}
	}
}