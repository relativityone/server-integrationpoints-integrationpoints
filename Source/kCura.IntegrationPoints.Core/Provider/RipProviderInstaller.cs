using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using LanguageExt;
using Newtonsoft.Json;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContractsSourceProvider = kCura.IntegrationPoints.Contracts.SourceProvider;

namespace kCura.IntegrationPoints.Core.Provider
{
    public class RipProviderInstaller : IRipProviderInstaller
    {
        private const int _ADMIN_CASE_ID = -1;

        private readonly IAPILog _logger;
        private readonly ISourceProviderRepository _sourceProviderRepository;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IApplicationGuidFinder _applicationGuidFinder;
        private readonly IHelper _helper;

        public RipProviderInstaller(
            IAPILog logger,
            ISourceProviderRepository sourceProviderRepository,
            IRelativityObjectManager objectManager,
            IApplicationGuidFinder applicationGuidFinder,
            IHelper helper)
        {
            _logger = logger;
            _objectManager = objectManager;
            _applicationGuidFinder = applicationGuidFinder;
            _helper = helper;

            _sourceProviderRepository = sourceProviderRepository;
        }

        public async Task<Either<string, Unit>> InstallProvidersAsync(IEnumerable<ContractsSourceProvider> providersToInstall)
        {
            try
            {
                return await InstallProvidersInternalAsync(providersToInstall).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string errorMessage = "Unhandled error occured while installing providers";
                _logger.LogError(ex, errorMessage);
                return $"{errorMessage}. Exception: {ex}";
            }
        }

        private async Task<Either<string, Unit>> InstallProvidersInternalAsync(
            IEnumerable<ContractsSourceProvider> providersToInstall)
        {
            IProviderFactoryLifecycleStrategy strategy = CreateProviderFactoryLifecycleStrategy();
            if (strategy == null)
            {
                return $"Cannot install source providers when {nameof(IProviderFactoryLifecycleStrategy)} is null";
            }

            using (var vendor = new ProviderFactoryVendor(strategy))
            {
                IDataProviderFactory dataProviderBuilder = new DataProviderBuilder(vendor);
                return await InstallProvidersOneByOneAsync(dataProviderBuilder, providersToInstall).ConfigureAwait(false);
            }
        }

