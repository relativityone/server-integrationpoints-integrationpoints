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

        internal override async Task InstallSourceProviders(IEnumerable<SourceProvider> sourceProviders)
        {
            Logger.LogInformation("Installing internal RIP source providers, providers: {@sourceProviders}", sourceProviders);

            Either<string, Unit> result = await CreateProviderInstaller()
                .BindAsync(providerInstaller => providerInstaller.InstallProvidersAsync(sourceProviders))
                .ConfigureAwait(false);

            result.Match(
                success => Logger.LogInformation("Internal source providers installed successfully."),
                error => throw new InvalidSourceProviderException(error)
            );
        }

        private Either<string, IProviderInstaller> CreateProviderInstaller()
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
