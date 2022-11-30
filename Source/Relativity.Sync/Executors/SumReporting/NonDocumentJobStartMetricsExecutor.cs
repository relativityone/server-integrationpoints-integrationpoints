using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
    internal class NonDocumentJobStartMetricsExecutor : IExecutor<INonDocumentJobStartMetricsConfiguration>
    {
        private const string _NOT_ASSIGNED_APPLICATION_NAME = "None";

        private readonly ISourceServiceFactoryForUser _serviceFactory;
        private readonly IFieldMappingSummary _fieldMappingSummary;
        private readonly ISyncMetrics _syncMetrics;
        private readonly IAPILog _logger;

        public NonDocumentJobStartMetricsExecutor(ISourceServiceFactoryForUser serviceFactory, ISyncMetrics syncMetrics, IFieldMappingSummary fieldMappingSummary, IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _syncMetrics = syncMetrics;
            _fieldMappingSummary = fieldMappingSummary;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(INonDocumentJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
        {
            if (configuration.Resuming)
            {
                _syncMetrics.Send(new JobResumeMetric
                {
                    Type = TelemetryConstants.PROVIDER_NAME,
                    RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null,
                });
            }
            else
            {
                string parentApplicationName = await GetParentApplicationNameAsync(configuration).ConfigureAwait(false);

                _syncMetrics.Send(new NonDocumentJobStartMetric
                {
                    Type = TelemetryConstants.PROVIDER_NAME,
                    FlowType = TelemetryConstants.FLOW_TYPE_VIEW_NON_DOCUMENT_OBJECTS,
                    RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null,
                    ParentApplicationName = parentApplicationName
                });

                try
                {
                    Dictionary<string, object> fieldsMappingSummary = await _fieldMappingSummary.GetFieldsMappingSummaryAsync(token.StopCancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Fields mapping summary: {@fieldsMappingSummary}", fieldsMappingSummary);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred when trying to log fields mapping summary");
                }
            }

            return ExecutionResult.Success();
        }

        private async Task<string> GetParentApplicationNameAsync(INonDocumentJobStartMetricsConfiguration configuration)
        {
            string parentApplicationName = _NOT_ASSIGNED_APPLICATION_NAME;

            using (IObjectTypeManager objectTypeManager = await _serviceFactory.CreateProxyAsync<IObjectTypeManager>().ConfigureAwait(false))
            {
                try
                {
                    List<ObjectTypeIdentifier> allObjectTypes = await objectTypeManager.GetAvailableParentObjectTypesAsync(configuration.SourceWorkspaceArtifactId).ConfigureAwait(false);
                    ObjectTypeIdentifier objectTypeIdentifier = allObjectTypes.SingleOrDefault(x => x.ArtifactTypeID == configuration.RdoArtifactTypeId);

                    if (objectTypeIdentifier == null)
                    {
                        _logger.LogWarning("Could not find Object Type with Artifact Type ID {artifactTypeId} in source workspace ID: {workspaceId}", configuration.RdoArtifactTypeId, configuration.SourceWorkspaceArtifactId);
                    }
                    else
                    {
                        ObjectTypeResponse objectTypeResponse = await objectTypeManager.ReadAsync(configuration.SourceWorkspaceArtifactId, objectTypeIdentifier.ArtifactID).ConfigureAwait(false);

                        if (objectTypeResponse?.RelativityApplications?.ViewableItems != null)
                        {
                            parentApplicationName = objectTypeResponse.RelativityApplications.ViewableItems.FirstOrDefault()?.Name;
                        }
                        else
                        {
                            _logger.LogWarning("Cannot read parent application name for Artifact Type ID: {artifactTypeId} Object Type Artifact ID: {objectTypeArtifactId} in Source Workspace ID: {sourceWorkspaceId}. " +
                                               "Response was null - Object Type does not have associated Relativity Application or user does not have permissions to view Applications.",
                                configuration.RdoArtifactTypeId, objectTypeIdentifier.ArtifactID, configuration.SourceWorkspaceArtifactId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Exception occurred while querying for parent application name for Artifact Type ID: {artifactTypeId} in Source Workspace ID: {sourceWorkspaceId}",
                        configuration.RdoArtifactTypeId, configuration.SourceWorkspaceArtifactId);
                }

                return parentApplicationName;
            }
        }
    }
}
