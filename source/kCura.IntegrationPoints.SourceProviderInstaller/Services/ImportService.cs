﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Domain;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;
using Relativity.API;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Services
{
	internal class ImportService : IImportService
	{
		private readonly ICaseServiceContext _caseContext;
		private readonly IEddsServiceContext _eddsContext;
		private readonly DeleteIntegrationPoints _deleteintegrationPoint;
		private readonly IHelper _helper;

		public ImportService(
			ICaseServiceContext caseContext,
			IEddsServiceContext eddsContext,
			DeleteIntegrationPoints deleteIntegrationPoints,
			IHelper helper)
		{
			_caseContext = caseContext;
			_eddsContext = eddsContext;
			_deleteintegrationPoint = deleteIntegrationPoints;
			_helper = helper;
		}

		public void InstallProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providers)
		{
			IList<SourceProvider> sourceProviders = providers as IList<SourceProvider> ?? providers.ToList();
			int applicationID = sourceProviders.Select(x => x.ApplicationID).First();
			Guid applicationGuid = GetApplicationGuid(applicationID);
			sourceProviders.ToList().ForEach(x => x.ApplicationGUID = applicationGuid);

			InstallSynchronizerForCoreOnly(applicationGuid);

			ValidateProviders(sourceProviders);

			List<Data.SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);
			Dictionary<string, SourceProviderInstaller.SourceProvider> installingProviderDict = sourceProviders.ToDictionary(x => x.GUID.ToString(), x => x);
			Dictionary<string, Data.SourceProvider> installedRdoProviderDict = installedRdoProviders.ToDictionary(x => x.Identifier, x => x);

			List<Data.SourceProvider> providersToBeUpdated =
				installedRdoProviders.Where(x => installingProviderDict.ContainsKey(x.Identifier)).ToList();

			List<SourceProviderInstaller.SourceProvider> providersToBeInstalled =
				sourceProviders.Where(x => !installedRdoProviderDict.ContainsKey(x.GUID.ToString())).ToList();

			UpdateExistingProviders(providersToBeUpdated, sourceProviders);

			AddNewProviders(providersToBeInstalled);
		}

		public void UninstallProvider(int applicationID)
		{
			try
			{
				Guid applicationGuid = GetApplicationGuid(applicationID);
				List<Data.SourceProvider> installedRdoProviders =
					new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);
				_deleteintegrationPoint.DeleteIPsWithSourceProvider(installedRdoProviders.Select(x => x.ArtifactId).ToList());
				RemoveProviders(installedRdoProviders);
			}
			catch
			{ }
		}

		private void InstallSynchronizerForCoreOnly(Guid applicationGuid)
		{
			//This is hack untill we introduce installation of Destination Providers
			if (applicationGuid == new Guid(Application.GUID))
			{
				new Core.Services.Synchronizer.RDOSynchronizerProvider(_caseContext).CreateOrUpdateLdapSourceType();
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
			var enumerated = providersToBeUpdated.ToList();
			if (enumerated.Any())
			{
				enumerated.ForEach(x => x.Name = providers.Where(y => y.GUID.ToString().Equals(x.Identifier)).Select(y => y.Name).First());
				enumerated.ForEach(x => x.SourceConfigurationUrl = providers.Where(y => y.GUID.ToString().Equals(x.Identifier)).Select(y => y.Url).First());
				enumerated.ForEach(x => x.ViewConfigurationUrl = providers.Where(y => y.GUID.ToString().Equals(x.Identifier)).Select(y => y.ViewDataUrl).First());
				_caseContext.RsapiService.SourceProviderLibrary.Update(enumerated);
			}
		}

		private void AddNewProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providersToBeInstalled)
		{
			IList<SourceProvider> toBeInstalled = providersToBeInstalled as IList<SourceProvider> ?? providersToBeInstalled.ToList();
			if (toBeInstalled.Any())
			{
				IEnumerable<Data.SourceProvider> newProviders =
					from p in toBeInstalled
					select new Data.SourceProvider()
					{
						Name = p.Name,
						ApplicationIdentifier = p.ApplicationGUID.ToString(),
						Identifier = p.GUID.ToString(),
						SourceConfigurationUrl = p.Url,
						ViewConfigurationUrl = p.ViewDataUrl
					};
				AddNewProviders(newProviders);
			}
		}

		private void AddNewProviders(IEnumerable<Data.SourceProvider> newProviders)
		{
			IList<Data.SourceProvider> sourceProviders = newProviders as IList<Data.SourceProvider> ?? newProviders.ToList();
			if (sourceProviders.Any())
			{
				_caseContext.RsapiService.SourceProviderLibrary.Create(sourceProviders);
			}
		}

		private void ValidateProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providers)
		{
			ISourcePluginProvider pluginProvider =
				new DefaultSourcePluginProvider(new GetApplicationBinaries(_eddsContext.SqlContext));
			using (AppDomainFactory factory = new AppDomainFactory(new DomainHelper(), pluginProvider, new RelativityFeaturePathService()))
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
			try
			{
				factory.GetDataProvider(provider.ApplicationGUID, provider.GUID, _helper);
			}
			catch (Exception ex)
			{
				throw new InvalidSourceProviderException(string.Format("Could not load {0} ({1}).", provider.Name, provider.GUID), ex);
			}
		}
	}
}