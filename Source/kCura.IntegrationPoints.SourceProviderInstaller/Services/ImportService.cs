using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Services
{
	internal class ImportService : IImportService
	{
		private readonly ICaseServiceContext _caseContext;
		private readonly IEddsServiceContext _eddsServiceContext;
		private readonly DeleteIntegrationPoints _deleteIntegrationPoint;
		private readonly IHelper _helper;

		public ImportService(
			ICaseServiceContext caseContext,
			IEddsServiceContext eddsServiceContext,
			DeleteIntegrationPoints deleteIntegrationPoints,
			IHelper helper)
		{
			_caseContext = caseContext;
			_eddsServiceContext = eddsServiceContext;
			_deleteIntegrationPoint = deleteIntegrationPoints;
			_helper = helper;
		}

		public void InstallProviders(IList<SourceProvider> providers)
		{
			IPluginProvider pluginProvider = new DefaultSourcePluginProvider(new GetApplicationBinaries(_eddsServiceContext.SqlContext));
			var domainHelper = new DomainHelper(pluginProvider, _helper, new RelativityFeaturePathService());
			var strategy = new AppDomainIsolatedFactoryLifecycleStrategy(domainHelper);
			using (var vendor = new ProviderFactoryVendor(strategy))
			{
				var dataProviderBuilder = new DataProviderBuilder(vendor);

				// install one provider at a time
				foreach (SourceProvider provider in providers)
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

		private void AddOrUpdateProvider(SourceProvider provider)
		{
			Dictionary<Guid, Data.SourceProvider> installedRdoProviderDict = GetInstalledRdoProviders(provider);

			if (installedRdoProviderDict.ContainsKey(provider.GUID))
			{
				Data.SourceProvider providerToUpdate = installedRdoProviderDict[provider.GUID];
				UpdateExistingProvider(providerToUpdate, provider);
			}
			else
			{
				AddProvider(provider);
			}
		}

		private Dictionary<Guid, Data.SourceProvider> GetInstalledRdoProviders(SourceProvider provider)
		{
			List<Data.SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(provider.ApplicationGUID);
			Dictionary<Guid, Data.SourceProvider> installedRdoProviderDict =
				installedRdoProviders.ToDictionary(x => Guid.Parse(x.Identifier), x => x);
			return installedRdoProviderDict;
		}

		public void UninstallProviders(int applicationID)
		{
			try
			{
				Guid applicationGuid = GetApplicationGuid(applicationID);
				List<Data.SourceProvider> installedRdoProviders =
					new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);
				_deleteIntegrationPoint.DeleteIPsWithSourceProvider(installedRdoProviders.Select(x => x.ArtifactId).ToList());
				RemoveProviders(installedRdoProviders);
			}
			catch
			{ }
		}

		private void InstallSynchronizerForCoreOnly(Guid applicationGuid)
		{
			//This is hack until we introduce installation of Destination Providers
			if (applicationGuid == new Guid(Domain.Constants.IntegrationPoints.APPLICATION_GUID_STRING))
			{
				new Core.Services.Synchronizer.RdoSynchronizerProvider(_caseContext, _helper).CreateOrUpdateDestinationProviders();
			}
		}

		private void RemoveProviders(IEnumerable<Data.SourceProvider> providersToBeRemoved)
		{
			if (providersToBeRemoved.Any())
			{
				//TODO: before deleting SourceProviderRDO, 
				//TODO: deactivate corresponding IntegrationPointRDO and delete corresponding queue job
				//TODO: want to use delete event handler for this case. 
				_caseContext.RsapiService.SourceProviderLibrary.Delete(providersToBeRemoved);
			}
		}

		private void UpdateExistingProvider(Data.SourceProvider existingProviderDto, SourceProvider provider)
		{
			existingProviderDto.Name = provider.Name;
			existingProviderDto.SourceConfigurationUrl = provider.Url;
			existingProviderDto.ViewConfigurationUrl = provider.ViewDataUrl;
			existingProviderDto.Config = provider.Configuration;
			existingProviderDto.Configuration = JsonConvert.SerializeObject(provider.Configuration);

			_caseContext.RsapiService.RelativityObjectManager.Update(existingProviderDto);
		}

		private void AddProvider(SourceProvider newProvider)
		{
			if (newProvider == null)
			{
				return;
			}

			var providerDto = new Data.SourceProvider
			{
				Name = newProvider.Name,
				ApplicationIdentifier = newProvider.ApplicationGUID.ToString(),
				Identifier = newProvider.GUID.ToString(),
				SourceConfigurationUrl = newProvider.Url,
				ViewConfigurationUrl = newProvider.ViewDataUrl,
				Config = newProvider.Configuration
			};
			_caseContext.RsapiService.RelativityObjectManager.Create(providerDto);
		}

		private void ValidateProvider(DataProviderBuilder dataProviderBuilder, SourceProvider provider)
		{
			TryLoadingProvider(dataProviderBuilder, provider);
		}


		private Guid GetApplicationGuid(int applicationID)
		{
			Guid? applicationGuid = null;
			Exception operationException = null;
			try
			{
				applicationGuid = new GetApplicationGuid(_caseContext.SqlContext).Execute(applicationID);
			}
			catch (Exception ex)
			{
				operationException = ex;
			}
			if (!applicationGuid.HasValue)
			{
				throw new InvalidSourceProviderException("Could not retrieve Application Guid.", operationException);
			}

			return applicationGuid.Value;
		}

		private void TryLoadingProvider(DataProviderBuilder factory, SourceProvider provider)
		{
			try
			{
				factory.GetDataProvider(provider.ApplicationGUID, provider.GUID);
			}
			catch (Exception ex)
			{
				throw new InvalidSourceProviderException($"Error while loading '{provider.Name}' provider: {ex.Message}", ex);
			}
		}
	}
}