using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Synchronizer
{
    public class RdoSynchronizerProvider : IRdoSynchronizerProvider
    {
        private readonly IDestinationProviderRepository _destinationProviderRepository;
        private readonly IAPILog _logger;
        public const string RDO_SYNC_TYPE_GUID = "74A863B9-00EC-4BB7-9B3E-1E22323010C6";
        public const string FILES_SYNC_TYPE_GUID = "1D3AD995-32C5-48FE-BAA5-5D97089C8F18";

        public RdoSynchronizerProvider(IDestinationProviderRepository destinationProviderRepository, IAPILog logger)
        {
            _destinationProviderRepository = destinationProviderRepository;
            _logger = logger.ForContext<RdoSynchronizerProvider>();
        }

        public virtual void CreateOrUpdateDestinationProviders()
        {
            CreateOrUpdateDestinationProvider("Relativity", RDO_SYNC_TYPE_GUID);
            CreateOrUpdateDestinationProvider("Load File", FILES_SYNC_TYPE_GUID);
        }

        public int GetRdoSynchronizerId()
        {
            DestinationProvider destinationProvider = _destinationProviderRepository.ReadByProviderGuid(RDO_SYNC_TYPE_GUID);
            if (destinationProvider != null)
            {
                return destinationProvider.ArtifactId;
            }
            string errorMessage = FormatUnableToRetrieveDestinationProviderErrorMessage(RDO_SYNC_TYPE_GUID);
            _logger.LogError(errorMessage);

            throw new IntegrationPointsException(errorMessage)
            {
                Source = IntegrationPointsExceptionSource.KEPLER
            };
        }

        private void CreateOrUpdateDestinationProvider(string name, string providerGuid)
        {
            DestinationProvider destinationProvider = _destinationProviderRepository.ReadByProviderGuid(providerGuid);
            if (destinationProvider == null)
            {
                CreateDestinationProvider(name, providerGuid);
            }
            else
            {
                UpdateDestinationProviderName(name, providerGuid, destinationProvider);
            }
        }

        private void UpdateDestinationProviderName(string name, string providerGuid, DestinationProvider destinationProvider)
        {
            LogUpdatingProvider(name, providerGuid);
            destinationProvider.Name = name;
            _destinationProviderRepository.Update(destinationProvider);
        }

        private void CreateDestinationProvider(string name, string providerGuid)
        {
            LogCreatingProvider(name, providerGuid);
            var destinationProvider = new DestinationProvider
            {
                Name = name,
                Identifier = providerGuid,
                ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING
            };
            _destinationProviderRepository.Create(destinationProvider);
        }

        #region Logging

        private void LogCreatingProvider(string name, string providerGuid)
        {
            _logger.LogInformation("Creating new destination provider {ProviderName} ({ProviderGuid}).", name, providerGuid);
        }

        private void LogUpdatingProvider(string name, string providerGuid)
        {
            _logger.LogInformation("Updating existing destination provider {ProviderName} ({ProviderGuid}).", name, providerGuid);
        }

        private string FormatUnableToRetrieveDestinationProviderErrorMessage(string guid)
        {
            return string.Format(Constants.IntegrationPoints.UNABLE_TO_RETRIEVE_DESTINATION_PROVIDER_GUID, guid);
        }

        #endregion
    }
}
