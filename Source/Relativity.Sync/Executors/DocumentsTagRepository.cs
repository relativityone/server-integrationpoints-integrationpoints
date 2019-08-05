using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class DocumentsTagRepository : IDocumentsTagRepository
	{
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly ISourceWorkspaceTagRepository _sourceWorkspaceTagRepository;
		private readonly ISyncLog _logger;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

		public DocumentsTagRepository(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository, 
			ISourceWorkspaceTagRepository sourceWorkspaceTagRepository, ISyncLog logger, IJobHistoryErrorRepository jobHistoryErrorRepository)
		{
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_sourceWorkspaceTagRepository = sourceWorkspaceTagRepository;
			_logger = logger;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
		}

		public async Task<IEnumerable<string>> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
			ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
		{
			var failedIdentifiers = new List<string>();
			IList<string> identifiersList = documentIdentifiers.ToList();
			if (identifiersList.Count > 0)
			{
				IList<TagDocumentsResult<string>> taggingResults = await _sourceWorkspaceTagRepository.TagDocumentsAsync(configuration, identifiersList, token).ConfigureAwait(false);
				foreach (TagDocumentsResult<string> taggingResult in taggingResults)
				{
					if (taggingResult.FailedDocuments.Any())
					{
						failedIdentifiers.AddRange(taggingResult.FailedDocuments);
					}
				}
			}
			return failedIdentifiers;
		}

		public async Task<IEnumerable<int>> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
		{
			var failedArtifactIds = new List<int>();
			IList<int> artifactIdsList = artifactIds.ToList();
			if (artifactIdsList.Count > 0)
			{
				IList<TagDocumentsResult<int>> taggingResults = await _destinationWorkspaceTagRepository.TagDocumentsAsync(configuration, artifactIdsList, token).ConfigureAwait(false);
				foreach (TagDocumentsResult<int> taggingResult in taggingResults)
				{
					if (taggingResult.FailedDocuments.Any())
					{
						failedArtifactIds.AddRange(taggingResult.FailedDocuments);
					}
				}
			}
			return failedArtifactIds;
		}

		public async Task<ExecutionResult> GetTaggingResults<TIdentifier>(IList<Task<IEnumerable<TIdentifier>>> taggingTasks, int jobHistoryArtifactId)
		{
			ExecutionResult taggingResult = ExecutionResult.Success();
			var failedTagArtifactIds = new List<TIdentifier>();
			try
			{
				IEnumerable<IEnumerable<TIdentifier>> taskResults = await Task.WhenAll(taggingTasks).ConfigureAwait(false);
				foreach (var taskResult in taskResults)
				{
					failedTagArtifactIds.AddRange(taskResult);
				}

				if (failedTagArtifactIds.Any())
				{
					const int maxSubset = 50;
					int subsetCount = failedTagArtifactIds.Count < maxSubset ? failedTagArtifactIds.Count : maxSubset;
					string subsetArtifactIds = string.Join(",", failedTagArtifactIds.Take(subsetCount));

					string errorMessage = $"Failed to tag synchronized documents in workspace. The first {subsetCount} out of {failedTagArtifactIds.Count} are: {subsetArtifactIds}.";
					var failedTaggingException = new SyncException(errorMessage, jobHistoryArtifactId.ToString(CultureInfo.InvariantCulture));
					taggingResult = ExecutionResult.Failure(errorMessage, failedTaggingException);
				}
			}
			catch (OperationCanceledException oce)
			{
				const string taggingCanceledMessage = "Tagging synchronized documents in workspace was interrupted due to the job being canceled.";
				_logger.LogInformation(oce, taggingCanceledMessage);
				taggingResult = new ExecutionResult(ExecutionStatus.Canceled, taggingCanceledMessage, oce);
			}
			catch (Exception ex)
			{
				const string message = "Unexpected exception occurred while tagging synchronized documents in workspace.";
				_logger.LogError(ex, message);
				taggingResult = ExecutionResult.Failure(message, ex);
			}
			return taggingResult;
		}
		public async Task GenerateDocumentTaggingJobHistoryError(ExecutionResult taggingResult, ISynchronizationConfiguration configuration)
		{
			var jobHistoryError = new CreateJobHistoryErrorDto(configuration.JobHistoryArtifactId, ErrorType.Job)
			{
				ErrorMessage = taggingResult.Message,
				StackTrace = taggingResult.Exception?.StackTrace
			};
			await _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, jobHistoryError).ConfigureAwait(false);
		}
	}
}