using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.Monitoring.Constants;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	internal class SourceObjectBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private int _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		private readonly int _jobHistoryInstanceId;
		private readonly int _destinationWorkspaceId;
		private readonly int? _federatedInstanceId;
		private readonly ISourceDocumentsTagger _sourceDocumentsTagger;
		private readonly ISourceWorkspaceTagCreator _sourceWorkspaceTagCreator;
		private readonly IAPILog _logger;

		public SourceObjectBatchUpdateManager(
			IRepositoryFactory sourceRepositoryFactory,
			IAPILog logger,
			ISourceWorkspaceTagCreator sourceWorkspaceTagCreator,
			ISourceDocumentsTagger sourceDocumentsTagger,
			SourceConfiguration sourceConfig,
			int jobHistoryInstanceId,
			string uniqueJobId)
		{
			ScratchTableRepository = sourceRepositoryFactory.GetScratchTableRepository(sourceConfig.SourceWorkspaceArtifactId, Data.Constants.TEMPORARY_DOC_TABLE_SOURCE_OBJECTS, uniqueJobId);
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_federatedInstanceId = sourceConfig.FederatedInstanceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_sourceWorkspaceTagCreator = sourceWorkspaceTagCreator;
			_sourceDocumentsTagger = sourceDocumentsTagger;
			_logger = logger.ForContext<SourceObjectBatchUpdateManager>();
		}

		public IScratchTableRepository ScratchTableRepository { get; }

		public void OnJobStart(Job job)
		{
			try
			{
				_destinationWorkspaceRdoId = _sourceWorkspaceTagCreator.CreateDestinationWorkspaceTag(_destinationWorkspaceId, _jobHistoryInstanceId, _federatedInstanceId);
				LogDestinationWorkspaceLinkedToJobHistory();
			}
			catch (Exception e)
			{
				_errorOccurDuringJobStart = true;
				throw LogAndWrapExceptionFromJobStart(e);
			}
		}

		public void OnJobComplete(Job job)
		{
			try
			{
				if (_errorOccurDuringJobStart)
				{
					return;
				}

				TagDocumentsAsync().GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				throw LogAndWrapExceptionFromJobComplete(e);
			}
			finally
			{
				ScratchTableRepository.Dispose();
			}
		}

		private async Task TagDocumentsAsync()
		{
			LogTaggingDocumentsStarted();

			using (CreateTaggingDurationLogger())
			{
				await _sourceDocumentsTagger.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
					ScratchTableRepository,
					_destinationWorkspaceRdoId,
					_jobHistoryInstanceId
				).ConfigureAwait(false);
			}
		}

		private DurationLogger CreateTaggingDurationLogger() => Client.MetricsClient.LogDuration(
			TelemetryMetricsBucketNames.BUCKET_SYNC_SOURCE_DOCUMENTS_TAGGING_DURATION,
			Guid.Empty,
			_jobHistoryInstanceId.ToString());

		#region Logging

		private IntegrationPointsException LogAndWrapExceptionFromJobStart(Exception e)
		{
			return LogAndWrapException(e, $"Error occurred during linking destination workspace to JobHistory in {nameof(SourceObjectBatchUpdateManager)}.");
		}

		private IntegrationPointsException LogAndWrapExceptionFromJobComplete(Exception e)
		{
			return LogAndWrapException(e, $"Error occurred during job completion in {nameof(SourceObjectBatchUpdateManager)}");
		}

		private IntegrationPointsException LogAndWrapException(Exception e, string message)
		{
			_logger.LogError(e, message);
			return new IntegrationPointsException(message, e);
		}

		private void LogDestinationWorkspaceLinkedToJobHistory()
		{
			_logger.LogInformation("Destination workspace {_destinationWorkspaceRdoId} linked  to job history {_jobHistoryInstanceId}.", _destinationWorkspaceRdoId, _jobHistoryInstanceId);
		}

		private void LogTaggingDocumentsStarted()
		{
			_logger.LogDebug("Tagging documents in source workspace {workspaceId} for job {jobIInstanceId}.",
				_destinationWorkspaceId, _jobHistoryInstanceId);
		}

		#endregion
	}
}