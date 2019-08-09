using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal abstract class WorkspaceTagRepositoryBase<T>
	{
		private const int _MAX_OBJECT_QUERY_BATCH_SIZE = 10000;

		public async Task<IList<TagDocumentsResult<T>>> TagDocumentsAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<T> documentIdentifiers,
			CancellationToken token)
		{
			var tagResults = new List<TagDocumentsResult<T>>();
			if (documentIdentifiers.Count == 0)
			{
				const string noUpdateMessage =
					"A call to the Mass Update API was not made as there are no objects to update.";
				var result = new TagDocumentsResult<T>(documentIdentifiers, noUpdateMessage, true,
					documentIdentifiers.Count);
				tagResults.Add(result);
				return tagResults;
			}

			IEnumerable<FieldRefValuePair> fieldValues = GetDocumentFieldTags(synchronizationConfiguration);
			var massUpdateOptions = new MassUpdateOptions
			{
				UpdateBehavior = FieldUpdateBehavior.Merge
			};
			IEnumerable<IList<T>> documentArtifactIdBatches =
				documentIdentifiers.SplitList(_MAX_OBJECT_QUERY_BATCH_SIZE);
			foreach (IList<T> documentArtifactIdBatch in documentArtifactIdBatches)
			{
				TagDocumentsResult<T> tagResult = await TagDocumentsBatchAsync(synchronizationConfiguration,
					documentArtifactIdBatch, fieldValues, massUpdateOptions, token).ConfigureAwait(false);
				tagResults.Add(tagResult);
			}

			return tagResults;
		}

		public abstract Task<TagDocumentsResult<T>> TagDocumentsBatchAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<T> batch,
			IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token);

		public abstract FieldRefValuePair[] GetDocumentFieldTags(ISynchronizationConfiguration synchronizationConfiguration);

		protected static IEnumerable<RelativityObjectRef> ToMultiObjectValue(params int[] artifactIds)
		{
			return artifactIds.Select(x => new RelativityObjectRef { ArtifactID = x });
		}

		protected static TagDocumentsResult<T> GenerateTagDocumentsResult(MassUpdateResult updateResult, IList<T> batch)
		{
			IEnumerable<T> failedDocumentArtifactIds;
			if (!updateResult.Success)
			{
				int elementsToCapture = batch.Count - updateResult.TotalObjectsUpdated;
				failedDocumentArtifactIds = batch.ToList().GetRange(updateResult.TotalObjectsUpdated, elementsToCapture);
			}
			else
			{
				failedDocumentArtifactIds = Array.Empty<T>();
			}
			var result = new TagDocumentsResult<T>(failedDocumentArtifactIds, updateResult.Message, updateResult.Success, updateResult.TotalObjectsUpdated);
			return result;
		}
	}
}