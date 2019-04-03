using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Provider
{
	public class ProviderUninstaller
	{
		private readonly IAPILog _logger;
		private readonly ISourceProviderRepository _sourceProviderRepository;
		private readonly IRelativityObjectManager _objectManager;
		private readonly IDBContext _dbContext;
		private readonly DeleteIntegrationPoints _deleteIntegrationPoint;

		public ProviderUninstaller(
			IAPILog logger,
			ISourceProviderRepository sourceProviderRepository,
			IRelativityObjectManager objectManager,
			IDBContext dbContext,
			DeleteIntegrationPoints deleteIntegrationPoint)
		{
			_logger = logger;
			_sourceProviderRepository = sourceProviderRepository;
			_objectManager = objectManager;
			_dbContext = dbContext;
			_deleteIntegrationPoint = deleteIntegrationPoint;
		}

		public async Task UninstallProvidersAsync(int applicationID)
		{
			try
			{
				Guid applicationGuid = GetApplicationGuid(applicationID);
				List<SourceProvider> installedRdoProviders = await _sourceProviderRepository
					.GetSourceProviderRdoByApplicationIdentifierAsync(applicationGuid)
					.ConfigureAwait(false);

				_deleteIntegrationPoint.DeleteIPsWithSourceProvider(installedRdoProviders.Select(x => x.ArtifactId).ToList());
				RemoveProviders(installedRdoProviders);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception occured while uninstalling provider: {applicationID}", applicationID);
				// TODO should we throw???
			}
		}

		private Guid GetApplicationGuid(int applicationID)
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


		private void RemoveProviders(IEnumerable<SourceProvider> providersToBeRemoved)
		{
			//TODO: before deleting SourceProviderRDO, 
			//TODO: deactivate corresponding IntegrationPointRDO and delete corresponding queue job
			//TODO: want to use delete event handler for this case. 

			foreach (SourceProvider sourceProvider in providersToBeRemoved)
			{
				_objectManager.Delete(sourceProvider);
			}
		}
	}
}
