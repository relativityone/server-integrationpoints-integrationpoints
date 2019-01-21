﻿using System;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent
{
	internal sealed class RelativitySyncConstrainsChecker
	{
		private readonly IAPILog _logger;
		private readonly IProviderTypeService _providerTypeService;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly IToggleProvider _toggleProvider;
		private readonly IConfigurationDeserializer _configurationDeserializer;

		public RelativitySyncConstrainsChecker(IIntegrationPointService integrationPointService,
			IProviderTypeService providerTypeService, IToggleProvider toggleProvider,
			IConfigurationDeserializer configurationDeserializer, IAPILog logger)
		{
			_integrationPointService = integrationPointService;
			_providerTypeService = providerTypeService;
			_toggleProvider = toggleProvider;
			_configurationDeserializer = configurationDeserializer;
			_logger = logger;
		}

		public bool ShouldUseRelativitySync(Job job)
		{
			_logger.LogDebug("Checking if Relativity Sync flow should be used for job with ID: {jobId}. IntegrationPointId: {integrationPointId}", job.JobId);
			try
			{
				if (!IsRelativitySyncToggleEnabled())
				{
					_logger.LogInformation(
						$"Normal flow will be used for job with ID: {{jobId}} because {nameof(EnableSyncToggle)} is disabled. IntegrationPointId: {{integrationPointId}}",
						job.JobId, job.RelatedObjectArtifactID);

					return false;
				}

				IntegrationPoint integrationPoint = GetIntegrationPoint(job.RelatedObjectArtifactID);
				ProviderType providerType = GetProviderType(integrationPoint.SourceProvider ?? 0,
					integrationPoint.DestinationProvider ?? 0);
				if (providerType == ProviderType.Relativity)
				{
					SourceConfiguration sourceConfiguration =
						VerboseDeserialize<SourceConfiguration>(integrationPoint.SourceConfiguration);
					ImportSettings importSettings =
						VerboseDeserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

					if (ConfigurationAllowsUsingRelativitySync(sourceConfiguration, importSettings))
					{
						_logger.LogInformation(
							"Relativity Sync flow will be used for job with ID: {jobId}. IntegrationPointId: {integrationPointId}",
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

		private T VerboseDeserialize<T>(string jsonToDeserialize)
		{
			_logger.LogDebug($"Deserializing json for type {nameof(T)}");

			if (string.IsNullOrWhiteSpace(jsonToDeserialize))
			{
				throw new ArgumentNullException(nameof(jsonToDeserialize));
			}

			try
			{
				T result = _configurationDeserializer.DeserializeConfiguration<T>(jsonToDeserialize);
				_logger.LogInformation($"Json for type {nameof(T)} deserialized successfully");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Deserializing json for type {nameof(T)} operation resulted in an error.");
				throw;
			}
		}

		private bool ConfigurationAllowsUsingRelativitySync(SourceConfiguration sourceConfiguration, ImportSettings importSettings)
		{
			_logger.LogDebug(
				"Checking if configurations allow using RelativitySync. SourceConfiguration.TypeOfExport: {typeOfExport}; DestinationConfiguration.ImageImport: {imageImport}; DestinationConfiguration.ProductionImport: {productionImport}",
				sourceConfiguration.TypeOfExport, 
				importSettings.ImageImport,
				importSettings.ProductionImport);

			return sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch &&
			       !importSettings.ImageImport &&
			       !importSettings.ProductionImport;
		}

		private ProviderType GetProviderType(int sourceProviderId, int destinationProviderId)
		{
			_logger.LogDebug(
				$"Determining Integration Point provider type based on source and destination provider id's using {nameof(IProviderTypeService)} SourceProviderId: {{sourceProviderId}}; DestinationProviderId: {{destinationProviderId}}",
				sourceProviderId, 
				destinationProviderId);

			try
			{
				ProviderType providerType =
					_providerTypeService.GetProviderType(sourceProviderId, destinationProviderId);
				_logger.LogInformation(
					"ProviderType for given providers has been determined as: {providerType}. SourceProviderId: {sourceProviderId}; DestinationProviderId: {destinationProviderId}",
					providerType, sourceProviderId, destinationProviderId);
				return providerType;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Getting Provider Type operation resulted in an error.");
				throw;
			}
		}

		private IntegrationPoint GetIntegrationPoint(int integrationPointId)
		{
			_logger.LogDebug("Retrieving Integration Point using IntegrationPointService. IntegrationPointId: {integrationPointId}", integrationPointId);

			try
			{
				IntegrationPoint integrationPoint = _integrationPointService.GetRdo(integrationPointId);
				_logger.LogInformation("Integration Point with id: {integrationPointId} retrieved successfully.", integrationPointId);
				return integrationPoint;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Getting Integration Point operation resulted in an error.");
				throw;
			}
		}

		private bool IsRelativitySyncToggleEnabled()
		{
			_logger.LogDebug($"Checking if {nameof(EnableSyncToggle)} is enabled.");

			try
			{
				bool isEnabled = _toggleProvider.IsEnabled<EnableSyncToggle>();
				_logger.LogInformation($"Confirmed that {nameof(EnableSyncToggle)} is {(isEnabled ? "enabled" : "disabled")}.");
				return isEnabled;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Checking if {nameof(EnableSyncToggle)} is enabled resulted in an error.");
				throw;
			}
		}
	}
}