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
	internal sealed class SourceWorkspaceTagRepository : WorkspaceTagRepositoryBase, ISourceWorkspaceTagRepository
	{
		private const int _MAX_OBJECT_QUERY_BATCH_SIZE = 10000;

		private readonly IProxyFactory _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IFieldMappings _fieldMappings;

		private readonly Guid _sourceWorkspaceTagFieldMultiObject = new Guid("2FA844E3-44F0-47F9-ABB7-D6D8BE0C9B8F");
		private readonly Guid _sourceJobTagFieldMultiObject = new Guid("7CC3FAAF-CBB8-4315-A79F-3AA882F1997F");

		public SourceWorkspaceTagRepository(IDestinationServiceFactoryForUser serviceFactory, ISyncLog logger, ISyncMetrics syncMetrics, IFieldMappings fieldMappings)
		{
			_serviceFactory = serviceFactory;
			_logger = logger;
			_syncMetrics = syncMetrics;
			_fieldMappings = fieldMappings;
		}
		
		public async Task<IList<TagDocumentsResult<string>>> TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<string> documentIdentifiers, CancellationToken token)
		{
			var tagResults = new List<TagDocumentsResult<string>>();
			if (documentIdentifiers.Count == 0)
			{
				const string noUpdateMessage = "A call to the Mass Update API was not made as there are no objects to update.";
				var result = new TagDocumentsResult<string>(documentIdentifiers, noUpdateMessage, true, documentIdentifiers.Count);
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
				TagDocumentsResult<string> tagResult = await TagDocumentsBatchAsync(synchronizationConfiguration, documentArtifactIdBatch, fieldValues, massUpdateOptions, token).ConfigureAwait(false);
				tagResults.Add(tagResult);
			}

			return tagResults;
		}

		private async Task<TagDocumentsResult<string>> TagDocumentsBatchAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<string> batch, IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token)
		{
			var metricsCustomData = new Dictionary<string, object> { { "batchSize", batch.Count } };

			var updateByCriteriaRequest = new MassUpdateByCriteriaRequest
			{
				ObjectIdentificationCriteria = ConvertIdentifiersToObjectCriteria(batch),
				FieldValues = fieldValues
			};

			TagDocumentsResult<string> result = null;
			try
			{
				using (_syncMetrics.TimedOperation("Relativity.Sync.TagDocuments.DestinationUpdate.Time", ExecutionStatus.None, metricsCustomData))
				using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					MassUpdateResult updateResult = await objectManager
						.UpdateAsync(synchronizationConfiguration.DestinationWorkspaceArtifactId, updateByCriteriaRequest, massUpdateOptions, token).ConfigureAwait(false);
					result = GenerateTagDocumentsResult(updateResult, batch);
				}
			}
			catch (Exception updateException)
			{
				const string exceptionMessage = "Mass tagging Documents with Source Workspace and Job History fields failed.";
				const string exceptionTemplate =
					"Mass tagging documents in destination workspace {DestinationWorkspace} with source workspace field {SourceWorkspaceField} and job history field {JobHistoryField} failed.";

				_logger.LogError(updateException, exceptionTemplate,
					synchronizationConfiguration.DestinationWorkspaceArtifactId, synchronizationConfiguration.SourceWorkspaceTagArtifactId, synchronizationConfiguration.SourceJobTagArtifactId);
				result = new TagDocumentsResult<string>(batch, exceptionMessage, false, 0);
			}

			_syncMetrics.GaugeOperation("Relativity.Sync.TagDocuments.DestinationUpdate.Count", ExecutionStatus.None, result.TotalObjectsUpdated, "document(s)", metricsCustomData);

			return result;
		}

		private ObjectIdentificationCriteria ConvertIdentifiersToObjectCriteria(IList<string> identifiers)
		{
			FieldMap identifierField = _fieldMappings.GetFieldMappings().FirstOrDefault(x => x.DestinationField.IsIdentifier);

			if (identifierField == null)
			{
				const string noIdentifierFoundMessage = "Unable to find the destination identifier field in the list of mapped fields for artifact tagging.";
				throw new SyncException(noIdentifierFoundMessage);
			}

			IEnumerable<string> quotedIdentifiers = identifiers.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(i => $"'{i}'");
			string joinedIdentifiers = string.Join(",", quotedIdentifiers);

			var criteria = new ObjectIdentificationCriteria
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
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

		private static TagDocumentsResult<string> GenerateTagDocumentsResult(MassUpdateResult updateResult, IList<string> batch)
		{
			IEnumerable<string> failedDocumentArtifactIds;
			if (!updateResult.Success)
			{
				int elementsToCapture = batch.Count - updateResult.TotalObjectsUpdated;
				failedDocumentArtifactIds = batch.ToList().GetRange(updateResult.TotalObjectsUpdated, elementsToCapture);
			}
			else
			{
				failedDocumentArtifactIds = Array.Empty<string>();
			}
			var result = new TagDocumentsResult<string>(failedDocumentArtifactIds, updateResult.Message, updateResult.Success, updateResult.TotalObjectsUpdated);
			return result;
		}
	}
}