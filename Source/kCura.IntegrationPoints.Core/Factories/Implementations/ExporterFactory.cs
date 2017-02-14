using System.Collections.Generic;
using System.Security.Claims;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;
		private readonly IRepositoryFactory _sourceRepositoryFactory;
		private readonly IRepositoryFactory _targetRepositoryFactory;
		private readonly IHelper _helper;

		public ExporterFactory(
			IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory,
			IRepositoryFactory sourceRepositoryFactory,
			IRepositoryFactory targetRepositoryFactory,
			IHelper helper)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
			_sourceRepositoryFactory = sourceRepositoryFactory;
			_targetRepositoryFactory = targetRepositoryFactory;
			_helper = helper;
		}

		public List<IBatchStatus> InitializeExportServiceJobObservers(Job job,
			ISourceWorkspaceManager sourceWorkspaceManager,
			ISourceJobManager sourceJobManager,
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

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(_sourceRepositoryFactory, sourceWorkspaceManager,
				sourceJobManager, documentRepository, synchronizerFactory, _helper, serializer, mappedFiles, integrationPoint.SourceConfiguration,
				userImportApiSettings, jobHistory.ArtifactId, uniqueJobId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			IConsumeScratchTableBatchStatus sourceFieldsTagger = new SourceObjectBatchUpdateManager(_sourceRepositoryFactory, _targetRepositoryFactory, _claimsPrincipalFactory, _helper, configuration, jobHistory.ArtifactId, job.SubmittedBy, uniqueJobId);
			IBatchStatus sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(jobHistoryErrorManager, _sourceRepositoryFactory, _claimsPrincipalFactory, jobStopManager, configuration.SourceWorkspaceArtifactId, job.SubmittedBy, updateStatusType);

			var batchStatusCommands = new List<IBatchStatus>()
			{
				destinationFieldsTagger,
				sourceFieldsTagger,
				sourceJobHistoryErrorUpdater
			};
			return batchStatusCommands;
		}

		public IExporterService BuildExporter(IJobStopManager jobStopManager, FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser, string userImportApiSettings)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);

			ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(userImportApiSettings);


			if (settings.ImageImport)
			{
				return new ImageExporterService(_sourceRepositoryFactory, _targetRepositoryFactory, jobStopManager, _helper,
					claimsPrincipal, mappedFiles, 0, config, savedSearchArtifactId);
			}
			else
			{
				return new RelativityExporterService(_sourceRepositoryFactory, _targetRepositoryFactory, jobStopManager, _helper, claimsPrincipal, mappedFiles, 0, config, savedSearchArtifactId);
			}
		}
	}
}