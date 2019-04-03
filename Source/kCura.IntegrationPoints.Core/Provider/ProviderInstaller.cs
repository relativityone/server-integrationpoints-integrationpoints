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
		private readonly IAPILog _logger;
		private readonly IRelativityObjectManager _objectManager;
		private readonly IDBContext _dbContext;
		private readonly IHelper _helper;

		private const int _ADMIN_CASE_ID = -1;

		public ProviderInstaller(
			IAPILog logger,
			IRelativityObjectManager objectManager,
			IDBContext dbContext,
			IHelper helper)
		{
			_logger = logger;
			_objectManager = objectManager;
			_dbContext = dbContext;
			_helper = helper;
		}

		public async Task InstallProvidersAsync(IEnumerable<SourceProviderInstaller.SourceProvider> providersToInstall)
		{
			await Task.Yield(); // TODO remove it

			IDBContext adminCaseDbContext = _helper.GetDBContext(_ADMIN_CASE_ID);
			IPluginProvider pluginProvider = new DefaultSourcePluginProvider(new GetApplicationBinaries(adminCaseDbContext));
			var domainHelper = new DomainHelper(pluginProvider, _helper, new RelativityFeaturePathService());
			var strategy = new AppDomainIsolatedFactoryLifecycleStrategy(domainHelper);
			using (var vendor = new ProviderFactoryVendor(strategy))
			{
				var dataProviderBuilder = new DataProviderBuilder(vendor);

				// install one provider at a time
				foreach (SourceProviderInstaller.SourceProvider provider in providersToInstall)
				{
					// when we migrate providers, we should already know which app does the provider belong to.
					if (provider.ApplicationGUID == Guid.Empty)
					{
						provider.ApplicationGUID = GetApplicationGuid(provider.ApplicationID);
					}

					InstallSynchronizerForCoreOnly(provider.ApplicationGUID);
					ValidateProvider(dataProviderBuilder, provider);

					AddOrUpdateProvider(provider);
				}
			}
		}

		private void AddOrUpdateProvider(SourceProviderInstaller.SourceProvider provider)
		{
			Dictionary<Guid, SourceProvider> installedRdoProviderDict = GetInstalledRdoProviders(provider);

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

		private Dictionary<Guid, SourceProvider> GetInstalledRdoProviders(SourceProviderInstaller.SourceProvider provider)
		{
			List<SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_objectManager).Execute(provider.ApplicationGUID);
			Dictionary<Guid, SourceProvider> installedRdoProviderDict =
				installedRdoProviders.ToDictionary(x => Guid.Parse(x.Identifier), x => x);
			return installedRdoProviderDict;
		}

		private void InstallSynchronizerForCoreOnly(Guid applicationGuid) // TODO KK - maybe we should create decorator which should be used for RIP providers????
		{
			//This is hack until we introduce installation of Destination Providers
			if (applicationGuid == new Guid(Domain.Constants.IntegrationPoints.APPLICATION_GUID_STRING))
			{
				new Services.Synchronizer.RdoSynchronizerProvider(_objectManager, _helper).CreateOrUpdateDestinationProviders();
			}
		}

		private void UpdateExistingProvider(SourceProvider existingProviderDto, SourceProviderInstaller.SourceProvider provider)
		{
			existingProviderDto.Name = provider.Name;
			existingProviderDto.SourceConfigurationUrl = provider.Url;
			existingProviderDto.ViewConfigurationUrl = provider.ViewDataUrl;
			existingProviderDto.Config = provider.Configuration;
			existingProviderDto.Configuration = JsonConvert.SerializeObject(provider.Configuration);

			_objectManager.Update(existingProviderDto);
		}

		private void AddProvider(SourceProviderInstaller.SourceProvider newProvider)
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

		private void ValidateProvider(DataProviderBuilder dataProviderBuilder, SourceProviderInstaller.SourceProvider provider)
		{
			TryLoadingProvider(dataProviderBuilder, provider);
		}

		private void TryLoadingProvider(DataProviderBuilder factory, SourceProviderInstaller.SourceProvider provider)
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
