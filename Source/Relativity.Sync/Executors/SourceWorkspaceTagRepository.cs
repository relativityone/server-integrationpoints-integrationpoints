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
	internal sealed class SourceWorkspaceTagRepository : WorkspaceTagRepositoryBase<string>, ISourceWorkspaceTagRepository
	{
		private readonly IFieldMappings _fieldMappings;
		private readonly IProxyFactory _serviceFactory;
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;

		private readonly Guid _sourceWorkspaceTagFieldMultiObject = new Guid("2FA844E3-44F0-47F9-ABB7-D6D8BE0C9B8F");
		private readonly Guid _sourceJobTagFieldMultiObject = new Guid("7CC3FAAF-CBB8-4315-A79F-3AA882F1997F");

		public SourceWorkspaceTagRepository(IDestinationServiceFactoryForUser serviceFactory, ISyncLog logger, ISyncMetrics syncMetrics, IFieldMappings fieldMappings)
		{
			_fieldMappings = fieldMappings;
			_logger = logger;
			_serviceFactory = serviceFactory;
			_syncMetrics = syncMetrics;
		}
		
		protected override async Task<TagDocumentsResult<string>> TagDocumentsBatchAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<string> batch, IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token)
		{
			var metricsCustomData = new Dictionary<string, object> { { "batchSize", batch.Count } };

			var updateByCriteriaRequest = new MassUpdateByCriteriaRequest
			{
				ObjectIdentificationCriteria = ConvertIdentifiersToObjectCriteria(batch),
				FieldValues = fieldValues
			};

			TagDocumentsResult<string> result;
			try
			{
				using (_syncMetrics.TimedOperation("Relativity.Sync.TagDocuments.DestinationUpdate.Time", ExecutionStatus.None, metricsCustomData))
				using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					MassUpdateResult updateResult = await objectManager.UpdateAsync(synchronizationConfiguration.DestinationWorkspaceArtifactId, updateByCriteriaRequest, massUpdateOptions, token).ConfigureAwait(false);
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

		protected override FieldRefValuePair[] GetDocumentFieldTags(ISynchronizationConfiguration synchronizationConfiguration)
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
	}
}