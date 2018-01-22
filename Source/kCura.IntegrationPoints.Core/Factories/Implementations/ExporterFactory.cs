using System.Collections.Generic;
using System.Security.Claims;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
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
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;
		private readonly IRepositoryFactory _sourceRepositoryFactory;
		private readonly IRepositoryFactory _targetRepositoryFactory;
		private readonly IHelper _helper;
		private readonly IFederatedInstanceManager _federatedInstanceManager;
		private readonly IFolderPathReaderFactory _folderPathReaderFactory;
		private readonly IToggleProvider _toggleProvider;

		public ExporterFactory(
			IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IHelper helper, IFederatedInstanceManager federatedInstanceManager,
			IFolderPathReaderFactory folderPathReaderFactory,
			IToggleProvider toggleProvider)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
			_sourceRepositoryFactory = sourceRepositoryFactory;
			_targetRepositoryFactory = targetRepositoryFactory;
			_helper = helper;
			_federatedInstanceManager = federatedInstanceManager;
			_folderPathReaderFactory = folderPathReaderFactory;
			_toggleProvider = toggleProvider;
		}

		public List<IBatchStatus> InitializeExportServiceJobObservers(Job job,
			ITagsCreator tagsCreator,
			ITagSavedSearchManager tagSavedSearchManager,
			ISynchronizerFactory synchronizerFactory,
			ISerializer serializer,
			IJobHistoryErrorManager jobHistoryErrorManager,
			IJobStopManager jobStopManager,
			FieldMap[] mappedFiles,
			SourceConfiguration configuration,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			IntegrationPoint integrationPoint,
			JobHistory jobHistory,
			string uniqueJobId,
			string userImportApiSettings)
		{
			IDocumentRepository documentRepository = _sourceRepositoryFactory.GetDocumentRepository(configuration.SourceWorkspaceArtifactId);

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(_sourceRepositoryFactory, tagsCreator, tagSavedSearchManager, documentRepository,
				synchronizerFactory, _helper, serializer, mappedFiles, integrationPoint.SourceConfiguration, userImportApiSettings, jobHistory.ArtifactId, uniqueJobId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			IConsumeScratchTableBatchStatus sourceFieldsTagger = new SourceObjectBatchUpdateManager(_sourceRepositoryFactory, _targetRepositoryFactory, _claimsPrincipalFactory, _helper,
				_federatedInstanceManager, configuration, jobHistory.ArtifactId, job.SubmittedBy, uniqueJobId);
			IBatchStatus sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(jobHistoryErrorManager, _sourceRepositoryFactory, _claimsPrincipalFactory, jobStopManager,
				configuration.SourceWorkspaceArtifactId, job.SubmittedBy, updateStatusType);

			var batchStatusCommands = new List<IBatchStatus>
			{
				destinationFieldsTagger,
				sourceFieldsTagger,
				sourceJobHistoryErrorUpdater
			};
			return batchStatusCommands;
		}

		public IExporterService BuildExporter(IJobStopManager jobStopManager, FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser,
			string userImportApiSettings)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);

			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(userImportApiSettings);
			SourceConfiguration sourceConfiguration = JsonConvert.DeserializeObject<SourceConfiguration>(config);
			BaseServiceContext baseServiceContext = claimsPrincipal.GetUnversionContext(sourceConfiguration.SourceWorkspaceArtifactId);
			IExporterService exporterService;
			if (settings.ImageImport)
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

				exporterService = new ImageExporterService(exporter, _sourceRepositoryFactory, _targetRepositoryFactory, jobStopManager, _helper,
					claimsPrincipal, mappedFiles, 0, config, searchArtifactId, settings);
			}
			else
			{
				IExporter exporter = BuildSavedSearchExporter(baseServiceContext, settings.LoadImportedFullTextFromServer);

				IFolderPathReader folderPathReader = _folderPathReaderFactory.Create(claimsPrincipal, settings, config);
                exporterService = new RelativityExporterService(exporter, _sourceRepositoryFactory, _targetRepositoryFactory, jobStopManager, _helper,
                    folderPathReader, _toggleProvider, claimsPrincipal, mappedFiles, 0, config, savedSearchArtifactId);
            }

			return exporterService;
		}

		private IExporter BuildSavedSearchExporter(BaseServiceContext baseService, bool shouldUseDgPaths)
		{
			return new SavedSearchExporter
				(
				baseService,
				new UserPermissionsMatrix(baseService),
				global::Relativity.ArtifactType.Document,
				IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER,
				IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER,
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
				IntegrationPoints.Domain.Constants.MULTI_VALUE_DELIMITER,
				IntegrationPoints.Domain.Constants.NESTED_VALUE_DELIMITER,
				global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths,
				shouldUseDgPaths
				);
		}
	}
}