using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.RelativitySync;
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
                    ImportSettings destinationConfiguration = VerboseDeserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

                    if (ConfigurationAllowsUsingRelativitySync(sourceConfiguration, destinationConfiguration))
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
                IEnumerable<Guid> integrationPointFields = new[]
                {
                    IntegrationPointFieldGuids.SourceProviderGuid,
                    IntegrationPointFieldGuids.DestinationProviderGuid,
                    IntegrationPointFieldGuids.SourceConfigurationGuid,
                    IntegrationPointFieldGuids.DestinationConfigurationGuid
                };

                IntegrationPoint integrationPoint = _relativityObjectManager.Read<IntegrationPoint>(integrationPointId, integrationPointFields);

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
                    providerType,
                    sourceProviderId,
                    destinationProviderId);
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

        private bool ConfigurationAllowsUsingRelativitySync(SourceConfiguration sourceConfiguration, ImportSettings destinationConfiguration)
        {
            _logger.LogInformation(
                "Checking if configurations allow using RelativitySync. " +
                    "DestinationConfiguration.ArtifactTypeId: {artifactTypeId}; " +
                    "SourceConfiguration.TypeOfExport: {typeOfExport}; " +
                    "DestinationConfiguration.ImageImport: {imageImport}; " +
                    "DestinationConfiguration.ProductionImport: {productionImport}",
                destinationConfiguration.ArtifactTypeId,
                sourceConfiguration.TypeOfExport,
                destinationConfiguration.ImageImport,
                destinationConfiguration.ProductionImport);

            bool isSyncDocumentFlow = sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch && !destinationConfiguration.ProductionImport;
            bool isSyncNonDocumentFlow = IsNonDocumentFlow(destinationConfiguration);

            return isSyncDocumentFlow || isSyncNonDocumentFlow;
        }

        private bool IsNonDocumentFlow(ImportSettings destinationConfiguration)
        {
            return destinationConfiguration.ArtifactTypeId != (int)ArtifactType.Document;
        }
    }
}
