using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using Newtonsoft.Json;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Provider
{
	public class ProviderInstaller // TODO introduce interface
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

		public async Task InstallProvidersAsync(IEnumerable<IntegrationPoints.Contracts.SourceProvider> providersToInstall)
		{
			IDBContext adminCaseDbContext = _helper.GetDBContext(_ADMIN_CASE_ID);
			IPluginProvider pluginProvider = new DefaultSourcePluginProvider(new GetApplicationBinaries(adminCaseDbContext));
			var domainHelper = new DomainHelper(pluginProvider, _helper, new RelativityFeaturePathService());
			var strategy = new AppDomainIsolatedFactoryLifecycleStrategy(domainHelper);
			using (var vendor = new ProviderFactoryVendor(strategy))
			{
				var dataProviderBuilder = new DataProviderBuilder(vendor);

				// install one provider at a time
				foreach (IntegrationPoints.Contracts.SourceProvider provider in providersToInstall)
				{
					// when we migrate providers, we should already know which app does the provider belong to.
					if (provider.ApplicationGUID == Guid.Empty)
					{
						provider.ApplicationGUID = GetApplicationGuid(provider.ApplicationID);
					}

					InstallSynchronizerForCoreOnly(provider.ApplicationGUID);
					ValidateProvider(dataProviderBuilder, provider);

					await AddOrUpdateProvider(provider).ConfigureAwait(false);
				}
			}
		}

		private async Task AddOrUpdateProvider(IntegrationPoints.Contracts.SourceProvider provider)
		{
			IDictionary<Guid, SourceProvider> installedRdoProviderDict = await GetInstalledRdoProviders(provider).ConfigureAwait(false);

			if (installedRdoProviderDict.ContainsKey(provider.GUID))
			{
				SourceProvider providerToUpdate = installedRdoProviderDict[provider.GUID];
				UpdateExistingProvider(providerToUpdate, provider);
			}
			else
			{
				AddProvider(provider);
			}
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

		private void InstallSynchronizerForCoreOnly(Guid applicationGuid) // TODO KK - maybe we should create separate event handler for it?
		{
			//This is hack until we introduce installation of Destination Providers
			if (applicationGuid == new Guid(Domain.Constants.IntegrationPoints.APPLICATION_GUID_STRING))
			{
				new Services.Synchronizer.RdoSynchronizerProvider(_objectManager, _helper).CreateOrUpdateDestinationProviders();
			}
		}

		private void UpdateExistingProvider(SourceProvider existingProviderDto, IntegrationPoints.Contracts.SourceProvider provider)
		{
			existingProviderDto.Name = provider.Name;
			existingProviderDto.SourceConfigurationUrl = provider.Url;
			existingProviderDto.ViewConfigurationUrl = provider.ViewDataUrl;
			existingProviderDto.Config = provider.Configuration;
			existingProviderDto.Configuration = JsonConvert.SerializeObject(provider.Configuration);

			_objectManager.Update(existingProviderDto);
		}

		private void AddProvider(IntegrationPoints.Contracts.SourceProvider newProvider)
		{
			if (newProvider == null)
			{
				return;
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
			_objectManager.Create(providerDto);
		}

		private void ValidateProvider(DataProviderBuilder dataProviderBuilder, IntegrationPoints.Contracts.SourceProvider provider)
		{
			TryLoadingProvider(dataProviderBuilder, provider);
		}

		private void TryLoadingProvider(DataProviderBuilder factory, IntegrationPoints.Contracts.SourceProvider provider)
		{
			try
			{
				factory.GetDataProvider(provider.ApplicationGUID, provider.GUID);
			}
			catch (Exception ex)
			{
				//throw new InvalidSourceProviderException($"Error while loading '{provider.Name}' provider: {ex.Message}", ex); // TODO throw better exception
				throw ex;
			}
		}

		private Guid GetApplicationGuid(int applicationID) // TODO it is duplicate here and in uninstaller
		{
			Guid? applicationGuid = null;
			Exception operationException = null;
			try
			{
				applicationGuid = new GetApplicationGuid(_dbContext).Execute(applicationID);
			}
			catch (Exception ex)
			{
				operationException = ex;
			}
			if (!applicationGuid.HasValue)
			{
				// throw new InvalidSourceProviderException("Could not retrieve Application Guid.", operationException); // TODO move exception here
			}

			return applicationGuid.Value;
		}
	}
}
