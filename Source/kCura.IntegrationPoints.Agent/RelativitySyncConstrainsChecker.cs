﻿using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent
{
	internal sealed class RelativitySyncConstrainsChecker : IRelativitySyncConstrainsChecker
	{
		private readonly IAPILog _logger;
		private readonly IProviderTypeService _providerTypeService;
		private readonly IToggleProvider _toggleProvider;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly ISerializer _serializer;

		public RelativitySyncConstrainsChecker(IIntegrationPointService integrationPointService,
			IProviderTypeService providerTypeService,
			IToggleProvider toggleProvider,
			ISerializer serializer,
			IAPILog logger)
		{
			_integrationPointService = integrationPointService;
			_providerTypeService = providerTypeService;
			_toggleProvider = toggleProvider;
			_serializer = serializer;
			_logger = logger;
		}

		public bool ShouldUseRelativitySync(Job job)
		{
			_logger.LogInformation("Checking if Relativity Sync flow should be used for job with ID: {jobId}. IntegrationPointId: {integrationPointId}", job.JobId);
			
            try
			{
				IntegrationPoint integrationPoint = GetIntegrationPoint(job.RelatedObjectArtifactID);
				ProviderType providerType = GetProviderType(integrationPoint.SourceProvider ?? 0,
					integrationPoint.DestinationProvider ?? 0);

				if (providerType == ProviderType.Relativity)
				{
					SourceConfiguration sourceConfiguration = VerboseDeserialize<SourceConfiguration>(integrationPoint.SourceConfiguration);
					ImportSettings importSettings = VerboseDeserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

					if (ConfigurationAllowsUsingRelativitySync(sourceConfiguration, importSettings))
					{
						if (importSettings.ImageImport && !IsToggleEnabled<EnableSyncImageFlowToggle>())
						{
							_logger.LogInformation(
								"Old image import flow will be used for job with ID: {jobId} because toggle '{typeof(EnableSyncImageFlowToggle).FullName}' is disabled. IntegrationPointId: {integrationPointId}",
								job.JobId, job.RelatedObjectArtifactID);

							return false;
						}

                        _logger.LogInformation("Relativity Sync flow will be used for job with ID: {jobId}. IntegrationPointId: {integrationPointId}",
							job.JobId, job.RelatedObjectArtifactID);

						return true;
					}
				}

				_logger.LogInformation(
					"Normal flow will be used for job with ID: {jobId} because this integration point does not meet conditions required for running Relativity Sync. IntegrationPointId: {integrationPointId}",
					job.JobId, job.RelatedObjectArtifactID);
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred when checking if Integration Point should use Relativity Sync workflow");
				return false;
			}
		}

        private bool IsToggleEnabled<T>() where T: IToggle
        {
            _logger.LogDebug($"Checking if {nameof(T)} is enabled.");

            try
            {
                bool isEnabled = _toggleProvider.IsEnabled<T>();
                _logger.LogInformation($"Confirmed that {nameof(T)} is {(isEnabled ? "enabled" : "disabled")}.");
                return isEnabled;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Checking if {nameof(T)} is enabled operation failed.");
                throw;
            }
		}

		private IntegrationPoint GetIntegrationPoint(int integrationPointId)
		{
			try
			{
				IntegrationPoint integrationPoint = _integrationPointService.ReadIntegrationPoint(integrationPointId);
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