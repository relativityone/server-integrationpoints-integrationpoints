using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class DocumentsTagRepository : IDocumentTagRepository
	{
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly ISourceWorkspaceTagRepository _sourceWorkspaceTagRepository;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

		public DocumentsTagRepository(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository, 
			ISourceWorkspaceTagRepository sourceWorkspaceTagRepository, IJobHistoryErrorRepository jobHistoryErrorRepository)
		{
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_sourceWorkspaceTagRepository = sourceWorkspaceTagRepository;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
		}

		public async Task<ExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
		{
			var failedIdentifiers = new List<string>();
			IList<string> identifiersList = documentIdentifiers.ToList();
			if (identifiersList.Any())
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

			ExecutionResult sourceTaggingResult = GetTaggingResults(failedIdentifiers, configuration.JobHistoryArtifactId);
			if (sourceTaggingResult.Status == ExecutionStatus.Failed)
			{
				await GenerateDocumentTaggingJobHistoryErrorAsync(sourceTaggingResult, configuration).ConfigureAwait(false);
			}
			return sourceTaggingResult;
		}

		public async Task<ExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
		{
			var failedArtifactIds = new List<int>();
			IList<int> artifactIdsList = artifactIds.ToList();
			if (artifactIdsList.Any())
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

			ExecutionResult destinationTaggingResult = GetTaggingResults(failedArtifactIds, configuration.JobHistoryArtifactId);
			if (destinationTaggingResult.Status == ExecutionStatus.Failed)
			{
				await GenerateDocumentTaggingJobHistoryErrorAsync(destinationTaggingResult, configuration).ConfigureAwait(false);
			}
			return destinationTaggingResult;
		}

		private ExecutionResult GetTaggingResults<TIdentifier>(IList<TIdentifier> failedIdentifiers, int jobHistoryArtifactId)
		{
			ExecutionResult taggingResult = ExecutionResult.Success();
			if (failedIdentifiers.Any())
			{
				const int maxSubset = 50;
				int subsetCount = failedIdentifiers.Count < maxSubset ? failedIdentifiers.Count : maxSubset;
				string subsetArtifactIds = string.Join(",", failedIdentifiers.Take(subsetCount));

				string errorMessage = $"Failed to tag synchronized documents in workspace. The first {subsetCount} out of {failedIdentifiers.Count} are: {subsetArtifactIds}.";
				var failedTaggingException = new SyncException(errorMessage, jobHistoryArtifactId.ToString(CultureInfo.InvariantCulture));
				taggingResult = ExecutionResult.Failure(errorMessage, failedTaggingException);
			}
			return taggingResult;
		}

		private async Task GenerateDocumentTaggingJobHistoryErrorAsync(ExecutionResult taggingResult, ISynchronizationConfiguration configuration)
		{
			var jobHistoryError = new CreateJobHistoryErrorDto(ErrorType.Job)
			{
				ErrorMessage = taggingResult.Message,
				StackTrace = taggingResult.Exception?.StackTrace
			};
			await _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, jobHistoryError).ConfigureAwait(false);
		}
	}
}