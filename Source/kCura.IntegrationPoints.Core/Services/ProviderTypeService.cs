using System;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services
{
    public class ProviderTypeService : IProviderTypeService
    {
        private readonly IRelativityObjectManager _objectManager;

        public ProviderTypeService(IRelativityObjectManager objectManager)
        {
            _objectManager = objectManager;
        }

        public string GetProviderName(int sourceProviderId, int destinationProviderId)
        {
            SourceProvider sourceProvider = _objectManager.Read<SourceProvider>(sourceProviderId);
            DestinationProvider destinationProvider = _objectManager.Read<DestinationProvider>(destinationProviderId);
            ProviderType providerType = GetProviderType(sourceProvider.Identifier, destinationProvider.Identifier);

            return providerType != ProviderType.Other
                ? providerType.ToString()
                : string.IsNullOrEmpty(sourceProvider.Name) ? providerType.ToString() : sourceProvider.Name.TrimAll();
        }

        public ProviderType GetProviderType(int sourceProviderId, int destinationProviderId)
        {
            string sourceProviderGuid = _objectManager.Read<SourceProvider>(sourceProviderId).Identifier;
            string destinationProviderGuid = _objectManager.Read<DestinationProvider>(destinationProviderId).Identifier;
            return GetProviderType(sourceProviderGuid, destinationProviderGuid);
        }

        public ProviderType GetProviderType(Data.IntegrationPoint integrationPoint)
        {
            return GetProviderType(
                integrationPoint.SourceProvider.Value,
                integrationPoint.DestinationProvider.Value);
        }

        private ProviderType GetProviderType(string sourceProviderGuid, string destinationProviderGuid)
        {
            if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.FTP, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProviderType.FTP;
            }
            if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.LDAP, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProviderType.LDAP;
            }
            if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE, StringComparison.InvariantCultureIgnoreCase))
            {
                return  ProviderType.ImportLoadFile;
            }
            if (sourceProviderGuid.Equals(Constants.IntegrationPoints.SourceProviders.RELATIVITY, StringComparison.InvariantCultureIgnoreCase))
            {
                if (destinationProviderGuid.Equals(Constants.IntegrationPoints.DestinationProviders.RELATIVITY, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ProviderType.Relativity;
                }
                if (destinationProviderGuid.Equals(Constants.IntegrationPoints.DestinationProviders.LOADFILE, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ProviderType.LoadFile;
                }
            }
            return ProviderType.Other;
        }
    }
}