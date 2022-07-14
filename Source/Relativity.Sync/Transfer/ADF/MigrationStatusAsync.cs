using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.ResourceServer;
using Relativity.Storage;
using Relativity.Sync.KeplerFactory;


namespace Relativity.Sync.Transfer.ADF
{
	internal class MigrationStatusAsync : IMigrationStatus
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
		private readonly IInstanceSettings _instanceSettings;
		private readonly IAPILog _logger;
		private readonly IStorageAccessFactory _storageAccessFactory;
		private const string ADLER_SIEBEN_TEAM_ID = "PTCI-2456712";
		private const string RELATIVITY_SYNC_SERVICE_NAME = "relativity-sync";

		public MigrationStatusAsync(ISourceServiceFactoryForAdmin serviceFactoryForAdmin, IInstanceSettings instanceSettings, IStorageAccessFactory storageAccessFactory, IAPILog logger)
		{
			_serviceFactoryForAdmin = serviceFactoryForAdmin;
			_instanceSettings = instanceSettings;
			_storageAccessFactory = storageAccessFactory;
			_logger = logger;
		}
		public async Task<bool> IsTenantFullyMigratedAsync()
		{
			_logger.LogInformation("Checking if tenant is fully migrated to ADLS");
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

		private async Task<List<string>> GetListOfFileSharesFromResourceServerAsync()
		{
			using (var resourceServer = await _serviceFactoryForAdmin.CreateProxyAsync<IFileShareServerManager>().ConfigureAwait(false))
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
			using IStorageDiscovery storageDiscovery = await _storageAccessFactory.CreateStorageDiscoveryAsync(
				teamId: ADLER_SIEBEN_TEAM_ID,
				serviceName: RELATIVITY_SYNC_SERVICE_NAME
			);
			Guid tenantId = await _instanceSettings.GetInstanceIdGuidAsync().ConfigureAwait(false);
			_logger.LogInformation("StorageDiscovery TenantId: {tenantId}", tenantId );
			
			R1Environment r1Environment = R1Environment.CommercialRegression;
			
			var bedrockEndpoints = await storageDiscovery.GetStorageEndpointsAsync(r1Environment, tenantId: tenantId).ConfigureAwait(false);
			_logger.LogInformation("Retrieved {fileServersBedrockCount} file server(s)", bedrockEndpoints.Length);
			
			foreach (var endpoint in bedrockEndpoints)
			{
				_logger.LogInformation("EndpointFqdn: {EndpointFqdn} StorageInterface: {StorageInterface}", endpoint.EndpointFqdn, endpoint.StorageInterface );
			}
			return bedrockEndpoints.Select(x => x.EndpointFqdn).ToList();
		}
	}
}