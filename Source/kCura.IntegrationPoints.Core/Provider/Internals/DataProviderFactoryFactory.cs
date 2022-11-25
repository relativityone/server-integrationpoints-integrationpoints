using System;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using LanguageExt;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public class DataProviderFactoryFactory : IDataProviderFactoryFactory
    {
        private const int _ADMIN_CASE_ID = -1;

        private readonly IAPILog _logger;
        private readonly IHelper _helper;
        private readonly IToggleProvider _toggleProvider;
        private readonly IKubernetesMode _kubernetesMode;

        public DataProviderFactoryFactory(IAPILog logger, IHelper helper, IToggleProvider toggleProvider, IKubernetesMode kubernetesMode)
        {
            _logger = logger;
            _helper = helper;
            _toggleProvider = toggleProvider;
            _kubernetesMode = kubernetesMode;
        }

        public Either<string, ProviderFactoryVendor> CreateProviderFactoryVendor()
        {
            IProviderFactoryLifecycleStrategy strategy = CreateProviderFactoryLifecycleStrategy();
            if (strategy == null)
            {
                return $"Cannot install source providers when {nameof(IProviderFactoryLifecycleStrategy)} is null";
            }

            return new ProviderFactoryVendor(strategy);
        }

        public IDataProviderFactory CreateDataProviderFactory(ProviderFactoryVendor providerFactoryVendor)
        {
            return new DataProviderBuilder(providerFactoryVendor);
        }

        private IProviderFactoryLifecycleStrategy CreateProviderFactoryLifecycleStrategy()
        {
            try
            {
                IEddsDBContext dbContext = new DbContextFactory(_helper, _logger).CreatedEDDSDbContext();
                var getAppBinaries = new GetApplicationBinaries(dbContext);
                IPluginProvider pluginProvider = new DefaultSourcePluginProvider(getAppBinaries);
                var relativityFeaturePathService = new RelativityFeaturePathService();
                var domainHelper = new AppDomainHelper(pluginProvider, _helper, relativityFeaturePathService, _toggleProvider, _kubernetesMode);
                var strategy = new AppDomainIsolatedFactoryLifecycleStrategy(domainHelper, _kubernetesMode, _helper);
                return strategy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while creating instance of {type}", nameof(IProviderFactoryLifecycleStrategy));
                return null;
            }
        }
    }
}
