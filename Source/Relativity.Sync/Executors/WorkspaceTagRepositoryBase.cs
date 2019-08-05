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

		public abstract Task<IList<TagDocumentsResult<T>>> TagDocumentsAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<T> documentArtifactIds, CancellationToken token);

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