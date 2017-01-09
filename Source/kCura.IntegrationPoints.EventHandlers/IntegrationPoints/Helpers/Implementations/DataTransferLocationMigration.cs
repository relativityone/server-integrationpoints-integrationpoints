using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class DataTransferLocationMigration : IDataTransferLocationMigration
	{
		private readonly IAPILog _logger;
		private readonly IDestinationProviderRepository _destinationProviderRepository;
		private readonly ISourceProviderRepository _sourceProviderRepository;
		private readonly IDataTransferLocationMigrationHelper _dataTransferLocationMigrationHelper;
		private readonly IGenericLibrary<Data.IntegrationPoint> _integrationPointLibrary;
		private readonly IDataTransferLocationService _dataTransferLocationService;
		private readonly IResourcePoolManager _resourcePoolManager;
		private readonly IEHHelper _helper;

		public DataTransferLocationMigration(IAPILog logger, IDestinationProviderRepository destinationProviderRepository, ISourceProviderRepository sourceProviderRepository, IDataTransferLocationMigrationHelper dataTransferLocationMigrationHelper, ICaseServiceContext serviceContext, IGenericLibrary<Data.IntegrationPoint> integrationPointLibrary, IDataTransferLocationService dataTransferLocationService, IResourcePoolManager resourcePoolManager, IEHHelper helper)
		{
			_logger = logger;
			_destinationProviderRepository = destinationProviderRepository;
			_sourceProviderRepository = sourceProviderRepository;
			_dataTransferLocationMigrationHelper = dataTransferLocationMigrationHelper;
			_integrationPointLibrary = integrationPointLibrary;
			_dataTransferLocationService = dataTransferLocationService;
			_resourcePoolManager = resourcePoolManager;
			_helper = helper;
		}

		public void Migrate()
		{
			int sourceProviderArtifactId = GetRelativitySourceProviderArtifactId();
			int destinationProviderArtifactId = GetLoadFileDestinationProviderArtifactId();

			IList<Data.IntegrationPoint> integrationPoints = GetAllExportIntegrationPoints(sourceProviderArtifactId,
				destinationProviderArtifactId);

			MigrateDestinationLocationPaths(integrationPoints);
		}

		private int GetRelativitySourceProviderArtifactId()
		{
			try
			{
				return _sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(Constants.IntegrationPoints.SourceProviders.RELATIVITY);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve Relativity Source Provider ArtifactId");
				throw;
			}
		}

		private int GetLoadFileDestinationProviderArtifactId()
		{
			try
			{
				return _destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(Constants.IntegrationPoints.DestinationProviders.LOADFILE);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve LoadFile Destination Provider ArtifactId");
				throw;
			}
		}

		private IList<Data.IntegrationPoint> GetAllExportIntegrationPoints(int relativitySourceProviderArtifactId, int loadFileDestinationProviderArtifactId)
		{
			try
			{
				Query<RDO> query = BuildIntegrationPointsQuery(relativitySourceProviderArtifactId, loadFileDestinationProviderArtifactId);
				return _integrationPointLibrary.Query(query);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve Integration Points data");
				throw;
			}
		}

		private void MigrateDestinationLocationPaths(IList<Data.IntegrationPoint> integrationPoints)
		{
			if (!integrationPoints.Any())
			{
				return;
			}

			IList<string> processingSourceLocations = GetProcessingSourceLocationsForCurrentWorkspace();
			string newDataTransferLocationRoot = GetNewDataTransferLocationRoot();

			foreach (var integrationPoint in integrationPoints)
			{
				UpdateIntegrationPoint(integrationPoint, processingSourceLocations, newDataTransferLocationRoot);
			}
		}

		private void UpdateIntegrationPoint(Data.IntegrationPoint integrationPoint, IList<string> processingSourceLocations,
			string newDataTransferLocationRoot)
		{
			try
			{
				string updatedSourceConfigurationString =
					_dataTransferLocationMigrationHelper.GetUpdatedSourceConfiguration(integrationPoint.SourceConfiguration,
						processingSourceLocations, newDataTransferLocationRoot);
				integrationPoint.SourceConfiguration = updatedSourceConfigurationString;

				_integrationPointLibrary.Update(integrationPoint);
			}
			catch (Exception ex)
			{
				var errorMessage = $"Failed to migrate Integration Point: {integrationPoint.Name} with ArtifactId: {integrationPoint.ArtifactId}";
				_logger.LogError(ex, errorMessage);
				throw new InvalidOperationException(errorMessage, ex);
			}
		}

		private IList<string> GetProcessingSourceLocationsForCurrentWorkspace()
		{
			try
			{
				IList<ProcessingSourceLocationDTO> processingSourceLocationDtos = _resourcePoolManager.GetProcessingSourceLocation(_helper.GetActiveCaseID());
				return processingSourceLocationDtos.Select(x => x.Location).ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve Processing Source Locations for Workspace: {workspaceId}", _helper.GetActiveCaseID());
				throw;
			}
		}

		private string GetNewDataTransferLocationRoot()
		{
			try
			{
				return _dataTransferLocationService.GetDefaultRelativeLocationFor(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve default relative location for integration point");
				throw;
			}
		}

		private Query<RDO> BuildIntegrationPointsQuery(int relativitySourceProviderArtifactId,
			int loadFileDestinationProviderArtifactId)
		{
			var sourceProviderCondition = new WholeNumberCondition()
			{
				Field = IntegrationPointFields.SourceProvider,
				Operator = NumericConditionEnum.EqualTo,
				Value = new List<int>() { relativitySourceProviderArtifactId }
			};

			var destinationProviderCondition = new WholeNumberCondition()
			{
				Field = IntegrationPointFields.DestinationProvider,
				Operator = NumericConditionEnum.EqualTo,
				Value = new List<int>() { loadFileDestinationProviderArtifactId }
			};

			Query<RDO> query = new Query<RDO>()
			{
				Fields =
					BaseRdo.GetFieldMetadata(typeof(Data.IntegrationPoint))
						.Values.ToList()
						.Select(field => new FieldValue(field.FieldGuid))
						.ToList(),
				Condition = new CompositeCondition(sourceProviderCondition, CompositeConditionEnum.And, destinationProviderCondition)
			};
			return query;
		}
	}
}