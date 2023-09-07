using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using LanguageExt;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Toggles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Provider
{
    public class RipProviderInstaller : IRipProviderInstaller
    {
        private readonly IAPILog _logger;
        private readonly IDataProviderFactoryFactory _dataProviderFactoryFactory;
        private readonly IToggleProvider _toggleProvider;
        readonly ISourceProviderRepository _sourceProviderRepository;
        private readonly IApplicationGuidFinder _applicationGuidFinder;

        public RipProviderInstaller(
            IAPILog logger,
            ISourceProviderRepository sourceProviderRepository,
            IApplicationGuidFinder applicationGuidFinder,
            IDataProviderFactoryFactory dataProviderFactoryFactory,
            IToggleProvider toggleProvider)
        {
            _logger = logger;

            _applicationGuidFinder = applicationGuidFinder;
            _dataProviderFactoryFactory = dataProviderFactoryFactory;
            _toggleProvider = toggleProvider;
            _sourceProviderRepository = sourceProviderRepository;
        }

        public async Task<Either<string, Unit>> InstallProvidersAsync(IEnumerable<global::Relativity.IntegrationPoints.Contracts.SourceProvider> providersToInstall)
        {
            if (providersToInstall == null)
            {
                return $"Argument '{nameof(providersToInstall)}' cannot be null";
            }

            _logger.LogInformation("Installing Source Providers");

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

        private Task<Either<string, Unit>> InstallProvidersInternalAsync(
            IEnumerable<global::Relativity.IntegrationPoints.Contracts.SourceProvider> providersToInstall)
        {
            return _dataProviderFactoryFactory
                .CreateProviderFactoryVendor()
                .BindAsync(providerFactoryVendor => InstallProvidersOneByOneAsync(providerFactoryVendor, providersToInstall));
        }

        private async Task<Either<string, Unit>> InstallProvidersOneByOneAsync(
            ProviderFactoryVendor providerFactoryVendor,
            IEnumerable<global::Relativity.IntegrationPoints.Contracts.SourceProvider> providersToInstall)
        {
            using (providerFactoryVendor)
            {
                IDataProviderFactory dataProviderFactory = _dataProviderFactoryFactory.CreateDataProviderFactory(providerFactoryVendor);
                return await InstallProvidersOneByOneAsync(dataProviderFactory, providersToInstall).ConfigureAwait(false);
            }
        }

        private async Task<Either<string, Unit>> InstallProvidersOneByOneAsync(
            IDataProviderFactory dataProviderFactory,
            IEnumerable<global::Relativity.IntegrationPoints.Contracts.SourceProvider> providersToInstall)
        {
            foreach (global::Relativity.IntegrationPoints.Contracts.SourceProvider provider in providersToInstall)
            {
                _logger.LogInformation("Installing Source Provider GUID: {guid}", provider.GUID);
                Either<string, Unit> installProviderResult = await InstallProviderAsync(dataProviderFactory, provider).ConfigureAwait(false);
                if (installProviderResult.IsLeft)
                {
                    return installProviderResult;
                }
            }
            return Unit.Default;
        }

        private Task<Either<string, Unit>> InstallProviderAsync(
            IDataProviderFactory dataProviderFactory,
            global::Relativity.IntegrationPoints.Contracts.SourceProvider provider)
        {
            return UpdateApplicationGuidIfMissing(provider)
                .BindAsync(AddOrUpdateProviderAsync);
        }

        private async Task<Either<string, Unit>> AddOrUpdateProviderAsync(global::Relativity.IntegrationPoints.Contracts.SourceProvider provider)
        {
            List<SourceProvider> installedRdoProviders = await GetInstalledRdoProvidersAsync(provider.ApplicationGUID).ConfigureAwait(false);
            SourceProvider sourceProvider = installedRdoProviders.FirstOrDefault(x => Guid.Parse(x.Identifier) == provider.GUID);

            if (sourceProvider != null)
            {
                return UpdateExistingProvider(sourceProvider, provider);
            }
            return AddProvider(provider);
        }

        private async Task<List<SourceProvider>> GetInstalledRdoProvidersAsync(Guid applicationGuid)
        {
            List<SourceProvider> installedSourceProviders = await _sourceProviderRepository
                .GetSourceProviderRdoByApplicationIdentifierAsync(applicationGuid, ExecutionIdentity.System)
                .ConfigureAwait(false);

            List<SourceProvider> deduplicatedProviders = new List<SourceProvider>();

            foreach (SourceProvider installedProvider in installedSourceProviders)
            {
                if (deduplicatedProviders.All(x => x.Identifier != installedProvider.Identifier))
                {
                    deduplicatedProviders.Add(installedProvider);
                }
            }

            if (installedSourceProviders.Count > deduplicatedProviders.Count)
            {
                // REL-539111
                _logger.LogWarning("There are duplicated entries in SourceProvider database table.");
            }

            return deduplicatedProviders;
        }

        private Either<string, Unit> UpdateExistingProvider(SourceProvider existingProviderDto, global::Relativity.IntegrationPoints.Contracts.SourceProvider provider)
        {
            _logger.LogInformation("Updating existing provider GUID: {guid}", existingProviderDto.Identifier);

            existingProviderDto.Name = provider.Name;
            existingProviderDto.SourceConfigurationUrl = provider.Url;
            existingProviderDto.ViewConfigurationUrl = provider.ViewDataUrl;
            existingProviderDto.Config = provider.Configuration;
            existingProviderDto.Configuration = JsonConvert.SerializeObject(provider.Configuration);

            try
            {
                _sourceProviderRepository.Update(existingProviderDto, ExecutionIdentity.System);
                _logger.LogInformation("Updated existing {object} - {artifactID}", nameof(SourceProvider), existingProviderDto.ArtifactId);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                return $"Error occured while updating {nameof(SourceProvider)}. Exception: {ex.Message}";
            }
        }

        private Either<string, Unit> AddProvider(global::Relativity.IntegrationPoints.Contracts.SourceProvider newProvider)
        {
            if (newProvider == null)
            {
                return "Cannot add null provider";
            }

            _logger.LogInformation("Adding Source Provider GUID: {guid}", newProvider.GUID);

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
                int sourceProviderArtifactID = _sourceProviderRepository.Create(providerDto, ExecutionIdentity.System);
                _logger.LogInformation("Created new {object} - {artifactID}", nameof(SourceProvider), sourceProviderArtifactID);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                return $"Error occured while adding {nameof(SourceProvider)} to workspace. Exception: {ex.Message}";
            }
        }

        private Either<string, global::Relativity.IntegrationPoints.Contracts.SourceProvider>
            UpdateApplicationGuidIfMissing(global::Relativity.IntegrationPoints.Contracts.SourceProvider provider)
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

        private global::Relativity.IntegrationPoints.Contracts.SourceProvider UpdateApplicationGuid(
            global::Relativity.IntegrationPoints.Contracts.SourceProvider provider, Guid newApplicationGuid)
        {
            provider.ApplicationGUID = newApplicationGuid;
            return provider;
        }
    }
}
