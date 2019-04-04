using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.SourceProviderInstaller;
using LanguageExt;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
    public abstract class InternalSourceProviderInstaller : IntegrationPointSourceProviderInstaller
    {
        private readonly Lazy<IAPILog> _logggerLazy;

        private IAPILog Logger => _logggerLazy.Value;

        internal IProviderInstaller ProviderInstallerForTests { get; set; }

        protected InternalSourceProviderInstaller()
        {
            _logggerLazy = new Lazy<IAPILog>(
                () => Helper.GetLoggerFactory().GetLogger().ForContext<InternalSourceProviderInstaller>()
            );
        }

        internal override async Task InstallSourceProviders(IEnumerable<SourceProvider> sourceProviders)
        {
            Logger.LogInformation("Installing internal RIP source providers, providers: {@sourceProviders}", sourceProviders);

            await CreateProviderInstaller() // TODO make sure async will work fine with our flow
                .ToAsync()
                .Bind(providerInstaller => providerInstaller.InstallProvidersAsync(sourceProviders).ToAsync())
                .Match(
                    success => Logger.LogInformation("Internal source providers installed successfully."),
                    error => throw new InvalidSourceProviderException(error)
                 )
                .ConfigureAwait(false);
        }

        private Either<string, IProviderInstaller> CreateProviderInstaller()
        {
            if (ProviderInstallerForTests != null) // TODO It's hack for testsing 
            {
                return Right<string, IProviderInstaller>(ProviderInstallerForTests);
            }

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
                return $"Error occured while creating instance of {nameof(IProviderInstaller)}. Exception: {ex}";
            }
        }

        private IRelativityObjectManager CreateObjectManager()
        {
            var objectManagerFactory = new RelativityObjectManagerFactory(Helper);
            return objectManagerFactory.CreateRelativityObjectManager(Helper.GetActiveCaseID());
        }
    }
}
