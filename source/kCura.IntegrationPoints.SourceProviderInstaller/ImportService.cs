using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Queries;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Queries;


namespace kCura.IntegrationPoints.SourceProviderInstaller
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

		public void InstallProviders(IEnumerable<SourceProvider> providers)
		{
			int applicationID = providers.Select(x => x.ApplicationID).First();
			Guid applicationGuid = GetApplicationGuid(applicationID);
			providers.ToList().ForEach(x => x.ApplicationGUID = applicationGuid);

			ValidateProviders(providers, applicationGuid);

			List<Data.SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);
			Dictionary<string, SourceProvider> installingProviderDict = providers.ToDictionary(x => x.GUID.ToString(), x => x);
			Dictionary<string, Data.SourceProvider> installedRdoProviderDict = installedRdoProviders.ToDictionary(x => x.Identifier, x => x);

			List<Data.SourceProvider> providersToBeRemoved =
				installedRdoProviders.Where(x => !installingProviderDict.ContainsKey(x.Identifier)).ToList();

			List<SourceProvider> providersToBeInstalled =
				providers.Where(x => !installedRdoProviderDict.ContainsKey(x.GUID.ToString())).ToList();

			if (providersToBeRemoved.Any())
			{
				//TODO: before deleting SourceProviderRDO, 
				//TODO: deactivate corresponding IntegrationPointRDO and delete corresponding queue job
				//TODO: want to use delete event hanler for this case. 
				_caseContext.RsapiService.SourceProviderLibrary.MassDelete(providersToBeRemoved);
			}

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
				_caseContext.RsapiService.SourceProviderLibrary.MassCreate(providersToBeRemoved);
			}
		}

		public void UninstallProvider(int applicationID)
		{
			Guid applicationGuid = GetApplicationGuid(applicationID);
			List<Data.SourceProvider> installedRdoProviders =
				new GetSourceProviderRdoByApplicationIdentifier(_caseContext).Execute(applicationGuid);
		}
		
		private void ValidateProviders(IEnumerable<SourceProvider> providers, Guid applicationGuid)
		{
			foreach (SourceProvider provider in providers)
			{
				TryLoadingProvider(applicationGuid, provider);
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

		private void TryLoadingProvider(Guid applicationGuid, SourceProvider provider)
		{
			Exception loadingException = null;
			IDataSourceProvider dataSourceProvider = null;
			ISourcePluginProvider pluginProvider =
				new DefaultSourcePluginProvider(_caseContext, _eddsContext) { ApplicationGuid = applicationGuid };
			using (AppDomainFactory factory = new AppDomainFactory(new DomainHelper(), pluginProvider))
			{
				try
				{
					dataSourceProvider = factory.GetDataProvider(provider.GUID);
				}
				catch (Exception ex)
				{
					loadingException = ex;
				}
			}
			if (dataSourceProvider == null)
			{
				throw new InvalidSourceProviderException(string.Format("Could not load {0} ({1}).", provider.Name, provider.GUID), loadingException);
			}
		}
	}
}