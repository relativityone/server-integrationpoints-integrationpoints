using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Console = System.Console;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler that migrates Integration Points that used Processing Source Location to use Data Transfer Location")]
	[Guid("A48963EA-AB72-4B98-A634-7721B6D2BB9E")]
	[RunOnce(true)]
	public class IntegrationPointDataTransferLocationMigrationEventHandler : PostInstallEventHandler
	{
		private IDataTransferLocationService _dataTransferLocationService;
		private IDestinationProviderRepository _destinationProviderRepository;
		private IIntegrationPointServiceFactory _integrationPointServiceFactory;
		private IIntegrationPointService _integrationPointService;
		private IAPILog _logger;
		private ISourceProviderRepository _sourceProviderRepository;
		private ISerializer _serializer;
		private IResourcePoolManager _resourcePoolManager;
		private IRepositoryFactory _repositoryFactory;
		private IIntegrationPointRepository _integrationPointRepository;
		private IDataTransferLocationMigrationHelper _dataTransferLocationMigrationHelper;

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

		internal IDataTransferLocationService DataTransferLocationService
		{
			get
			{
				if (_dataTransferLocationService == null)
				{
					var context = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
					IIntegrationPointTypeService typeService = new IntegrationPointTypeService(Helper, context);

					_dataTransferLocationService = new DataTransferLocationService(Helper, typeService);
				}

				return _dataTransferLocationService;
			}
		}

		internal IIntegrationPointServiceFactory IntegrationPointServiceFactory
		{
			get
			{
				if (_integrationPointServiceFactory == null)
				{
					RsapiClientFactory rsapiClientFactory = new RsapiClientFactory(Helper);
					IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID(), rsapiClientFactory);
					_integrationPointServiceFactory = new IntegrationPointServiceFactory(Helper.GetActiveCaseID(), Helper,
						serviceContextHelper, Serializer, RepositoryFactory, rsapiClientFactory);
				}

				return _integrationPointServiceFactory;
			}
		}

		internal IIntegrationPointService IntegrationPointService
		{
			get
			{
				if (_integrationPointService == null)
				{
					_integrationPointService = IntegrationPointServiceFactory.Create();
				}

				return _integrationPointService;
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

		internal ISerializer Serializer
		{
			get
			{
				if (_serializer == null)
				{
					_serializer = new JSONSerializer();
				}

				return _serializer;
			}
		}

		internal IResourcePoolManager ResourcePoolManager
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

		internal IIntegrationPointRepository IntegrationPointRepository
		{
			get
			{
				if (_integrationPointRepository == null)
				{
					_integrationPointRepository = RepositoryFactory.GetIntegrationPointRepository(Helper.GetActiveCaseID());
				}

				return _integrationPointRepository;
			}
		}

		internal IDataTransferLocationMigrationHelper DataTransferLocationMigrationHelper
		{
			get
			{
				if (_dataTransferLocationMigrationHelper == null)
				{
					_dataTransferLocationMigrationHelper = new DataTransferLocationMigrationHelper(Helper.GetActiveCaseID(),
						DataTransferLocationService, ResourcePoolManager, Serializer);
				}

				return _dataTransferLocationMigrationHelper;
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

		private void MigrateDestinationLocationPaths(IList<Data.IntegrationPoint> integrationPoints)
		{
			foreach (var integrationPoint in integrationPoints)
			{
				var updatedSourceConfigurationString = DataTransferLocationMigrationHelper.GetUpdatedSourceConfiguration(integrationPoint);

				IntegrationPointModel model = IntegrationPointModel.FromIntegrationPoint(integrationPoint);
				model.SourceConfiguration = updatedSourceConfigurationString;

				IntegrationPointService.SaveIntegration(model);
			}
		}

		private int GetRelativitySourceProviderArtifactId()
		{
			try
			{
				return SourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(Constants.IntegrationPoints.SourceProviders.RELATIVITY);
			}
			catch (Exception)
			{
				Logger.LogError("Failed to retrieve Relativity Source Provider ArtifactId");
				throw;
			}
		}

		private int GetLoadFileDestinationProviderArtifactId()
		{
			try
			{
				return DestinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(Constants.IntegrationPoints.DestinationProviders.LOADFILE);
			}
			catch (Exception)
			{
				Logger.LogError("Failed to retrieve LoadFile Destination Provider ArtifactId");
				throw;
			}
		}

		private IList<Data.IntegrationPoint> GetAllExportIntegrationPoints(int relativitySourceProviderArtifactId, int loadFileDestinationProviderArtifactId)
		{
			try
			{
				IList<Data.IntegrationPoint> integrationPoints = IntegrationPointService.GetAllRDOs();

				if (!integrationPoints.Any())
				{
					return new List<Data.IntegrationPoint>();
				}

				return integrationPoints.Where(ip =>
						ip.SourceProvider.HasValue && ip.SourceProvider.Value == relativitySourceProviderArtifactId &&
						ip.DestinationProvider.HasValue && ip.DestinationProvider.Value == loadFileDestinationProviderArtifactId)
					.ToList();
			}
			catch (Exception)
			{
				Logger.LogError("Failed to retrieve Integration Points data");
				throw;
			}
		}
	}
}