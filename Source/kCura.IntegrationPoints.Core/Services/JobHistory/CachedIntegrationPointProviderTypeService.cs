using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class CachedIntegrationPointProviderTypeService : IIntegrationPointProviderTypeService
    {
        private DateTime _lastRefreshTime = DateTime.MinValue;

        private readonly IIntegrationPointService _integrationPointService;
        private readonly IProviderTypeService _providerTypeService;

        private readonly IDateTimeHelper _currentDateTimeProvider;
        private readonly TimeSpan _cacheRefreshDelay;
        private readonly Dictionary<int, ProviderType> _providerTypesCache = new Dictionary<int, ProviderType>();
        private readonly object _providerTypesCacheLock = new object();

        public CachedIntegrationPointProviderTypeService(
            IProviderTypeService providerTypeService,
            IIntegrationPointService integrationPointService,
            IDateTimeHelper currentDateTimeProvider,
            TimeSpan cacheRefreshDelay)
        {
            _cacheRefreshDelay = cacheRefreshDelay;
            _providerTypeService = providerTypeService;
            _currentDateTimeProvider = currentDateTimeProvider;
            _integrationPointService = integrationPointService;
        }

        public ProviderType GetProviderType(int integrationPointArtifactId)
        {
            return GetProviderType(integrationPointArtifactId, null);
        }

        public ProviderType GetProviderType(IntegrationPointDto integrationPoint)
        {
            return GetProviderType(integrationPoint.ArtifactId, integrationPoint);
        }

        private ProviderType GetProviderType(int integrationPointArtifactId, IntegrationPointDto integrationPoint)
        {
            lock (_providerTypesCacheLock)
            {
                RefreshCache();

                return GetProviderTypeFromCache(integrationPointArtifactId, integrationPoint);
            }
        }

        private ProviderType GetProviderTypeFromCache(int integrationPointArtifactId, IntegrationPointDto integrationPoint)
        {
            ProviderType providerType;
            if (_providerTypesCache.TryGetValue(integrationPointArtifactId, out providerType))
            {
                return providerType;
            }

            providerType = GetProviderTypeFromService(integrationPointArtifactId, integrationPoint);

            _providerTypesCache[integrationPointArtifactId] = providerType;

            return providerType;
        }


        private ProviderType GetProviderTypeFromService(int integrationPointArtifactID, IntegrationPointDto integrationPoint)
        {
            if (integrationPoint == null)
            {
                integrationPoint = _integrationPointService.ReadSlim(integrationPointArtifactID);
            }

            ProviderType providerType = integrationPoint.GetProviderType(_providerTypeService);

            return providerType;
        }

        private void RefreshCache()
        {
            DateTime currentTime = _currentDateTimeProvider.Now();
            TimeSpan timeSinceLastRefresh = currentTime - _lastRefreshTime;
            if (timeSinceLastRefresh > _cacheRefreshDelay)
            {
                _providerTypesCache.Clear();
                _lastRefreshTime = currentTime;
            }
        }
    }
}
