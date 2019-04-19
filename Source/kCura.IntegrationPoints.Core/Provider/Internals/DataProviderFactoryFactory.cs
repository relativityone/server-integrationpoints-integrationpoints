﻿using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain;
using LanguageExt;
using Relativity.API;
using System;

namespace kCura.IntegrationPoints.Core.Provider.Internals
{
    public class DataProviderFactoryFactory : IDataProviderFactoryFactory
    {
        private const int _ADMIN_CASE_ID = -1;

        private readonly IAPILog _logger;
        private readonly IHelper _helper;

        public DataProviderFactoryFactory(IAPILog logger, IHelper helper)
        {
            _logger = logger;
            _helper = helper;
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
                IDBContext adminCaseDbContext = _helper.GetDBContext(_ADMIN_CASE_ID);
                var getAppBinaries = new GetApplicationBinaries(adminCaseDbContext);
                IPluginProvider pluginProvider = new DefaultSourcePluginProvider(getAppBinaries);
                var relativityFeaturePathService = new RelativityFeaturePathService();
                var domainHelper = new DomainHelper(pluginProvider, _helper, relativityFeaturePathService);
                var strategy = new AppDomainIsolatedFactoryLifecycleStrategy(domainHelper);
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