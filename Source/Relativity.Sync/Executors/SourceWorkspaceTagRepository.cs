using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors
{
	internal sealed class SourceWorkspaceTagRepository : ISourceWorkspaceTagRepository
	{
		private const int _MAX_OBJECT_QUERY_BATCH_SIZE = 10000;

		private readonly IProxyFactory _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IFieldMappings _fieldMappings;

		private readonly Guid _sourceWorkspaceTagFieldMultiObject = new Guid("036DB373-5724-4C72-A073-375106DE5E73");
		private readonly Guid _sourceJobTagFieldMultiObject = new Guid("4F632A3F-68CF-400E-BD29-FD364A5EBE58");

		public SourceWorkspaceTagRepository(IDestinationServiceFactoryForUser serviceFactory, ISyncLog logger, ISyncMetrics syncMetrics, IFieldMappings fieldMappings)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
			_syncMetrics = syncMetrics;
			_fieldMappings = fieldMappings;
		}
		
		public async Task<IList<TagDocumentsResult>> TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<string> documentIdentifiers, CancellationToken token)
		{
			var tagResults = new List<TagDocumentsResult>();
			if (documentIdentifiers.Count == 0)
			{
				const string noUpdateMessage = "A call to the Mass Update API was not made as there are no objects to update.";
				var result = new TagDocumentsResult(Array.Empty<int>(), noUpdateMessage, true, documentIdentifiers.Count);
				tagResults.Add(result);
				return tagResults;
			}

			IEnumerable<FieldRefValuePair> fieldValues = GetDocumentFieldTags(synchronizationConfiguration);
			var massUpdateOptions = new MassUpdateOptions
			{
				UpdateBehavior = FieldUpdateBehavior.Merge
			};
			IEnumerable<IList<string>> documentArtifactIdBatches = documentIdentifiers.SplitList(_MAX_OBJECT_QUERY_BATCH_SIZE);
			foreach (IList<string> documentArtifactIdBatch in documentArtifactIdBatches)
			{
				TagDocumentsResult tagResult = await TagDocumentsBatchAsync(synchronizationConfiguration, documentArtifactIdBatch, fieldValues, massUpdateOptions, token).ConfigureAwait(false);
				tagResults.Add(tagResult);
			}

			return tagResults;
		}

		private async Task<TagDocumentsResult> TagDocumentsBatchAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<string> batch, IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token)
		{
			var metricsCustomData = new Dictionary<string, object> { { "batchSize", batch.Count } };

			var updateByCriteriaRequest = new MassUpdateByCriteriaRequest
			{
				ObjectIdentificationCriteria = ConvertIdentifiersToObjectCriteria(batch),
				FieldValues = fieldValues
			};

			TagDocumentsResult result = null;
			try
			{
				using (_syncMetrics.TimedOperation("Relativity.Sync.TagDocuments.DestinationUpdate.Time", ExecutionStatus.None, metricsCustomData))
				using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					MassUpdateResult updateResult = await objectManager
						.UpdateAsync(synchronizationConfiguration.SourceWorkspaceArtifactId, updateByCriteriaRequest,
							massUpdateOptions, token).ConfigureAwait(false);

					result = GenerateTagDocumentsResult(updateResult, Array.Empty<int>());
				}
			}
			catch (SyncException updateException)
			{
				const string exceptionMessage = "Mass tagging Documents with Destination Workspace and Job History fields failed.";
				const string exceptionTemplate =
					"Mass tagging documents in source workspace {SourceWorkspace} with destination workspace field {DestinationWorkspaceField} and job history field {JobHistoryField} failed.";

				_logger.LogError(updateException, exceptionTemplate,
					synchronizationConfiguration.SourceWorkspaceArtifactId, synchronizationConfiguration.DestinationWorkspaceTagArtifactId, synchronizationConfiguration.JobHistoryArtifactId);
				result = new TagDocumentsResult(Array.Empty<int>(), exceptionMessage, false, 0);
			}

			_syncMetrics.GaugeOperation("Relativity.Sync.TagDocuments.SourceUpdate.Count", ExecutionStatus.None, result.TotalObjectsUpdated, "document(s)", metricsCustomData);

			return result;
		}

		private ObjectIdentificationCriteria ConvertIdentifiersToObjectCriteria(IList<string> identifiers)
		{
			FieldMap identifierField = _fieldMappings.GetFieldMappings().FirstOrDefault(x => x.DestinationField.IsIdentifier);
			IEnumerable<string> quotedIdentifiers = identifiers.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(i => $"'{i}'");
			string joinedIdentifiers = string.Join(",", quotedIdentifiers);

			var criteria = new ObjectIdentificationCriteria
			{
				ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Document},
				Condition = $"'{identifierField.DestinationField.DisplayName}' IN [{joinedIdentifiers}]"
			};
			return criteria;
		}

		private FieldRefValuePair[] GetDocumentFieldTags(ISynchronizationConfiguration synchronizationConfiguration)
		{
			FieldRefValuePair[] fieldRefValuePairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _sourceWorkspaceTagFieldMultiObject },
					Value = ToMultiObjectValue(synchronizationConfiguration.SourceWorkspaceTagArtifactId)
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _sourceJobTagFieldMultiObject },
					Value = ToMultiObjectValue(synchronizationConfiguration.SourceJobTagArtifactId)
				}
			};
			return fieldRefValuePairs;
		}

		private static IEnumerable<RelativityObjectRef> ToMultiObjectValue(params int[] artifactIds)
		{
			return artifactIds.Select(x => new RelativityObjectRef { ArtifactID = x });
		}

		private static TagDocumentsResult GenerateTagDocumentsResult(MassUpdateResult updateResult, IList<int> batch)
		{
			IEnumerable<int> failedDocumentArtifactIds;
			if (!updateResult.Success)
			{
				int elementsToCapture = batch.Count - updateResult.TotalObjectsUpdated;
				failedDocumentArtifactIds = batch.ToList().GetRange(updateResult.TotalObjectsUpdated, elementsToCapture);
			}
			else
			{
				failedDocumentArtifactIds = Array.Empty<int>();
			}
			var result = new TagDocumentsResult(failedDocumentArtifactIds, updateResult.Message, updateResult.Success, updateResult.TotalObjectsUpdated);
			return result;
		}
	}
}