using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using LanguageExt;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Provider.Internals;

namespace kCura.IntegrationPoints.Core.Provider
{
    public class RipProviderUninstaller : IRipProviderUninstaller
    {
        private readonly IAPILog _logger;
        private readonly ISourceProviderRepository _sourceProviderRepository;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IApplicationGuidFinder _applicationGuidFinder;
        private readonly DeleteIntegrationPoints _deleteIntegrationPoint;

        public RipProviderUninstaller(
            IAPILog logger,
            ISourceProviderRepository sourceProviderRepository,
            IRelativityObjectManager objectManager,
            IApplicationGuidFinder applicationGuidFinder,
            DeleteIntegrationPoints deleteIntegrationPoint)
        {
            _logger = logger;
            _sourceProviderRepository = sourceProviderRepository;
            _objectManager = objectManager;
            _applicationGuidFinder = applicationGuidFinder;
            _deleteIntegrationPoint = deleteIntegrationPoint;
        }

        public Task<Either<string, Unit>> UninstallProvidersAsync(int applicationID)
        {
            return _applicationGuidFinder
                .GetApplicationGuid(applicationID)
                .BindAsync(applicationGuid => UninstallProvidersAsync(applicationID, applicationGuid));
        }

        private async Task<Either<string, Unit>> UninstallProvidersAsync(int applicationID, Guid applicationGuid)
        {
            try
            {
                List<SourceProvider> installedRdoProviders = await _sourceProviderRepository
                    .GetSourceProviderRdoByApplicationIdentifierAsync(applicationGuid)
                    .ConfigureAwait(false);

                _deleteIntegrationPoint.DeleteIPsWithSourceProvider(installedRdoProviders.Select(x => x.ArtifactId).ToList());
                RemoveProviders(installedRdoProviders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while uninstalling provider: {applicationID}", applicationID);
                return $"Exception occured while uninstalling provider: {applicationID}";
            }

            return Unit.Default;
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
