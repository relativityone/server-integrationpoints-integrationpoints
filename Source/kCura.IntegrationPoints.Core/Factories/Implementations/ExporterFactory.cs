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
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IHelper _helper;

		public ExporterFactory(IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory, IRepositoryFactory repositoryFactory, IHelper helper)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
			_repositoryFactory = repositoryFactory;
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
			IDocumentRepository documentRepository = _repositoryFactory.GetDocumentRepository(configuration.SourceWorkspaceArtifactId);

			TargetDocumentsTaggingManagerFactory taggerFactory = new TargetDocumentsTaggingManagerFactory(_repositoryFactory, sourceWorkspaceManager,
				sourceJobManager, documentRepository, synchronizerFactory, _helper, serializer, mappedFiles, integrationPoint.SourceConfiguration,
				userImportApiSettings, jobHistory.ArtifactId, uniqueJobId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			IConsumeScratchTableBatchStatus sourceFieldsTagger = new SourceObjectBatchUpdateManager(_repositoryFactory, _claimsPrincipalFactory, _helper, configuration, jobHistory.ArtifactId, job.SubmittedBy, uniqueJobId);
			IBatchStatus sourceJobHistoryErrorUpdater = new JobHistoryErrorBatchUpdateManager(jobHistoryErrorManager, _repositoryFactory, _claimsPrincipalFactory, jobStopManager, configuration.SourceWorkspaceArtifactId, job.SubmittedBy, updateStatusType);

			var batchStatusCommands = new List<IBatchStatus>()
			{
				destinationFieldsTagger,
				sourceFieldsTagger,
				sourceJobHistoryErrorUpdater
			};
			return batchStatusCommands;
		}

		public IExporterService BuildExporter(IJobStopManager jobStopManager, FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);
			return new RelativityExporterService(_repositoryFactory, jobStopManager, _helper, claimsPrincipal, mappedFiles, 0, config, savedSearchArtifactId);
		}
	}
}