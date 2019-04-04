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

namespace kCura.IntegrationPoints.Core.Provider
{
    public class ProviderInstaller : IProviderInstaller
    {
        private const int _ADMIN_CASE_ID = -1;

        private readonly IAPILog _logger;
        private readonly ISourceProviderRepository _sourceProviderRepository;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IDBContext _dbContext;
        private readonly IHelper _helper;

        public ProviderInstaller(
            IAPILog logger,
            ISourceProviderRepository sourceProviderRepository,
            IRelativityObjectManager objectManager,
            IDBContext dbContext,
            IHelper helper)
        {
            _logger = logger;
            _objectManager = objectManager;
            _dbContext = dbContext;
            _helper = helper;

            _sourceProviderRepository = sourceProviderRepository;
        }

        public async Task<Either<string, Unit>> InstallProvidersAsync(IEnumerable<IntegrationPoints.Contracts.SourceProvider> providersToInstall)
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
            IEnumerable<IntegrationPoints.Contracts.SourceProvider> providersToInstall)
        {
            IProviderFactoryLifecycleStrategy strategy = CreateProviderFactoryLifecycleStrategy();
            if (strategy == null)
            {
                return $"Cannot install source providers when {nameof(IProviderFactoryLifecycleStrategy)} is null";
            }

            using (var vendor = new ProviderFactoryVendor(strategy))
            {
                IDataProviderFactory dataProviderBuilder = new DataProviderBuilder(vendor);
                return await InstallProvidersInternalAsync(dataProviderBuilder, providersToInstall);
            }
        }

        private async Task<Either<string, Unit>> InstallProvidersInternalAsync(
            IDataProviderFactory dataProviderFactory,
            IEnumerable<IntegrationPoints.Contracts.SourceProvider> providersToInstall)
        {
            foreach (IntegrationPoints.Contracts.SourceProvider provider in providersToInstall) // install one provider at a time
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
                IPluginProvider pluginProvider = new DefaultSourcePluginProvider(new GetApplicationBinaries(adminCaseDbContext));
                var domainHelper = new DomainHelper(pluginProvider, _helper, new RelativityFeaturePathService());
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
            IntegrationPoints.Contracts.SourceProvider provider)
        {
            Either<string, IntegrationPoints.Contracts.SourceProvider> sourceProviderWithApplicationGuid = UpdateApplicationGuidIfMissing(provider);

            InstallSynchronizerForCoreOnly(provider.ApplicationGUID);

            return sourceProviderWithApplicationGuid
                .Bind(x => ValidateProvider(dataProviderFactory, x))
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

        private Either<string, IntegrationPoints.Contracts.SourceProvider> ValidateProvider(
            IDataProviderFactory dataProviderFactory,
            IntegrationPoints.Contracts.SourceProvider provider)
        {
            return TryLoadingProvider(dataProviderFactory, provider);
        }

        private Either<string, IntegrationPoints.Contracts.SourceProvider> TryLoadingProvider(
            IDataProviderFactory dataProviderFactory,
            IntegrationPoints.Contracts.SourceProvider provider)
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

        private async Task<Either<string, Unit>> AddOrUpdateProvider(IntegrationPoints.Contracts.SourceProvider provider)
        {
            IDictionary<Guid, SourceProvider> installedRdoProviderDict = await GetInstalledRdoProviders(provider).ConfigureAwait(false);

            if (installedRdoProviderDict.ContainsKey(provider.GUID))
            {
                SourceProvider providerToUpdate = installedRdoProviderDict[provider.GUID];
                return UpdateExistingProvider(providerToUpdate, provider);
            }
            return AddProvider(provider);
        }

        private async Task<IDictionary<Guid, SourceProvider>> GetInstalledRdoProviders(IntegrationPoints.Contracts.SourceProvider provider)
        {
            List<SourceProvider> installedRdoProviders = await _sourceProviderRepository
                .GetSourceProviderRdoByApplicationIdentifierAsync(provider.ApplicationGUID)
                .ConfigureAwait(false);

            Dictionary<Guid, SourceProvider> installedRdoProviderDict =
                installedRdoProviders.ToDictionary(x => Guid.Parse(x.Identifier), x => x);
            return installedRdoProviderDict;
        }

        private Either<string, Unit> UpdateExistingProvider(SourceProvider existingProviderDto, IntegrationPoints.Contracts.SourceProvider provider)
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

        private Either<string, Unit> AddProvider(IntegrationPoints.Contracts.SourceProvider newProvider)
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

        private Either<string, IntegrationPoints.Contracts.SourceProvider> UpdateApplicationGuidIfMissing(IntegrationPoints.Contracts.SourceProvider provider)
        {
            // when we migrate providers, we should already know which app does the provider belong to.
            if (provider.ApplicationGUID == Guid.Empty)
            {
                return GetApplicationGuid(provider.ApplicationID)
                    .Map(applicationGuid => UpdateApplicationGuid(provider, applicationGuid));
            }
            return provider;
        }

        private IntegrationPoints.Contracts.SourceProvider UpdateApplicationGuid(
            IntegrationPoints.Contracts.SourceProvider provider, Guid newApplicationGuid)
        {
            provider.ApplicationGUID = newApplicationGuid;
            return provider;
        }

        private Either<string, Guid> GetApplicationGuid(int applicationID)
        {
            return new GetApplicationGuid(_dbContext).Execute(applicationID);
        }
    }
}
