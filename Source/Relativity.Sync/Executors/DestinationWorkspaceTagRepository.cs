using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors
{
    internal sealed class DestinationWorkspaceTagRepository : WorkspaceTagRepositoryBase<int>, IDestinationWorkspaceTagRepository
    {
        private readonly IFederatedInstance _federatedInstance;
        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IAPILog _logger;
        private readonly ISyncMetrics _syncMetrics;
        private readonly ITagNameFormatter _tagNameFormatter;
        private readonly IRdoGuidConfiguration _rdoGuidConfiguration;
        private readonly Func<IStopwatch> _stopwatch;

        public DestinationWorkspaceTagRepository(ISourceServiceFactoryForUser serviceFactoryForUser, IFederatedInstance federatedInstance, ITagNameFormatter tagNameFormatter,
            IRdoGuidConfiguration rdoGuidConfiguration, IAPILog logger, ISyncMetrics syncMetrics, Func<IStopwatch> stopwatch)
            : base(logger)
        {
            _federatedInstance = federatedInstance;
            _logger = logger;
            _serviceFactoryForUser = serviceFactoryForUser;
            _syncMetrics = syncMetrics;
            _tagNameFormatter = tagNameFormatter;
            _rdoGuidConfiguration = rdoGuidConfiguration;
            _stopwatch = stopwatch;
        }

        public async Task<DestinationWorkspaceTag> ReadAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token)
        {
            _logger.LogVerbose(
                $"Reading {nameof(DestinationWorkspaceTag)}. Source workspace artifact ID: {{sourceWorkspaceArtifactId}} " +
                "Destination workspace artifact ID: {destinationWorkspaceArtifactId}",
                sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
            RelativityObject tag = await QueryRelativityObjectTagAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, token).ConfigureAwait(false);

            DestinationWorkspaceTag destinationWorkspaceTag = null;
            if (tag != null)
            {
                destinationWorkspaceTag = new DestinationWorkspaceTag
                {
                    ArtifactId = tag.ArtifactID,
                    DestinationWorkspaceName = tag[_rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceNameGuid].Value.ToString(),
                    DestinationInstanceName = tag[_rdoGuidConfiguration.DestinationWorkspace.DestinationInstanceNameGuid].Value.ToString(),
                    DestinationWorkspaceArtifactId = Convert.ToInt32(tag[_rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceArtifactIdGuid].Value, CultureInfo.InvariantCulture)
                };
            }

            return destinationWorkspaceTag;
        }

        public async Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
        {
            _logger.LogVerbose(
                $"Creating {nameof(DestinationWorkspaceTag)} in source workspace ID: {{sourceWorkspaceArtifactId}} " +
                    "Destination workspace ID: {destinationWorkspaceArtifactId}",
                sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
            string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);

            int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                var request = new CreateRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        Guid = _rdoGuidConfiguration.DestinationWorkspace.TypeGuid
                    },
                    FieldValues = CreateFieldValues(destinationWorkspaceArtifactId, destinationWorkspaceName, federatedInstanceName, federatedInstanceId)
                };

                CreateResult result;
                try
                {
                    result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
                }
                catch (ServiceException ex)
                {
                    request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
                    _logger.LogError(ex, $"Service call failed while creating {nameof(DestinationWorkspaceTag)}: {{request}}", request);
                    throw new SyncKeplerException($"Service call failed while creating {nameof(DestinationWorkspaceTag)}: {request}", ex);
                }
                catch (Exception ex)
                {
                    request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
                    _logger.LogError(ex, $"Failed to create {nameof(DestinationWorkspaceTag)}: {{request}}", request);
                    throw new SyncKeplerException($"Failed to create {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}", ex);
                }

                var createdTag = new DestinationWorkspaceTag
                {
                    ArtifactId = result.Object.ArtifactID,
                    DestinationWorkspaceName = destinationWorkspaceName,
                    DestinationInstanceName = federatedInstanceName,
                    DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId
                };
                return createdTag;
            }
        }

        public async Task UpdateAsync(int sourceWorkspaceArtifactId, DestinationWorkspaceTag destinationWorkspaceTag)
        {
            _logger.LogVerbose($"Updating {nameof(DestinationWorkspaceTag)} in source workspace ID: {{sourceWorkspaceArtifactId}}", sourceWorkspaceArtifactId);
            string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);
            int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

            var request = new UpdateRequest
            {
                Object = new RelativityObjectRef { ArtifactID = destinationWorkspaceTag.ArtifactId },
                FieldValues = CreateFieldValues(destinationWorkspaceTag.DestinationWorkspaceArtifactId, destinationWorkspaceTag.DestinationWorkspaceName, federatedInstanceName, federatedInstanceId),
            };

            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                try
                {
                    await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
                }
                catch (ServiceException ex)
                {
                    request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
                    _logger.LogError(ex, $"Service call failed while updating {nameof(DestinationWorkspaceTag)}: {{request}}", request);
                    throw new SyncKeplerException($"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTag.ArtifactId} in workspace {sourceWorkspaceArtifactId}", ex);
                }
                catch (Exception ex)
                {
                    request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
                    _logger.LogError(ex, $"Failed to update {nameof(DestinationWorkspaceTag)}: {{request}}", request);
                    throw new SyncKeplerException($"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTag.ArtifactId} in workspace {sourceWorkspaceArtifactId}", ex);
                }
            }
        }

        protected override async Task<TagDocumentsResult<int>> TagDocumentsBatchAsync(
            ISynchronizationConfiguration synchronizationConfiguration, IList<int> batch, IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token)
        {
            var updateByIdentifiersRequest = new MassUpdateByObjectIdentifiersRequest
            {
                Objects = ConvertArtifactIdsToObjectRefs(batch),
                FieldValues = fieldValues
            };

            TagDocumentsResult<int> result;
            IStopwatch stopwatch = _stopwatch();
            try
            {
                using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
                {
                    stopwatch.Start();
                    MassUpdateResult updateResult = await objectManager.UpdateAsync(synchronizationConfiguration.SourceWorkspaceArtifactId, updateByIdentifiersRequest, massUpdateOptions, token).ConfigureAwait(false);
                    result = GenerateTagDocumentsResult(updateResult, batch);
                    stopwatch.Stop();
                }
            }
            catch (Exception updateException)
            {
                const string exceptionMessage = "Mass tagging Documents with Destination Workspace and Job History fields failed.";
                const string exceptionTemplate =
                    "Mass tagging documents in source workspace {SourceWorkspace} with destination workspace field {DestinationWorkspaceField} and job history field {JobHistoryField} failed.";

                _logger.LogError(updateException, exceptionTemplate,
                    synchronizationConfiguration.SourceWorkspaceArtifactId, synchronizationConfiguration.DestinationWorkspaceTagArtifactId, synchronizationConfiguration.JobHistoryArtifactId);
                result = new TagDocumentsResult<int>(batch, exceptionMessage, false, 0);
            }

            _syncMetrics.Send(new DestinationWorkspaceTagMetric
            {
                BatchSize = batch.Count,
                SourceUpdateTime = stopwatch.Elapsed.TotalMilliseconds,
                SourceUpdateCount = result.TotalObjectsUpdated,
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
                    Field = new FieldRef
                    {
                        Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceOnDocument
                    },
                    Value = ToMultiObjectValue(synchronizationConfiguration.DestinationWorkspaceTagArtifactId)
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.JobHistoryOnDocumentGuid },
                    Value = ToMultiObjectValue(synchronizationConfiguration.JobHistoryArtifactId)
                }
            };
            return fieldRefValuePairs;
        }

        private async Task<RelativityObject> QueryRelativityObjectTagAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token)
        {
            using (var objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

                var request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.TypeGuid },
                    Condition = $"'{_rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceArtifactIdGuid}' == {destinationWorkspaceArtifactId} AND ('{_rdoGuidConfiguration.DestinationWorkspace.DestinationInstanceArtifactIdGuid}' == {federatedInstanceId})",
                    Fields = new List<FieldRef>
                    {
                        new FieldRef { Name = "ArtifactId" },
                        new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceNameGuid },
                        new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationInstanceNameGuid },
                        new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceArtifactIdGuid },
                        new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationInstanceArtifactIdGuid },
                        new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.NameGuid }
                    }
                };
                QueryResult queryResult;
                try
                {
                    const int start = 0;
                    const int length = 1;
                    queryResult = await objectManager.QueryAsync(sourceWorkspaceArtifactId, request, start, length).ConfigureAwait(false);
                }
                catch (ServiceException ex)
                {
                    _logger.LogError(ex, "Service call failed while querying {TagObject} object: {Request}.", nameof(DestinationWorkspaceTag), request);
                    throw new SyncKeplerException($"Service call failed while querying {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to query {TagObject} object: {Request}.", nameof(DestinationWorkspaceTag), request);
                    throw new SyncKeplerException($"Failed to query {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}.", ex);
                }

                return queryResult.Objects.FirstOrDefault();
            }
        }

        private static IReadOnlyList<RelativityObjectRef> ConvertArtifactIdsToObjectRefs(IList<int> artifactIds)
        {
            var objectRefs = new RelativityObjectRef[artifactIds.Count];

            for (int i = 0; i < artifactIds.Count; i++)
            {
                var objectRef = new RelativityObjectRef
                {
                    ArtifactID = artifactIds[i]
                };
                objectRefs[i] = objectRef;
            }

            return objectRefs;
        }

        private IEnumerable<FieldRefValuePair> CreateFieldValues(int destinationWorkspaceArtifactId, string destinationWorkspaceName, string federatedInstanceName, int federatedInstanceId)
        {
            string destinationTagName = _tagNameFormatter.FormatWorkspaceDestinationTagName(federatedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
            FieldRefValuePair[] pairs =
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.NameGuid },
                    Value = destinationTagName
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceNameGuid },
                    Value = destinationWorkspaceName
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationWorkspaceArtifactIdGuid },
                    Value = destinationWorkspaceArtifactId
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationInstanceNameGuid },
                    Value = federatedInstanceName
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = _rdoGuidConfiguration.DestinationWorkspace.DestinationInstanceArtifactIdGuid },
                    Value = federatedInstanceId
                }
            };
            return pairs;
        }

        private IEnumerable<FieldRefValuePair> RemoveSensitiveUserData(IEnumerable<FieldRefValuePair> fieldValues)
        {
            fieldValues.First(fieldValue => fieldValue.Field.Guid == _rdoGuidConfiguration.DestinationWorkspace.NameGuid).Value = "[Sensitive user data has been removed]";

            return fieldValues;
        }
    }
}
