﻿using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Interfaces;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.RelativitySync
{
    internal sealed class RelativitySyncConstrainsChecker : IRelativitySyncConstrainsChecker
    {
        private readonly IAPILog _logger;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IToggleProvider _toggleProvider;
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly ISerializer _serializer;

        public RelativitySyncConstrainsChecker(
            IRelativityObjectManager relativityObjectManager,
            IProviderTypeService providerTypeService,
            IToggleProvider toggleProvider,
            ISerializer serializer,
            IAPILog logger)
        {
            _relativityObjectManager = relativityObjectManager;
            _providerTypeService = providerTypeService;
            _toggleProvider = toggleProvider;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task<bool> ShouldUseRelativitySyncAppAsync(int integrationPointId)
        {
            _logger.LogInformation("Checking if Relativity Sync application flow should be used for Integration Point ID: {integrationPointId}", integrationPointId);

            bool isToggleEnabled = await _toggleProvider.IsEnabledAsync<EnableRelativitySyncApplicationToggle>().ConfigureAwait(false);

            if (!isToggleEnabled)
            {
                _logger.LogInformation("Toggle {toggleName} is disabled - Relativity Sync application will not be used", typeof(EnableRelativitySyncApplicationToggle).FullName);
                return false;
            }

            bool configurationAllowsUsingSync = ShouldUseRelativitySync(integrationPointId);

            return configurationAllowsUsingSync;
        }

        public bool ShouldUseRelativitySync(int integrationPointId)
        {
            _logger.LogInformation("Checking if Relativity Sync flow should be used for IntegrationPoint ID: {integrationPointId}", integrationPointId);

            try
            {
                IntegrationPoint integrationPoint = GetIntegrationPoint(integrationPointId);
                ProviderType providerType = GetProviderType(
                    integrationPoint.SourceProvider ?? 0,
                    integrationPoint.DestinationProvider ?? 0);

                if (providerType == ProviderType.Relativity)
                {
                    SourceConfiguration sourceConfiguration = VerboseDeserialize<SourceConfiguration>(integrationPoint.SourceConfiguration);
                    ImportSettings importSettings = VerboseDeserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

                    if (ConfigurationAllowsUsingRelativitySync(sourceConfiguration, importSettings))
                    {
                        _logger.LogInformation("Relativity Sync flow will be used for IntegrationPoint ID: {integrationPointId}", integrationPointId);

                        return true;
                    }
                }

                _logger.LogInformation(
                    "Normal flow will be used because this integration point does not meet conditions required for running Relativity Sync. IntegrationPoint ID: {integrationPointId}", integrationPointId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred when checking if Integration Point should use Relativity Sync workflow");
                return false;
            }
        }

        private IntegrationPoint GetIntegrationPoint(int integrationPointId)
        {
            try
            {
                IntegrationPoint integrationPoint = _relativityObjectManager.Read<IntegrationPoint>(integrationPointId, new[]
                {
                    IntegrationPointFieldGuids.SourceProviderGuid,
                    IntegrationPointFieldGuids.DestinationProviderGuid,
                    IntegrationPointFieldGuids.SourceConfigurationGuid,
                    IntegrationPointFieldGuids.DestinationConfigurationGuid
                });

                _logger.LogInformation("Integration Point with Id: {integrationPointId} retrieved successfully.", integrationPointId);
                return integrationPoint;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve Integration Point with id: {integrationPointId}", integrationPointId);
                throw;
            }
        }

        private ProviderType GetProviderType(int sourceProviderId, int destinationProviderId)
        {
            try
            {
                ProviderType providerType = _providerTypeService.GetProviderType(sourceProviderId, destinationProviderId);
                _logger.LogInformation(
                    "ProviderType for given providers has been determined as: {providerType}. SourceProviderId: {sourceProviderId}; DestinationProviderId: {destinationProviderId}",
                    providerType, sourceProviderId, destinationProviderId);
                return providerType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Getting Provider Type operation failed.");
                throw;
            }
        }

        private T VerboseDeserialize<T>(string jsonToDeserialize)
        {
            if (string.IsNullOrWhiteSpace(jsonToDeserialize))
            {
                throw new ArgumentNullException(nameof(jsonToDeserialize));
            }

            try
            {
                T result = _serializer.Deserialize<T>(jsonToDeserialize);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deserializing json for type {nameof(T)} operation failed.");
                throw;
            }
        }

        private bool ConfigurationAllowsUsingRelativitySync(SourceConfiguration sourceConfiguration, ImportSettings importSettings)
        {
            _logger.LogInformation("Checking if configurations allow using RelativitySync. " +
                    "SourceConfiguration.TypeOfExport: {typeOfExport}; " +
                    "DestinationConfiguration.ImageImport: {imageImport}; " +
                    "DestinationConfiguration.ProductionImport: {productionImport}",
                sourceConfiguration.TypeOfExport, importSettings.ImageImport, importSettings.ProductionImport);

            bool isSyncDocumentFlow = sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch && !importSettings.ProductionImport;
            bool isSyncNonDocumentFlow = IsNonDocumentFlow(importSettings);

            return isSyncDocumentFlow || isSyncNonDocumentFlow;
        }

        private bool IsNonDocumentFlow(ImportSettings destinationConfiguration)
        {
            return destinationConfiguration.ArtifactTypeId != (int)ArtifactType.Document;
        }
    }
}
