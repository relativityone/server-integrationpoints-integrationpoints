using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;
using FieldValue = kCura.Relativity.Client.DTOs.FieldValue;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler that migrates Integration Points that used Processing Source Location to use Data Transfer Location")]
	[Guid("A48963EA-AB72-4B98-A634-7721B6D2BB9E")]
	[RunOnce(true)]
	public class IntegrationPointDataTransferLocationMigrationEventHandler : PostInstallEventHandler
	{
		private IAPILog _logger;
		private IDestinationProviderRepository _destinationProviderRepository;
		private ISourceProviderRepository _sourceProviderRepository;
		private IRepositoryFactory _repositoryFactory;
		private IDataTransferLocationMigrationHelper _dataTransferLocationMigrationHelper;
		private ICaseServiceContext _serviceContext;
		private IGenericLibrary<Data.IntegrationPoint> _integrationPointLibrary;
		private IDataTransferLocationService _dataTransferLocationService;
		private IResourcePoolManager _resourcePoolManager;

		internal IAPILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = Helper.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationMigrationEventHandler>();
				}

				return _logger;
			}
		}

		internal IRepositoryFactory RepositoryFactory
		{
			get
			{
				if (_repositoryFactory == null)
				{
					_repositoryFactory = new RepositoryFactory(Helper);
				}

				return _repositoryFactory;
			}
		}

		internal IDestinationProviderRepository DestinationProviderRepository
		{
			get
			{
				if (_destinationProviderRepository == null)
				{
					_destinationProviderRepository = RepositoryFactory.GetDestinationProviderRepository(Helper.GetActiveCaseID());
				}

				return _destinationProviderRepository;
			}
		}

		internal ISourceProviderRepository SourceProviderRepository
		{
			get
			{
				if (_sourceProviderRepository == null)
				{
					_sourceProviderRepository = RepositoryFactory.GetSourceProviderRepository(Helper.GetActiveCaseID());
				}

				return _sourceProviderRepository;
			}
		}

		internal IDataTransferLocationMigrationHelper DataTransferLocationMigrationHelper
		{
			get
			{
				if (_dataTransferLocationMigrationHelper == null)
				{
					ISerializer serializer = new JSONSerializer();
					_dataTransferLocationMigrationHelper = new DataTransferLocationMigrationHelper(serializer);
				}

				return _dataTransferLocationMigrationHelper;
			}
		}

		internal ICaseServiceContext CaseServiceContext
		{
			get
			{
				if (_serviceContext == null)
				{
					_serviceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
				}

				return _serviceContext;
			}
		}

		internal IGenericLibrary<Data.IntegrationPoint> IntegrationPointLibrary
		{
			get
			{
				if (_integrationPointLibrary == null)
				{
					_integrationPointLibrary = CaseServiceContext.RsapiService.GetGenericLibrary<Data.IntegrationPoint>();
				}

				return _integrationPointLibrary;
			}
		}

		private IDataTransferLocationService DataTransferLocationService
		{
			get
			{
				if (_dataTransferLocationService == null)
				{
					IIntegrationPointTypeService typeService = new IntegrationPointTypeService(Helper, CaseServiceContext);
					_dataTransferLocationService = new DataTransferLocationService(Helper, typeService);
				}

				return _dataTransferLocationService;
			}
		}

		private IResourcePoolManager ResourcePoolManager
		{
			get
			{
				if (_resourcePoolManager == null)
				{
					_resourcePoolManager = new ResourcePoolManager(RepositoryFactory, Helper);
				}

				return _resourcePoolManager;
			}
		}

		public override Response Execute()
		{
			try
			{
				int sourceProviderArtifactId = GetRelativitySourceProviderArtifactId();
				int destinationProviderArtifactId = GetLoadFileDestinationProviderArtifactId();

				IList<Data.IntegrationPoint> integrationPoints = GetAllExportIntegrationPoints(sourceProviderArtifactId,
					destinationProviderArtifactId);

				MigrateDestinationLocationPaths(integrationPoints);

				return new Response
				{
					Message = "Integration Points Data Transfer Location migrated successfully",
					Success = true
				};
			}
			catch (Exception e)
			{
				return new Response()
				{
					Exception = e,
					Message = e.Message,
					Success = false
				};
			}
		}

		private int GetRelativitySourceProviderArtifactId()
		{
			try
			{
				return SourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(Constants.IntegrationPoints.SourceProviders.RELATIVITY);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Failed to retrieve Relativity Source Provider ArtifactId");
				throw;
			}
		}

		private int GetLoadFileDestinationProviderArtifactId()
		{
			try
			{
				return DestinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(Constants.IntegrationPoints.DestinationProviders.LOADFILE);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Failed to retrieve LoadFile Destination Provider ArtifactId");
				throw;
			}
		}

		private IList<Data.IntegrationPoint> GetAllExportIntegrationPoints(int relativitySourceProviderArtifactId, int loadFileDestinationProviderArtifactId)
		{
			try
			{
				Query<RDO> query = BuildIntegrationPointsQuery(relativitySourceProviderArtifactId, loadFileDestinationProviderArtifactId);
				return IntegrationPointLibrary.Query(query);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Failed to retrieve Integration Points data");
				throw;
			}
		}

		private void MigrateDestinationLocationPaths(IList<Data.IntegrationPoint> integrationPoints)
		{
			IList<string> processingSourceLocations = GetProcessingSourceLocationsForCurrentWorkspace();
			string newDataTransferLocationRoot = GetNewDataTransferLocationRoot();

			foreach (var integrationPoint in integrationPoints)
			{
				var updatedSourceConfigurationString = DataTransferLocationMigrationHelper.GetUpdatedSourceConfiguration(integrationPoint, processingSourceLocations, newDataTransferLocationRoot);
				integrationPoint.SourceConfiguration = updatedSourceConfigurationString;

				IntegrationPointLibrary.Update(integrationPoint);
			}
		}

		private IList<string> GetProcessingSourceLocationsForCurrentWorkspace()
		{
			IList<ProcessingSourceLocationDTO> processingSourceLocationDtos = ResourcePoolManager.GetProcessingSourceLocation(Helper.GetActiveCaseID());
			return processingSourceLocationDtos.Select(x => x.Location).ToList();
		}

		private string GetNewDataTransferLocationRoot()
		{
			return DataTransferLocationService.GetDefaultRelativeLocationFor(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
		}

		private Query<RDO> BuildIntegrationPointsQuery(int relativitySourceProviderArtifactId,
			int loadFileDestinationProviderArtifactId)
		{
			var sourceProviderCondition = new WholeNumberCondition()
			{
				Field = IntegrationPointFields.SourceProvider,
				Operator = NumericConditionEnum.EqualTo,
				Value = new List<int>() {relativitySourceProviderArtifactId}
			};

			var destinationProviderCondition = new WholeNumberCondition()
			{
				Field = IntegrationPointFields.DestinationProvider,
				Operator = NumericConditionEnum.EqualTo,
				Value = new List<int>() {loadFileDestinationProviderArtifactId}
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