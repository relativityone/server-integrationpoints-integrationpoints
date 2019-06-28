﻿using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Helpers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExportServiceObserversFactory : IExportServiceObserversFactory
	{
		private readonly IHelper _helper;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ISourceDocumentsTagger _sourceDocumentsTagger;
		private readonly IMassUpdateHelper _massUpdateHelper;
		private readonly IAPILog _logger;

		public ExportServiceObserversFactory(
			IHelper helper,
			IRepositoryFactory repositoryFactory,
			ISourceDocumentsTagger sourceDocumentsTagger,
			IMassUpdateHelper massUpdateHelper,
			IAPILog logger)
		{
			_helper = helper;
			_repositoryFactory = repositoryFactory;
			_sourceDocumentsTagger = sourceDocumentsTagger;
			_massUpdateHelper = massUpdateHelper;
			_logger = logger;
		}

		public List<IBatchStatus> InitializeExportServiceJobObservers(
			Job job,
			ITagsCreator tagsCreator,
			ITagSavedSearchManager tagSavedSearchManager,
			ISynchronizerFactory synchronizerFactory,
			ISerializer serializer,
			IJobHistoryErrorManager jobHistoryErrorManager,
			IJobStopManager jobStopManager,
			ISourceWorkspaceTagCreator sourceWorkspaceTagCreator,
			FieldMap[] mappedFields,
			SourceConfiguration configuration,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType,
			JobHistory jobHistory,
			string uniqueJobId,
			string userImportApiSettings)
		{
			IConsumeScratchTableBatchStatus destinationFieldsTagger = CreateDestinationFieldsTagger(
				tagsCreator,
				tagSavedSearchManager,
				synchronizerFactory,
				serializer,
				mappedFields,
				configuration,
				jobHistory,
				uniqueJobId,
				userImportApiSettings);

			IConsumeScratchTableBatchStatus sourceFieldsTagger = CreateSourceFieldsTagger(
				configuration,
				jobHistory,
				sourceWorkspaceTagCreator,
				uniqueJobId);

			IBatchStatus sourceJobHistoryErrorUpdater = CreateJobHistoryErrorUpdater(
				jobHistoryErrorManager,
				jobStopManager,
				configuration,
				updateStatusType);

			return new List<IBatchStatus>
			{
				destinationFieldsTagger,
				sourceFieldsTagger,
				sourceJobHistoryErrorUpdater
			};
		}

		private IConsumeScratchTableBatchStatus CreateDestinationFieldsTagger(
			ITagsCreator tagsCreator,
			ITagSavedSearchManager tagSavedSearchManager,
			ISynchronizerFactory synchronizerFactory,
			ISerializer serializer,
			FieldMap[] mappedFields,
			SourceConfiguration sourceConfiguration,
			JobHistory jobHistory,
			string uniqueJobId,
			string userImportApiSettings)
		{
			IDocumentRepository documentRepository = _repositoryFactory.GetDocumentRepository(sourceConfiguration.SourceWorkspaceArtifactId);

			var taggerFactory = new TargetDocumentsTaggingManagerFactory(
				_repositoryFactory,
				tagsCreator,
				tagSavedSearchManager,
				documentRepository,
				synchronizerFactory,
				_helper,
				serializer,
				mappedFields,
				sourceConfiguration,
				userImportApiSettings,
				jobHistory.ArtifactId,
				uniqueJobId);

			IConsumeScratchTableBatchStatus destinationFieldsTagger = taggerFactory.BuildDocumentsTagger();
			return destinationFieldsTagger;
		}

		private IConsumeScratchTableBatchStatus CreateSourceFieldsTagger(
			SourceConfiguration configuration,
			JobHistory jobHistory,
			ISourceWorkspaceTagCreator sourceWorkspaceTagsCreator,
			string uniqueJobId)
		{
			return new SourceObjectBatchUpdateManager(
				_repositoryFactory,
				_logger,
				sourceWorkspaceTagsCreator,
				_sourceDocumentsTagger,
				configuration,
				jobHistory,
				uniqueJobId);
		}

		private IBatchStatus CreateJobHistoryErrorUpdater(
			IJobHistoryErrorManager jobHistoryErrorManager,
			IJobStopManager jobStopManager,
			SourceConfiguration configuration,
			JobHistoryErrorDTO.UpdateStatusType updateStatusType)
		{
			return new JobHistoryErrorBatchUpdateManager(
				jobHistoryErrorManager,
				_logger,
				_repositoryFactory,
				jobStopManager,
				configuration.SourceWorkspaceArtifactId,
				updateStatusType,
				_massUpdateHelper);
		}
	}
}
