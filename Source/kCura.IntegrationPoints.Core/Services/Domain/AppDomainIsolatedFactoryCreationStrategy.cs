using System;
using System.Collections.Generic;
using System.IO;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
    public class AppDomainIsolatedFactoryLifecycleStrategy : IProviderFactoryLifecycleStrategy
    {
        private readonly Dictionary<Guid, AppDomain> _appDomains;
        protected readonly IAppDomainHelper DomainHelper;
        private readonly IKubernetesMode _kubernetesMode;
        private readonly IAPILog _logger;

        public AppDomainIsolatedFactoryLifecycleStrategy(IAppDomainHelper domainHelper, IKubernetesMode kubernetesMode, IHelper helper)
        {
            DomainHelper = domainHelper;
            _kubernetesMode = kubernetesMode;

            _logger = helper.GetLoggerFactory().GetLogger().ForContext<AppDomainIsolatedFactoryLifecycleStrategy>();
            _appDomains = new Dictionary<Guid, AppDomain>();
        }

        public virtual IProviderFactory CreateProviderFactory(Guid applicationId)
        {
            if (_kubernetesMode.IsEnabled())
            {
                return GetKubernetesProviderFactory(applicationId);
            }

            return GetInTenantProviderFactory(applicationId);
        }

        private IProviderFactory GetInTenantProviderFactory(Guid applicationId)
        {
            AppDomain newDomain = CreateNewDomain(applicationId);

            _logger.LogInformation("Prepare ProviderFactory in in-tenant architecture for {domain}", newDomain.BaseDirectory);
            IAppDomainManager domainManager = DomainHelper.SetupDomainAndCreateManager(newDomain, applicationId);
            DomainHelper.BootstrapDomain(newDomain);

            return domainManager.CreateProviderFactory();
        }

        private IProviderFactory GetKubernetesProviderFactory(Guid applicationId)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            _logger.LogInformation("Prepare ProviderFactory in k8s mode for domain {domain}", currentDomain.BaseDirectory);

            string assemblyLocalPath = new Uri(typeof(AssemblyDomainLoader).Assembly.CodeBase).LocalPath;
            string assemblyLocalDirectory = Path.GetDirectoryName(assemblyLocalPath);
            _logger.LogInformation("Working Directory - {workingDirectory}", assemblyLocalDirectory);

            var domainManager = DomainHelper.SetupDomainAndCreateManager(currentDomain, applicationId);

            return domainManager.CreateProviderFactory();
        }

        private AppDomain CreateNewDomain(Guid applicationId)
        {
            AppDomain newDomain = DomainHelper.CreateNewDomain();
            _appDomains[applicationId] = newDomain;
            return newDomain;
        }

        public virtual void OnReleaseProviderFactory(Guid applicationId)
        {
            AppDomain domainToRelease;
            if (_appDomains.TryGetValue(applicationId, out domainToRelease))
            {
                DomainHelper.ReleaseDomain(domainToRelease);
                _appDomains.Remove(applicationId);
            }
        }
    }
}
