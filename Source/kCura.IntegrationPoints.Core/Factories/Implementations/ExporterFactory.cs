using System.Collections.Generic;
using System.Security.Claims;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Core.Services.Exporter.Images;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Api.Shared.Manager.Export;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private const int _ADMIN_USER_ID = 9;
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;
		private readonly IRepositoryFactory _sourceRepositoryFactory;
		private readonly IRepositoryFactory _targetRepositoryFactory;
		private readonly IHelper _helper;
		private readonly IFederatedInstanceManager _federatedInstanceManager;
		private readonly IFolderPathReaderFactory _folderPathReaderFactory;
		private readonly IToggleProvider _toggleProvider;
		private readonly IAPILog _logger;

		public ExporterFactory(
			IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IHelper helper, IFederatedInstanceManager federatedInstanceManager,
			IFolderPathReaderFactory folderPathReaderFactory,
			IToggleProvider toggleProvider
			)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
			_sourceRepositoryFactory = sourceRepositoryFactory;
			_targetRepositoryFactory = targetRepositoryFactory;
			_helper = helper;
			_federatedInstanceManager = federatedInstanceManager;
			_folderPathReaderFactory = folderPathReaderFactory;
			_toggleProvider = toggleProvider;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<ExporterFactory>();
		}

		public List<IBatchStatus> InitializeExportServiceJobObservers(Job job,
			ITagsCreator tagsCreator,
			ITagSavedSearchManager tagSavedSearchManager,
			ISynchronizerFactory synchronizerFactory,
			ISerializer serializer,
			IJobHistoryErrorManager jobHistoryErrorManager,
			IJobStopManager jobStopManager,
			ISourceWorkspaceTagCreator sourceWorkspaceTagsCreator,
			FieldMap[] mappedFields,
			SourceConfiguration configuration,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			IntegrationPoint integrationPoint,
			JobHistory jobHistory,
			string uniqueJobId,
			string userImportApiSettings)
		{
			IConsumeScratchTableBatchStatus destinationFieldsTagger = CreateDestinationFieldsTagger(tagsCreator, tagSavedSearchManager, synchronizerFactory,
				serializer, mappedFields, configuration, integrationPoint, jobHistory, uniqueJobId, userImportApiSettings);
			IConsumeScratchTableBatchStatus sourceFieldsTagger = CreateSourceFieldsTagger(job, configuration, jobHistory, sourceWorkspaceTagsCreator, uniqueJobId);
			IBatchStatus sourceJobHistoryErrorUpdater = CreateJobHistoryErrorUpdater(job, jobHistoryErrorManager, jobStopManager, configuration, updateStatusType);

			var batchStatusCommands = new List<IBatchStatus>
			{
				destinationFieldsTagger,
				sourceFieldsTagger,
				sourceJobHistoryErrorUpdater
			};
			return batchStatusCommands;
		}

		public IExporterService BuildExporter(IJobStopManager jobStopManager, FieldMap[] mappedFields, string config, int savedSearchArtifactId, int onBehalfOfUser,
			string userImportApiSettings)
        {
            LogBuildExporterExecutionWithParameters(mappedFields, config, savedSearchArtifactId, onBehalfOfUser, userImportApiSettings);
            ClaimsPrincipal claimsPrincipal = GetClaimsPrincipal(onBehalfOfUser);
            IBaseServiceContextProvider baseServiceContextProvider = new BaseServiceContextProvider(claimsPrincipal);

            ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(userImportApiSettings);
            SourceConfiguration sourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(config);
            BaseServiceContext baseServiceContext = claimsPrincipal.GetUnversionContext(sourceConfiguration.SourceWorkspaceArtifactId);

            IExporterService exporter = settings.ImageImport ?
                CreateImageExporterService(jobStopManager, mappedFields, config, savedSearchArtifactId, baseServiceContextProvider, settings, sourceConfiguration, baseServiceContext) :
                CreateRelativityExporterService(jobStopManager, mappedFields, config, savedSearchArtifactId, claimsPrincipal, baseServiceContextProvider, settings, baseServiceContext);
            return exporter;
        }

	    private IConsumeScratchTableBatchStatus CreateDestinationFieldsTagger(ITagsCreator tagsCreator, ITagSavedSearchManager tagSavedSearchManager, 
			ISynchronizerFactory synchronizerFactory, ISerializer serializer, FieldMap[] mappedFields, SourceConfiguration configuration, 
			IntegrationPoint integrationPoint, JobHistory jobHistory, string uniqueJobId, string userImportApiSettings)
		{
			IDocumentRepository documentRepository = _sourceRepositoryFactory.GetDocumentRepository(configuration.SourceWorkspaceArtifactId);

			var taggerFactory = new TargetDocumentsTaggingManagerFactory(_sourceRepositoryFactory, tagsCreator, tagSavedSearchManager, documentRepository,
				synchronizerFactory, _helper, serializer, mappedFields, integrationPoint.SourceConfiguration, userImportApiSettings, jobHistory.ArtifactId, uniqueJobId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			return destinationFieldsTagger;
		}

	    private IConsumeScratchTableBatchStatus CreateSourceFieldsTagger(Job job, SourceConfiguration configuration, JobHistory jobHistory, ISourceWorkspaceTagCreator sourceWorkspaceTagsCreator,
		    string uniqueJobId)
		{
			return new SourceObjectBatchUpdateManager(_sourceRepositoryFactory, _targetRepositoryFactory, _claimsPrincipalFactory, _helper,
				_federatedInstanceManager, sourceWorkspaceTagsCreator, configuration, jobHistory.ArtifactId, job.SubmittedBy, uniqueJobId);
		}

	    private IBatchStatus CreateJobHistoryErrorUpdater(Job job, IJobHistoryErrorManager jobHistoryErrorManager, IJobStopManager jobStopManager, 
			SourceConfiguration configuration, JobHistoryErrorDTO.UpdateStatusType updateStatusType)
		{
			return new JobHistoryErrorBatchUpdateManager(jobHistoryErrorManager, _helper, _sourceRepositoryFactory, _claimsPrincipalFactory, jobStopManager,
				configuration.SourceWorkspaceArtifactId, job.SubmittedBy, updateStatusType);
		}

	    private IExporterService CreateRelativityExporterService(IJobStopManager jobStopManager, FieldMap[] mappedFields, string config, int savedSearchArtifactId,
			ClaimsPrincipal claimsPrincipal, IBaseServiceContextProvider baseServiceContextProvider, ImportSettings settings, BaseServiceContext baseServiceContext)
		{
			IExporter exporter = BuildSavedSearchExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);

			IFolderPathReader folderPathReader = _folderPathReaderFactory.Create(claimsPrincipal, settings, config);
			var exporterService = new RelativityExporterService(exporter, _sourceRepositoryFactory, _targetRepositoryFactory, jobStopManager, _helper,
				folderPathReader, _toggleProvider, baseServiceContextProvider, mappedFields, 0, config, savedSearchArtifactId);
			return exporterService;
		}

	    private IExporterService CreateImageExporterService(IJobStopManager jobStopManager, FieldMap[] mappedFiles, string config, int savedSearchArtifactId,
			IBaseServiceContextProvider baseServiceContextProvider, ImportSettings settings, SourceConfiguration sourceConfiguration, BaseServiceContext baseServiceContext)
		{
			IExporter exporter;
			int searchArtifactId;
			if (sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
			{
				exporter = BuildSavedSearchExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);
				searchArtifactId = savedSearchArtifactId;
			}
			else
			{
				exporter = BuildProductionExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);
				searchArtifactId = sourceConfiguration.SourceProductionId;
			}

			var exporterService = new ImageExporterService(exporter, _sourceRepositoryFactory, _targetRepositoryFactory, jobStopManager, _helper,
				baseServiceContextProvider, mappedFiles, 0, config, searchArtifactId, settings);
			return exporterService;
		}

	    private ClaimsPrincipal GetClaimsPrincipal(int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = _ADMIN_USER_ID;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);
			return claimsPrincipal;
		}

	    private IExporter BuildSavedSearchExporter(BaseServiceContext baseService, bool shouldUseDgPaths)
		{
			return new SavedSearchExporter
				(
				baseService,
				new UserPermissionsMatrix(baseService),
				global::Relativity.ArtifactType.Document,
				Domain.Constants.MULTI_VALUE_DELIMITER,
				Domain.Constants.NESTED_VALUE_DELIMITER,
				global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths,
				shouldUseDgPaths
				);
		}

	    private IExporter BuildProductionExporter(BaseServiceContext baseService, bool shouldUseDgPaths)
		{
			return new ProductionExporter
				(
				baseService,
				new UserPermissionsMatrix(baseService),
				global::Relativity.ArtifactType.Document,
				Domain.Constants.MULTI_VALUE_DELIMITER,
				Domain.Constants.NESTED_VALUE_DELIMITER,
				global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths,
				shouldUseDgPaths
				);
		}

	    private void LogBuildExporterExecutionWithParameters(FieldMap[] mappedFields, string config, int savedSearchArtifactId, int onBehalfOfUser, string userImportApiSettings)
	    {
	        _logger.LogDebug("Building Exporter with parameters: \nmappedFields {@mappedFields} \nconfig {config} \nsavedSearchArtifactId {savedSearchArtifactId} \nonBehalfOfUser: {onBehalfOfUser} \nuserImportApiSettings {userImportApiSettings}",
                mappedFields,config, savedSearchArtifactId, onBehalfOfUser, userImportApiSettings);
	    }
	}
}