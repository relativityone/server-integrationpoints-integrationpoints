using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal sealed class SourceWorkspaceTagRepository : WorkspaceTagRepositoryBase<string>, ISourceWorkspaceTagRepository
    {
        private readonly IFieldMappings _fieldMappings;
        private readonly IProxyFactory _serviceFactory;
        private readonly ISyncMetrics _syncMetrics;
        private readonly Func<IStopwatch> _stopwatch;

        private readonly Guid _sourceWorkspaceTagFieldMultiObject = new Guid("2FA844E3-44F0-47F9-ABB7-D6D8BE0C9B8F");
        private readonly Guid _sourceJobTagFieldMultiObject = new Guid("7CC3FAAF-CBB8-4315-A79F-3AA882F1997F");

        public SourceWorkspaceTagRepository(
            IDestinationServiceFactoryForUser serviceFactory,
            IAPILog logger,
            ISyncMetrics syncMetrics,
            IFieldMappings fieldMappings,
            Func<IStopwatch> stopwatch)
        : base(logger)
        {
            _fieldMappings = fieldMappings;
            _serviceFactory = serviceFactory;
            _syncMetrics = syncMetrics;
            _stopwatch = stopwatch;
        }

        protected override async Task<TagDocumentsResult<string>> TagDocumentsBatchAsync(
            ISynchronizationConfiguration synchronizationConfiguration, IList<string> batch, CancellationToken token)
        {
            IStopwatch stopwatch = _stopwatch();
            stopwatch.Start();

            Func<IList<string>, int, Task<MassUpdateResult>> taggingFuncAsync =
                (IList<string> batch, int workspaceId) => TagDocumentsInDestinationAsync(batch, workspaceId, synchronizationConfiguration, token);

            TagDocumentsResult<string> result = await TagDocumentsBatchInternalAsync(taggingFuncAsync, batch, synchronizationConfiguration.DestinationWorkspaceArtifactId).ConfigureAwait(false);

            stopwatch.Stop();

            _syncMetrics.Send(new SourceWorkspaceTagMetric
            {
                BatchSize = batch.Count,
                DestinationUpdateTime = stopwatch.Elapsed.TotalMilliseconds,
                DestinationUpdateCount = result.TotalObjectsUpdated,
                UnitOfMeasure = _UNIT_OF_MEASURE
            });

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

        private async Task<MassUpdateResult> TagDocumentsInDestinationAsync(
            IList<string> batch, int workspaceId, ISynchronizationConfiguration configuration, CancellationToken token)
        {
            using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var massUpdateOptions = new MassUpdateOptions
                {
                    UpdateBehavior = FieldUpdateBehavior.Merge
                };

                var updateByCriteriaRequest = new MassUpdateByCriteriaRequest
                {
                    ObjectIdentificationCriteria = ConvertIdentifiersToObjectCriteria(batch),
                    FieldValues = GetDocumentFieldTags(configuration)
                };

                return await objectManager.UpdateAsync(workspaceId, updateByCriteriaRequest, massUpdateOptions, token).ConfigureAwait(false);
            }
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