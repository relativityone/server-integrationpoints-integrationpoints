using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using LanguageExt;
using Relativity.API;
using Relativity.IntegrationPoints.SourceProviderInstaller;
using Relativity.IntegrationPoints.SourceProviderInstaller.Internals;
using Relativity.Toggles;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    internal class InProcessSourceProviderInstaller : ISourceProviderInstaller
    {
        private readonly IAPILog _logger;
        private readonly IEHHelper _helper;
        private readonly IRipProviderInstaller _ripProviderInstaller;
        private readonly IKubernetesMode _kubernetesMode;
        private readonly IToggleProvider _toggleProvider;

        public InProcessSourceProviderInstaller(
            IAPILog logger,
            IEHHelper helper,
            IKubernetesMode kubernetesMode,
            IToggleProvider toggleProvider,
            IRipProviderInstaller ripProviderInstaller 
        )
        {
            _logger = logger.ForContext<InProcessSourceProviderInstaller>();
            _helper = helper;
            _ripProviderInstaller = ripProviderInstaller;
            _kubernetesMode = kubernetesMode ?? new KubernetesMode(logger);
            _toggleProvider = toggleProvider;
        }

        public Task InstallSourceProvidersAsync(int workspaceID, IEnumerable<global::Relativity.IntegrationPoints.Contracts.SourceProvider> sourceProviders)
        {
            _logger.LogInformation("Installing internal RIP source providers, providers: {@sourceProviders}", sourceProviders);

            // we are not using EitherAsync, because language-ext in version 3.1.15 does not call ConfigureAwait(false)
            GetProviderInstaller(workspaceID)
                .Bind(providerInstaller => InstallProviders(providerInstaller, sourceProviders))
                .Match(
                    success => _logger.LogInformation("Internal source providers installed successfully."),
                    error => throw new InvalidSourceProviderException(error),
                    Bottom: () => _logger.LogFatal("Unexpected state of Either")
                );

            return Task.CompletedTask;
        }

        private static Either<string, Unit> InstallProviders(
            IRipProviderInstaller providerInstaller,
            IEnumerable<global::Relativity.IntegrationPoints.Contracts.SourceProvider> sourceProviders)
        {
            return providerInstaller.InstallProvidersAsync(sourceProviders).GetAwaiter().GetResult();
        }

        private Either<string, IRipProviderInstaller> GetProviderInstaller(int workspaceID)
        {
            if (_ripProviderInstaller != null)
            {
                return Right<string, IRipProviderInstaller>(_ripProviderInstaller);
            }

            try
            {
                IRipProviderInstaller providerInstaller = CreateProviderInstaller(workspaceID);
                return Right<string, IRipProviderInstaller>(providerInstaller);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while creating instance of {type}", nameof(IRipProviderInstaller));
                return $"Error occured while creating instance of {nameof(IRipProviderInstaller)}. Exception: {ex}";
            }
        }

        private IRipProviderInstaller CreateProviderInstaller(int workspaceID)
        {
            IApplicationGuidFinder applicationGuidFinder = CreateApplicationGuidFinder(workspaceID);
            IDataProviderFactoryFactory dataProviderFactoryFactory = CreateDataProviderFactoryFactory();
            IRelativityObjectManager objectManager = CreateObjectManager(workspaceID);
            var sourceProviderRepository = new SourceProviderRepository(objectManager);

            return new RipProviderInstaller(
                _logger,
                sourceProviderRepository,
                applicationGuidFinder,
                dataProviderFactoryFactory,
                _toggleProvider
            );
        }

        private IRelativityObjectManager CreateObjectManager(int workspaceID)
        {
            var objectManagerFactory = new RelativityObjectManagerFactory(_helper);
            return objectManagerFactory.CreateRelativityObjectManager(workspaceID);
        }

        private IApplicationGuidFinder CreateApplicationGuidFinder(int workspaceID)
        {
            IDBContext workspaceDbContext = _helper.GetDBContext(workspaceID);
            var workspaceDbContextAsWorkspaceContext = new WorkspaceDBContext(workspaceDbContext);
            return new ApplicationGuidFinder(workspaceDbContextAsWorkspaceContext);
        }

        private IDataProviderFactoryFactory CreateDataProviderFactoryFactory()
        {
            return new DataProviderFactoryFactory(_logger, _helper, _toggleProvider, _kubernetesMode);
        }
    }
}
