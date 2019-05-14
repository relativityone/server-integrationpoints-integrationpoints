using System;
using System.Security.Claims;
using kCura.IntegrationPoints.Common.Monitoring.Constants;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations
{
	public class SourceObjectBatchUpdateManager : IConsumeScratchTableBatchStatus
	{
		private int _destinationWorkspaceRdoId;
		private bool _errorOccurDuringJobStart;

		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly int _destinationWorkspaceId;
		private readonly IDestinationWorkspaceRepository _destinationWorkspaceRepository;
		private readonly int _jobHistoryInstanceId;
		private readonly IAPILog _logger;
		private readonly int _sourceWorkspaceId;
		private readonly int? _federatedInstanceId;
		private readonly ISourceWorkspaceTagCreator _sourceWorkspaceTagCreator;

		public SourceObjectBatchUpdateManager(IRepositoryFactory sourceRepositoryFactory, IRepositoryFactory targetRepositoryFactory,
			IOnBehalfOfUserClaimsPrincipalFactory userClaimsPrincipalFactory, IHelper helper, IFederatedInstanceManager federatedInstanceManager, ISourceWorkspaceTagCreator sourceWorkspaceTagCreator, 
			SourceConfiguration sourceConfig, int jobHistoryInstanceId, int submittedBy, string uniqueJobId)
		{
			_destinationWorkspaceRepository = sourceRepositoryFactory.GetDestinationWorkspaceRepository(sourceConfig.SourceWorkspaceArtifactId);
			ScratchTableRepository = sourceRepositoryFactory.GetScratchTableRepository(sourceConfig.SourceWorkspaceArtifactId, Data.Constants.TEMPORARY_DOC_TABLE_SOURCE_OBJECTS, uniqueJobId);
			_claimsPrincipal = userClaimsPrincipalFactory.CreateClaimsPrincipal(submittedBy);
			_sourceWorkspaceId = sourceConfig.SourceWorkspaceArtifactId;
			_destinationWorkspaceId = sourceConfig.TargetWorkspaceArtifactId;
			_federatedInstanceId = sourceConfig.FederatedInstanceArtifactId;
			_jobHistoryInstanceId = jobHistoryInstanceId;
			_sourceWorkspaceTagCreator = sourceWorkspaceTagCreator;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SourceObjectBatchUpdateManager>();
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
				if (!_errorOccurDuringJobStart)
				{
					int documentCount = ScratchTableRepository.Count;
					LogTaggingDocumentsStarted(documentCount);
					using (Client.MetricsClient.LogDuration(
						TelemetryMetricsBucketNames.BUCKET_SYNC_SOURCE_DOCUMENTS_TAGGING_DURATION,
						Guid.Empty,
						_jobHistoryInstanceId.ToString())
						)
					{
						_destinationWorkspaceRepository.TagDocsWithDestinationWorkspaceAndJobHistory(
							_claimsPrincipal,
							documentCount,
							_destinationWorkspaceRdoId,
							_jobHistoryInstanceId,
							ScratchTableRepository.GetTempTableName(), 
							_sourceWorkspaceId
						);
					}

				}
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

		#region Logging

		private IntegrationPointsException LogAndWrapExceptionFromJobStart(Exception e)
		{
			return LogAndWrapException(e,
				"Error occurred during linking destination workspace to JobHistory in SourceObjectBatchUpdateManager.");
		}

		private IntegrationPointsException LogAndWrapExceptionFromJobComplete(Exception e)
		{
			return LogAndWrapException(e, "Error occurred during job completion in SourceObjectBatchUpdateManager");
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

		private void LogTaggingDocumentsStarted(int documentCount)
		{
			_logger.LogDebug("Tagging {documentCount} documents in destination workspace {workspaceId} for job {jobIInstanceId}.",
				documentCount, _destinationWorkspaceId, _jobHistoryInstanceId);
		}

		#endregion
	}
}