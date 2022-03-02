using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Relativity.Sync.Configuration;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class NonDocumentJobStartMetricsExecutor : IExecutor<INonDocumentJobStartMetricsConfiguration>
	{
        private const string _EXTRACTED_TEXT_FIELD_NAME = "Extracted Text";
        private const string _NOT_ASSIGNED_APPLICATION_NAME = "Custom";

		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
        private readonly IFieldManager _fieldManager;
        private readonly ISyncLog _logger;
        private readonly ISyncMetrics _syncMetrics;

		public NonDocumentJobStartMetricsExecutor(ISourceServiceFactoryForAdmin serviceFactory, ISyncLog logger, ISyncMetrics syncMetrics, IFieldManager fieldManager)
        {
            _serviceFactory = serviceFactory;
            _logger = logger;
            _syncMetrics = syncMetrics;
            _fieldManager = fieldManager;
        }

        public async Task<ExecutionResult> ExecuteAsync(INonDocumentJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
        {
            string dataSourceType = ((ArtifactType)configuration.RdoArtifactTypeId).ToString();
            string parentApplicationName = await GetParentApplicationNameAsync(configuration);

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
                _syncMetrics.Send(new NonDocumentJobStartMetric
                {
                    Type = TelemetryConstants.PROVIDER_NAME,
                    FlowType = TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NON_DOCUMENT_OBJECTS,
                    RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null,
					DataSourceType = dataSourceType,
					ParentApplicationName = parentApplicationName
				});

                try
                {
                    await LogFieldsMappingDetailsAsync(configuration, token.StopCancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.LogError("Exception occurred when trying to log mapping details", exception);
                }
            }

            return ExecutionResult.Success();
		}

        private async Task<string> GetParentApplicationNameAsync(INonDocumentJobStartMetricsConfiguration configuration)
        {
            using (IObjectTypeManager objectTypeManager = await _serviceFactory.CreateProxyAsync<IObjectTypeManager>())
            {
                ObjectTypeResponse objectTypeResponse =
                    await objectTypeManager.ReadAsync(configuration.SourceWorkspaceArtifactId,
                        configuration.RdoArtifactTypeId);
                if (objectTypeResponse.RelativityApplications.ViewableItems.Any())
                {
                    return objectTypeResponse.RelativityApplications.ViewableItems.First().Name;
                }

                return _NOT_ASSIGNED_APPLICATION_NAME;
            }

		}
		
        private async Task LogFieldsMappingDetailsAsync(INonDocumentJobStartMetricsConfiguration configuration, CancellationToken token)
        {
            IReadOnlyList<FieldInfoDto> nonDocumentFields = await _fieldManager.GetMappedFieldNonDocumentWithoutLinksAsync(token).ConfigureAwait(false);

            Task<Dictionary<string, RelativityObjectSlim>> sourceFieldsDetailsTask = GetFieldsDetailsAsync(configuration.SourceWorkspaceArtifactId,
                nonDocumentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
                    .Select(x => x.SourceFieldName), configuration.RdoArtifactTypeId, token);

            Task<Dictionary<string, RelativityObjectSlim>> destinationFieldsDetailsTask = GetFieldsDetailsAsync(configuration.DestinationWorkspaceArtifactId,
                nonDocumentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
                    .Select(x => x.DestinationFieldName), configuration.RdoArtifactTypeId, token);

            await Task.WhenAll(sourceFieldsDetailsTask, destinationFieldsDetailsTask).ConfigureAwait(false);

            _logger.LogInformation("Fields map configuration summary: {@summary}",
                GetFieldsMappingSummary(nonDocumentFields, sourceFieldsDetailsTask.Result, destinationFieldsDetailsTask.Result));
        }

		private async Task<Dictionary<string, RelativityObjectSlim>> GetFieldsDetailsAsync(int workspaceId,
			IEnumerable<string> fieldNames, int rdoArtifactTypeId, CancellationToken token)
		{
			if (fieldNames == null || !fieldNames.Any())
			{
				return new Dictionary<string, RelativityObjectSlim>();
			}

			ICollection<string> requestedFieldNames = new HashSet<string>(fieldNames);

			IEnumerable<string> formattedFieldNames =
				requestedFieldNames.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(f => $"'{f}'");
			string concatenatedFieldNames = string.Join(", ", formattedFieldNames);

			QueryRequest request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { Name = "Field" },
				Condition =
					$"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {rdoArtifactTypeId}",
				Fields = new[]
				{
					new FieldRef {Name = "Name"},
					new FieldRef {Name = "Enable Data Grid"}
				}
			};

			QueryResultSlim result;
			using (var objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					const int start = 0;
					result = await objectManager
						.QuerySlimAsync(workspaceId, request, start, requestedFieldNames.Count, token)
						.ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex,
						"Service call failed while querying non document fields in workspace {workspaceArtifactId} for mapping details",
						workspaceId);
					throw new SyncKeplerException(
						$"Service call failed while querying non document fields in workspace {workspaceId} for mapping details",
						ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex,
						"Failed to query non document fields in workspace {workspaceArtifactId} for mapping details",
						workspaceId);
					throw new SyncKeplerException(
						$"Failed to query non document fields in workspace {workspaceId} for mapping details", ex);
				}
			}

			return result.Objects.ToDictionary(x => x.Values[0].ToString(), x => x);
		}

		private Dictionary<string, object> GetFieldsMappingSummary(IReadOnlyList<FieldInfoDto> mappings,
			IDictionary<string, RelativityObjectSlim> sourceLongTextFieldsDetails,
			IDictionary<string, RelativityObjectSlim> destinationLongTextFieldsDetails)
		{
			const string keyFormat = "[{0}] <--> [{1}]";

			Dictionary<string, int> mappingSummary = mappings
				.GroupBy(x => x.RelativityDataType, x => x)
				.ToDictionary(x => x.Key.ToString(), x => x.Count());

			Dictionary<string, Dictionary<string, Dictionary<string, object>>> longTextFields = mappings.Where(x => x.RelativityDataType == RelativityDataType.LongText)
				.ToDictionary(x => string.Format(keyFormat, x.SourceFieldName, x.DestinationFieldName), x =>
					new Dictionary<string, Dictionary<string, object>>
					{
						{
							"Source", new Dictionary<string, object>()
							{
								{"ArtifactId", sourceLongTextFieldsDetails[x.SourceFieldName].ArtifactID},
								{"DataGridEnabled", sourceLongTextFieldsDetails[x.SourceFieldName].Values[1]}
							}
						},
						{
							"Destination", new Dictionary<string, object>()
							{
								{"ArtifactId", destinationLongTextFieldsDetails[x.DestinationFieldName].ArtifactID},
								{"DataGridEnabled", destinationLongTextFieldsDetails[x.DestinationFieldName].Values[1]}
							}
						}
					});

			string extractedTextKey = string.Format(keyFormat, _EXTRACTED_TEXT_FIELD_NAME, _EXTRACTED_TEXT_FIELD_NAME);

			var summary = new Dictionary<string, object>()
			{
				{ "FieldMapping", mappingSummary },
				{ "ExtractedText", longTextFields.TryGetValue(extractedTextKey, out var v) ? v : null },
				{ "LongText", longTextFields.Where(x => x.Key != extractedTextKey).Select(x => x.Value).ToArray() }
			};

			return summary;
		}
	}
}
