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
	internal sealed class DocumentTagRepository : IDocumentTagRepository
	{
		private readonly IDestinationWorkspaceTagRepository _destinationWorkspaceTagRepository;
		private readonly ISourceWorkspaceTagRepository _sourceWorkspaceTagRepository;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

		public DocumentTagRepository(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
			ISourceWorkspaceTagRepository sourceWorkspaceTagRepository, IJobHistoryErrorRepository jobHistoryErrorRepository)
		{
			_destinationWorkspaceTagRepository = destinationWorkspaceTagRepository;
			_sourceWorkspaceTagRepository = sourceWorkspaceTagRepository;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
		}

		public Task<ExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
		{
			return TagDocumentsInWorkspaceWithInfoAsync(_sourceWorkspaceTagRepository.TagDocumentsAsync, configuration, documentIdentifiers, token);
		}

		public Task<ExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
		{
			return TagDocumentsInWorkspaceWithInfoAsync(_destinationWorkspaceTagRepository.TagDocumentsAsync, configuration, artifactIds, token);
		}

		private async Task<ExecutionResult> TagDocumentsInWorkspaceWithInfoAsync<TIdentifier>(
			Func<ISynchronizationConfiguration, IList<TIdentifier>, CancellationToken, Task<IList<TagDocumentsResult<TIdentifier>>>> taggingFunctionAsync,
			ISynchronizationConfiguration configuration, IEnumerable<TIdentifier> documentIdentifiers, CancellationToken token)
		{
			var failedArtifactIds = new List<TIdentifier>();
			IList<TIdentifier> documentIdentifiersList = documentIdentifiers.ToList();
			if (documentIdentifiersList.Any())
			{
				IList<TagDocumentsResult<TIdentifier>> taggingResults = await taggingFunctionAsync.Invoke(configuration, documentIdentifiersList, token).ConfigureAwait(false);
				foreach (TagDocumentsResult<TIdentifier> taggingResult in taggingResults)
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

		private Task GenerateDocumentTaggingJobHistoryErrorAsync(ExecutionResult taggingResult, ISynchronizationConfiguration configuration)
		{
			var jobHistoryError = new CreateJobHistoryErrorDto(ErrorType.Job)
			{
				ErrorMessage = taggingResult.Message,
				StackTrace = taggingResult.Exception?.StackTrace
			};
			return _jobHistoryErrorRepository.CreateAsync(configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, jobHistoryError);
		}
	}
}