using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public abstract class InternalSourceProviderInstaller : IntegrationPointSourceProviderInstaller
	{
		private readonly Lazy<IAPILog> _logggerLazy;

		private IAPILog Logger => _logggerLazy.Value;

		protected InternalSourceProviderInstaller()
		{
			_logggerLazy = new Lazy<IAPILog>(
				() => Helper.GetLoggerFactory().GetLogger().ForContext<InternalSourceProviderInstaller>()
			);
		}

		internal override async Task<bool> InstallSourceProviders(IEnumerable<SourceProvider> sourceProviders)
		{
			Logger.LogWarning("Installing internal RIP source providers, providers: {@sourceProviders}", sourceProviders); // TODO log info instead of warning

			IProviderInstaller providerInstaller = CreateProviderInstaller();

			try
			{
				return await providerInstaller.InstallProvidersAsync(sourceProviders).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error occured while installing internal RIP source providers.");
				return false;
			}
		}

		private IProviderInstaller CreateProviderInstaller()
		{
			try
			{
				IDBContext workspaceDbContext = Helper.GetDBContext(Helper.GetActiveCaseID());
				IRelativityObjectManager objectManager = CreateObjectManager();
				var sourceProviderRepository = new SourceProviderRepository(objectManager);

				return new ProviderInstaller(
					Logger,
					sourceProviderRepository,
					objectManager,
					workspaceDbContext,
					Helper
				);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error occured while creating instance of {type}", nameof(IProviderInstaller));
				return null;
			}
		}

		private IRelativityObjectManager CreateObjectManager()
		{
			var objectManagerFactory = new RelativityObjectManagerFactory(Helper);
			return objectManagerFactory.CreateRelativityObjectManager(Helper.GetActiveCaseID());
		}
	}
}