        private async Task<Either<string, Unit>> InstallProvidersOneByOneAsync(
            IDataProviderFactory dataProviderFactory,
            IEnumerable<ContractsSourceProvider> providersToInstall)
        {
            foreach (ContractsSourceProvider provider in providersToInstall)
            {
                Either<string, Unit> installProviderResult = await InstallProviderAsync(dataProviderFactory, provider).ConfigureAwait(false);
                if (installProviderResult.IsLeft)
                {
                    return installProviderResult;
                }
            }
            return Unit.Default;
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

        private Task<Either<string, Unit>> InstallProviderAsync(
            IDataProviderFactory dataProviderFactory,
            ContractsSourceProvider provider)
        {
            Either<string, ContractsSourceProvider> sourceProviderWithApplicationGuid = UpdateApplicationGuidIfMissing(provider);

            InstallSynchronizerForCoreOnly(provider.ApplicationGUID);

            return sourceProviderWithApplicationGuid
                .Bind(providerWithAppGuid => ValidateProvider(dataProviderFactory, providerWithAppGuid))
                .BindAsync(AddOrUpdateProvider);
        }

        // TODO KK - maybe we should create separate event handler for this step???
        private void InstallSynchronizerForCoreOnly(Guid applicationGuid)
        {
            //This is hack until we introduce installation of Destination Providers
            if (applicationGuid == new Guid(Domain.Constants.IntegrationPoints.APPLICATION_GUID_STRING))
            {
                try
                {
                    new Services.Synchronizer.RdoSynchronizerProvider(_objectManager, _helper)
                        .CreateOrUpdateDestinationProviders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occured while installing destination providers");
                    // TODO KK - what we should do in case of exception?
                }
            }
        }

        private Either<string, ContractsSourceProvider> ValidateProvider(
            IDataProviderFactory dataProviderFactory,
            ContractsSourceProvider provider)
        {
            return TryLoadingProvider(dataProviderFactory, provider);
        }

        private Either<string, ContractsSourceProvider> TryLoadingProvider(
            IDataProviderFactory dataProviderFactory,
            ContractsSourceProvider provider)
        {
            try
            {
                dataProviderFactory.GetDataProvider(provider.ApplicationGUID, provider.GUID);
                return provider;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading {provider}", provider?.Name);
                return $"Error while loading '{provider?.Name}' provider: {ex.Message}";
            }
        }

        private async Task<Either<string, Unit>> AddOrUpdateProvider(ContractsSourceProvider provider)
        {
            IDictionary<Guid, SourceProvider> installedRdoProviderDict = await GetInstalledRdoProviders(provider).ConfigureAwait(false);

            if (installedRdoProviderDict.ContainsKey(provider.GUID))
            {
                SourceProvider providerToUpdate = installedRdoProviderDict[provider.GUID];
                return UpdateExistingProvider(providerToUpdate, provider);
            }
            return AddProvider(provider);
        }

        private async Task<IDictionary<Guid, SourceProvider>> GetInstalledRdoProviders(ContractsSourceProvider provider)
        {
            List<SourceProvider> installedRdoProviders = await _sourceProviderRepository
                .GetSourceProviderRdoByApplicationIdentifierAsync(provider.ApplicationGUID)
                .ConfigureAwait(false);

            Dictionary<Guid, SourceProvider> installedRdoProviderDict =
                installedRdoProviders.ToDictionary(x => Guid.Parse(x.Identifier), x => x);
            return installedRdoProviderDict;
        }

        private Either<string, Unit> UpdateExistingProvider(SourceProvider existingProviderDto, ContractsSourceProvider provider)
        {
            existingProviderDto.Name = provider.Name;
            existingProviderDto.SourceConfigurationUrl = provider.Url;
            existingProviderDto.ViewConfigurationUrl = provider.ViewDataUrl;
            existingProviderDto.Config = provider.Configuration;
            existingProviderDto.Configuration = JsonConvert.SerializeObject(provider.Configuration);

            try
            {
                _objectManager.Update(existingProviderDto);
                _logger.LogInformation("Updated existing {object} - {artifactID}", nameof(SourceProvider), existingProviderDto.ArtifactId);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                return $"Error occured while updating {nameof(SourceProvider)}. Exception: {ex.Message}";
            }
        }

        private Either<string, Unit> AddProvider(ContractsSourceProvider newProvider)
        {
            if (newProvider == null)
            {
                return "Cannot add null provider";
            }

            var providerDto = new SourceProvider
            {
                Name = newProvider.Name,
                ApplicationIdentifier = newProvider.ApplicationGUID.ToString(),
                Identifier = newProvider.GUID.ToString(),
                SourceConfigurationUrl = newProvider.Url,
                ViewConfigurationUrl = newProvider.ViewDataUrl,
                Config = newProvider.Configuration
            };

            try
            {
                int sourceProviderArtifactId = _objectManager.Create(providerDto);
                _logger.LogInformation("Created new {object} - {artifactID}", nameof(SourceProvider), sourceProviderArtifactId);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                return $"Error occured while adding {nameof(SourceProvider)} to workspace. Exception: {ex.Message}";
            }
        }

        private Either<string, ContractsSourceProvider> UpdateApplicationGuidIfMissing(ContractsSourceProvider provider)
        {
            // when we migrate providers, we should already know which app does the provider belong to.
            if (provider.ApplicationGUID == Guid.Empty)
            {
                return _applicationGuidFinder
                    .GetApplicationGuid(provider.ApplicationID)
                    .Map(applicationGuid => UpdateApplicationGuid(provider, applicationGuid));
            }
            return provider;
        }

        private ContractsSourceProvider UpdateApplicationGuid(
            ContractsSourceProvider provider, Guid newApplicationGuid)
        {
            provider.ApplicationGUID = newApplicationGuid;
            return provider;
        }
    }
}
