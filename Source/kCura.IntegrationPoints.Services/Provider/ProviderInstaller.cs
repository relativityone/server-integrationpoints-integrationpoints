using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
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

namespace kCura.IntegrationPoints.Services.Provider
{
	internal class ProviderInstaller
	{
		private readonly IRelativityObjectManager _objectManager;

		private readonly ICaseServiceContext _caseContext;
		private readonly IEddsServiceContext _eddsServiceContext;
		private readonly IDBContext _dbContext;
		private readonly IHelper _helper;

		public ProviderInstaller(
			IRelativityObjectManager objectManager,
			ICaseServiceContext caseContext,
			IEddsServiceContext eddsServiceContext,
			IDBContext dbContext,
			IHelper helper)
		{
			_objectManager = objectManager;
			_caseContext = caseContext;
			_eddsServiceContext = eddsServiceContext;
			_dbContext = dbContext;
			_helper = helper;
		}

		public async Task InstallProvidersAsync(IEnumerable<ProviderToInstallDto> providersToInstall)
		{
			await Task.Yield(); // TODO remove it

			IPluginProvider pluginProvider = new DefaultSourcePluginProvider(new GetApplicationBinaries(_eddsServiceContext.SqlContext));
			var domainHelper = new DomainHelper(pluginProvider, _helper, new RelativityFeaturePathService());
			var strategy = new AppDomainIsolatedFactoryLifecycleStrategy(domainHelper);
			using (var vendor = new ProviderFactoryVendor(strategy))
			{
				var dataProviderBuilder = new DataProviderBuilder(vendor);

				// install one provider at a time
				foreach (ProviderToInstallDto provider in providersToInstall)
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

		private void AddOrUpdateProvider(ProviderToInstallDto provider)
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

		private Dictionary<Guid, SourceProvider> GetInstalledRdoProviders(ProviderToInstallDto provider)
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
				new Core.Services.Synchronizer.RdoSynchronizerProvider(_caseContext, _helper).CreateOrUpdateDestinationProviders();
			}
		}

		private void UpdateExistingProvider(SourceProvider existingProviderDto, ProviderToInstallDto provider)
		{
			existingProviderDto.Name = provider.Name;
			existingProviderDto.SourceConfigurationUrl = provider.Url;
			existingProviderDto.ViewConfigurationUrl = provider.ViewDataUrl;
			existingProviderDto.Config = ConvertConfigurationToSourceProviderConfiguration(provider.Configuration);
			existingProviderDto.Configuration = JsonConvert.SerializeObject(provider.Configuration);

			_objectManager.Update(existingProviderDto);
		}

		private void AddProvider(ProviderToInstallDto newProvider)
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
				Config = ConvertConfigurationToSourceProviderConfiguration(newProvider.Configuration)
			};
			_objectManager.Create(providerDto);
		}

		private SourceProviderConfiguration ConvertConfigurationToSourceProviderConfiguration(ProviderToInstallConfigurationDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new SourceProviderConfiguration
			{
				AlwaysImportNativeFileNames = dto.AlwaysImportNativeFileNames,
				AlwaysImportNativeFiles = dto.AlwaysImportNativeFiles,
				CompatibleRdoTypes = dto.CompatibleRdoTypes,
				OnlyMapIdentifierToIdentifier = dto.OnlyMapIdentifierToIdentifier
			};
		}

		private void ValidateProvider(DataProviderBuilder dataProviderBuilder, ProviderToInstallDto provider)
		{
			TryLoadingProvider(dataProviderBuilder, provider);
		}

		private void TryLoadingProvider(DataProviderBuilder factory, ProviderToInstallDto provider)
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
