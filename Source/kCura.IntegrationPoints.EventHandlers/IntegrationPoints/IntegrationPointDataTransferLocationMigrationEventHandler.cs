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
				var updatedSourceConfigurationString = _dataTransferLocationMigrationHelper.GetUpdatedSourceConfiguration(integrationPoint);

				IntegrationPointModel model = IntegrationPointModel.FromIntegrationPoint(integrationPoint);
				model.SourceConfiguration = updatedSourceConfigurationString;

				//IntegrationPointService.SaveIntegration(model);
			}
		}

		//private string GetUpdatedSourceConfiguration(Data.IntegrationPoint integrationPoint)
		//{
		//	Dictionary<string, object> sourceConfiguration = DeserializeSourceConfigurationString(integrationPoint.SourceConfiguration);
		//	UpdateDataTransferLocation(sourceConfiguration);

		//	return SerializeSourceConfiguration(sourceConfiguration);
		//}

		//private Dictionary<string, object> DeserializeSourceConfigurationString(string sourceConfiguration)
		//{
		//	return Serializer.Deserialize<Dictionary<string, object>>(sourceConfiguration);
		//}

		//private string SerializeSourceConfiguration(Dictionary<string, object> sourceConfiguration)
		//{
		//	return Serializer.Serialize(sourceConfiguration);
		//}

		//private void UpdateDataTransferLocation(Dictionary<string, object> sourceConfiguration)
		//{
		//	string currentPath = sourceConfiguration[SOURCECONFIGURATION_FILESHARE_KEY] as string; ;
		//	IList<string> processingSourceLocations = GetProcessingSourceLocationsForCurrentWorkspace();
		//	string exportDestinationFolder = ExtractExportDestinationFolder(processingSourceLocations, currentPath);
		//	string newDataTransferLocationRoot = DataTransferLocationService.GetDefaultRelativeLocationFor(Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
		//	string newPath = Path.Combine(newDataTransferLocationRoot, exportDestinationFolder);

		//	sourceConfiguration[SOURCECONFIGURATION_FILESHARE_KEY] = newPath;
		//}

		//private IList<string> GetProcessingSourceLocationsForCurrentWorkspace()
		//{
		//	IList<ProcessingSourceLocationDTO> processingSourceLocationDtos = ResourcePoolManager.GetProcessingSourceLocation(Helper.GetActiveCaseID());
		//	return processingSourceLocationDtos.Select(x => x.Location).ToList();
		//}

		//private string ExtractExportDestinationFolder(IList<string> processingSourceLocations, string currentExportLocation)
		//{
		//	foreach (var processingSourceLocation in processingSourceLocations)
		//	{
		//		if (processingSourceLocation == currentExportLocation)
		//		{
		//			//This means that previous Export was done to root of Processing Source Location therefore new destination folder is also root of new Data Transfer Location
		//			return string.Empty;
		//		}

		//		int startIndex = currentExportLocation.IndexOf(processingSourceLocation, StringComparison.Ordinal);

		//		if (startIndex == -1)
		//		{
		//			//ProcessingSourceLocation not found within currentExportLocation
		//			continue;
		//		}

		//		int exportLocationStartIndex = startIndex + processingSourceLocation.Length + 1;
		//		int length = currentExportLocation.Length - processingSourceLocation.Length;
		//		string exportLocation = currentExportLocation.Substring(exportLocationStartIndex, length);

		//		return exportLocation;
		//	}

		//	return string.Empty;
		//}

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
			IList<Data.IntegrationPoint> integrationPoints = IntegrationPointService.GetAllRDOs();

			return integrationPoints.Where(ip =>
						ip.SourceProvider.HasValue && ip.SourceProvider.Value == relativitySourceProviderArtifactId &&
						ip.DestinationProvider.HasValue && ip.DestinationProvider.Value == loadFileDestinationProviderArtifactId)
						.ToList();
		}

		//private IIntegrationPointService CreateIntegrationPointService()
		//{
		//	RsapiClientFactory rsapiClientFactory = new RsapiClientFactory(Helper);
		//	IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID(), rsapiClientFactory);
		//	ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
		//	IWorkspaceRepository workspaceRepository = RepositoryFactory.GetWorkspaceRepository();
		//	IRSAPIClient rsapiClient = rsapiClientFactory.CreateClientForWorkspace(Helper.GetActiveCaseID(), ExecutionIdentity.System);
		//	IChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);
		//	IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
		//	IAgentService agentService = new AgentService(Helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
		//	IJobService jobService = new JobService(agentService, Helper);
		//	IDBContext dbContext = Helper.GetDBContext(Helper.GetActiveCaseID());
		//	IWorkspaceDBContext workspaceDbContext = new WorkspaceContext(dbContext);
		//	JobResourceTracker jobResourceTracker = new JobResourceTracker(RepositoryFactory, workspaceDbContext);
		//	JobTracker jobTracker = new JobTracker(jobResourceTracker);
		//	IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, Helper, Serializer, jobTracker);
		//	IJobHistoryService jobHistoryService = new JobHistoryService(caseServiceContext, workspaceRepository, Helper, Serializer);
		//	IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
		//	IManagerFactory managerFactory = new ManagerFactory(Helper);

		//	IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), Serializer);
		//	IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), Serializer);

		//	return new IntegrationPointService(Helper, caseServiceContext, contextContainerFactory, Serializer,
		//		choiceQuery, jobManager, jobHistoryService, managerFactory, ipValidator, permissionValidator);
		//}
	}
}