﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Vendor.Castle.Core.Internal;


namespace kCura.IntegrationPoints.SourceProviderInstaller.Services
{
	public class ImportService : IImportService
	{
		private ICaseServiceContext _caseContext;
		private IEddsServiceContext _eddsContext;
		public ImportService(ICaseServiceContext caseContext, IEddsServiceContext eddsContext)
		{
			_caseContext = caseContext;
			_eddsContext = eddsContext;
		}

		public void InstallProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providers)
		{
			int applicationID = providers.Select(x => x.ApplicationID).First();
			Guid applicationGuid = GetApplicationGuid(applicationID);
			providers.ToList().ForEach(x => x.ApplicationGUID = applicationGuid);

			InstallSyncronizerForCoreOnly(applicationGuid);

			ValidateProviders(providers);

			List<Data.SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);
			Dictionary<string, SourceProviderInstaller.SourceProvider> installingProviderDict = providers.ToDictionary(x => x.GUID.ToString(), x => x);
			Dictionary<string, Data.SourceProvider> installedRdoProviderDict = installedRdoProviders.ToDictionary(x => x.Identifier, x => x);

			List<Data.SourceProvider> providersToBeRemoved =
				installedRdoProviders.Where(x => !installingProviderDict.ContainsKey(x.Identifier)).ToList();

			List<Data.SourceProvider> providersToBeUpdated =
				installedRdoProviders.Where(x => installingProviderDict.ContainsKey(x.Identifier)).ToList();

			List<SourceProviderInstaller.SourceProvider> providersToBeInstalled =
				providers.Where(x => !installedRdoProviderDict.ContainsKey(x.GUID.ToString())).ToList();

			RemoveProviders(providersToBeRemoved);

			UpdateExistingProviders(providersToBeUpdated, providers);

			AddNewProviders(providersToBeInstalled);
		}

		public void UninstallProvider(int applicationID)
		{
			Guid applicationGuid = GetApplicationGuid(applicationID);
			List<Data.SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);

			RemoveProviders(installedRdoProviders);
		}

		private void InstallSyncronizerForCoreOnly(Guid applicationGuid)
		{
			//This is hack untill we introduce installation of Destination Providers
			if (applicationGuid == new Guid(Application.GUID))
			{
				new Core.Services.Syncronizer.RDOSyncronizerProvider(_caseContext).CreateOrUpdateLdapSourceType();
			}
		}

		private void RemoveProviders(IEnumerable<Data.SourceProvider> providersToBeRemoved)
		{
			if (providersToBeRemoved.Any())
			{
				//TODO: before deleting SourceProviderRDO, 
				//TODO: deactivate corresponding IntegrationPointRDO and delete corresponding queue job
				//TODO: want to use delete event hanler for this case. 
				_caseContext.RsapiService.SourceProviderLibrary.Delete(providersToBeRemoved);
			}
		}

		private void UpdateExistingProviders(IEnumerable<Data.SourceProvider> providersToBeUpdated, 
			IEnumerable<SourceProviderInstaller.SourceProvider> providers)
		{
			if (providersToBeUpdated.Any())
			{
				providersToBeUpdated.ForEach(x => x.Name = providers.Where(y => y.GUID.ToString().Equals(x.Identifier)).Select(y => y.Name).First());
				providersToBeUpdated.ForEach(x => x.SourceConfigurationUrl = providers.Where(y => y.GUID.ToString().Equals(x.Identifier)).Select(y => y.Url).First());

				_caseContext.RsapiService.SourceProviderLibrary.Update(providersToBeUpdated);
			}
		}

		private void AddNewProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providersToBeInstalled)
		{
			if (providersToBeInstalled.Any())
			{
				IEnumerable<Data.SourceProvider> newProviders =
					from p in providersToBeInstalled
					select new Data.SourceProvider()
					{
						Name = p.Name,
						ApplicationIdentifier = p.ApplicationGUID.ToString(),
						Identifier = p.GUID.ToString(),
						SourceConfigurationUrl = p.Url
					};
				AddNewProviders(newProviders);
			}
		}

		private void AddNewProviders(IEnumerable<Data.SourceProvider> newProviders)
		{
			if (newProviders.Any())
			{
				_caseContext.RsapiService.SourceProviderLibrary.Create(newProviders);
			}
		}

		private void ValidateProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providers)
		{
			ISourcePluginProvider pluginProvider =
				new DefaultSourcePluginProvider(new GetApplicationBinaries(_eddsContext.SqlContext));
			using (AppDomainFactory factory = new AppDomainFactory(new DomainHelper(), pluginProvider))
			{
				foreach (SourceProviderInstaller.SourceProvider provider in providers)
				{
					TryLoadingProvider(factory, provider);
				}
			}
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

		private void TryLoadingProvider(AppDomainFactory factory, SourceProviderInstaller.SourceProvider provider)
		{
			Exception loadingException = null;
			IDataSourceProvider dataSourceProvider = null;
			try
			{
				dataSourceProvider = factory.GetDataProvider(provider.ApplicationGUID, provider.GUID);
			}
			catch (Exception ex)
			{
				loadingException = ex;
			}
			if (dataSourceProvider == null)
			{
				throw new InvalidSourceProviderException(string.Format("Could not load {0} ({1}).", provider.Name, provider.GUID),
					loadingException);
			}
		}
	}
}