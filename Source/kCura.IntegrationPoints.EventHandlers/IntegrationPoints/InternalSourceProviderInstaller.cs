﻿using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
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

			IDBContext workspaceDbContext = Helper.GetDBContext(Helper.GetActiveCaseID());
			IRelativityObjectManager objectManager = CreateObjectManager();
			var sourceProviderRepository = new SourceProviderRepository(objectManager);

			var providerInstaller = new ProviderInstaller(Logger, sourceProviderRepository, objectManager, workspaceDbContext, Helper);

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
