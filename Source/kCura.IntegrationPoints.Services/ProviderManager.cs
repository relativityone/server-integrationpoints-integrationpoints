using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class ProviderManager : KeplerServiceBase, IProviderManager
	{
		private IInstaller _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		internal ProviderManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory) : base(logger, permissionRepositoryFactory)
		{
		}

		public ProviderManager(ILog logger) : base(logger)
		{
		}

		protected override IInstaller Installer => _installer ?? (_installer = new ProviderManagerInstaller());

		public void Dispose()
		{
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			return
				await
					Execute((IProviderRepository providerRepository) =>
							providerRepository.GetSourceProviderArtifactId(workspaceArtifactId, sourceProviderGuidIdentifier), workspaceArtifactId);
		}

		public async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			return
				await
					Execute((IProviderRepository providerRepository) =>
							providerRepository.GetDestinationProviderArtifactId(workspaceArtifactId, destinationProviderGuidIdentifier), workspaceArtifactId);
		}

		public async Task<IList<ProviderModel>> GetSourceProviders(int workspaceArtifactId)
		{
			return await Execute((IProviderRepository providerRepository) => providerRepository.GetSourceProviders(workspaceArtifactId), workspaceArtifactId);
		}

		public async Task<IList<ProviderModel>> GetDestinationProviders(int workspaceArtifactId)
		{
			return await Execute((IProviderRepository providerRepository) => providerRepository.GetDesinationProviders(workspaceArtifactId), workspaceArtifactId);
		}
	}
}