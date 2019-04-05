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
        private readonly IProviderInstaller _providerInstaller;
        private readonly Lazy<IAPILog> _logggerLazy;

        private IAPILog Logger => _logggerLazy.Value;

        protected InternalSourceProviderInstaller()
        {
            _logggerLazy = new Lazy<IAPILog>(
                () => Helper.GetLoggerFactory().GetLogger().ForContext<InternalSourceProviderInstaller>()
            );
        }

        protected InternalSourceProviderInstaller(IProviderInstaller providerInstaller) : this()
        {
            _providerInstaller = providerInstaller;
        }

        internal override Task InstallSourceProviders(IEnumerable<SourceProvider> sourceProviders)
        {
            Logger.LogInformation("Installing internal RIP source providers, providers: {@sourceProviders}", sourceProviders);

            // we are not using EitherAsync, because language-ext in version 3.1.15 does not call ConfigureAwait(false)
            GetProviderInstaller()
                .Bind(providerInstaller => InstallProviders(providerInstaller, sourceProviders))
                .Match(
                    success => Logger.LogInformation("Internal source providers installed successfully."),
                    error => throw new InvalidSourceProviderException(error),
                    Bottom: () => Logger.LogFatal("Unexpected state of Either")
                 );

            return Task.CompletedTask;
        }

        private static Either<string, Unit> InstallProviders(
            IProviderInstaller providerInstaller,
            IEnumerable<SourceProvider> sourceProviders)
        {
            return providerInstaller.InstallProvidersAsync(sourceProviders).GetAwaiter().GetResult();
        }

        private Either<string, IProviderInstaller> GetProviderInstaller()
        {
            if (_providerInstaller != null)
            {
                return Right<string, IProviderInstaller>(_providerInstaller);
            }

            try
            {
                IProviderInstaller providerInstaller = CreateProviderInstaller();
                return Right<string, IProviderInstaller>(providerInstaller);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occured while creating instance of {type}", nameof(IProviderInstaller));
                return $"Error occured while creating instance of {nameof(IProviderInstaller)}. Exception: {ex}";
            }
        }

        private IProviderInstaller CreateProviderInstaller()
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

        private IRelativityObjectManager CreateObjectManager()
        {
            var objectManagerFactory = new RelativityObjectManagerFactory(Helper);
            return objectManagerFactory.CreateRelativityObjectManager(Helper.GetActiveCaseID());
        }
    }
}
