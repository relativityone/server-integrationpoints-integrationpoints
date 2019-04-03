using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;
using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	public abstract class InternalSourceProviderInstaller : IntegrationPointSourceProviderInstaller
	{
		private readonly Lazy<IAPILog> _logggerLazy;

		private IAPILog Logger => _logggerLazy.Value;

		protected InternalSourceProviderInstaller()
		{
			_logggerLazy = new Lazy<IAPILog>(
				() => Helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointSourceProviderInstaller>()
			);
		}

		internal override void InstallSourceProvider(IEnumerable<SourceProvider> sourceProviders)
		{
			Logger.LogError("Installing internal RIP source provider"); // TODO debug/info
			var providerInstaller = new ProviderInstaller(Logger, CreateObjectManager(), Helper.GetDBContext(Helper.GetActiveCaseID()), Helper);

			providerInstaller.InstallProvidersAsync(sourceProviders).GetAwaiter().GetResult();
		}

		#region ToRefactor
		private IRelativityObjectManager CreateObjectManager()
		{
			return CreateObjectManagerFactory().CreateRelativityObjectManager(Helper.GetActiveCaseID());
		}

		private IRelativityObjectManagerFactory CreateObjectManagerFactory()
		{
			return new RelativityObjectManagerFactory(Helper);
		}
		#endregion
	}
}
