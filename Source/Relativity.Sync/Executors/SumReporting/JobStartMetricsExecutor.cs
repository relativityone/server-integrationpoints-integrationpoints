using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Pipelines.Extensions;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class JobStartMetricsExecutor : IExecutor<ISumReporterConfiguration>
	{
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IPipelineSelector _pipelineSelector;
		private readonly IFieldManager _fieldManager;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISerializer _serializer;

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int) ArtifactType.Document;
		private string _EXTRACTED_TEXT_FIELD_NAME = "Extracted Text";

		public JobStartMetricsExecutor(ISyncLog logger, ISyncMetrics syncMetrics, IPipelineSelector pipelineSelector, IFieldManager fieldManager,
			ISourceServiceFactoryForUser serviceFactory, ISerializer serializer)
		{
			_logger = logger;
			_syncMetrics = syncMetrics;
			_pipelineSelector = pipelineSelector;
			_fieldManager = fieldManager;
			_serviceFactory = serviceFactory;
			_serializer = serializer;
		}

		public async Task<ExecutionResult> ExecuteAsync(ISumReporterConfiguration configuration,
			CancellationToken token)
		{
			_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE,
				TelemetryConstants.PROVIDER_NAME);

			if (configuration.JobHistoryToRetryId != null)
			{
				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE,
					TelemetryConstants.PROVIDER_NAME);
			}

			LogFlowType();

			try
			{
				await LogFieldsMappingDetailsAsync(configuration, token).ConfigureAwait(false);
			}
			catch (Exception exception)
			{
				_logger.LogError("Exception occured when trying to log mapping details", exception);
			}

			return ExecutionResult.Success();
		}

		private void LogFlowType()
		{
			ISyncPipeline syncPipeline = _pipelineSelector.GetPipeline();
			if (syncPipeline.IsDocumentPipeline())
			{
				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.FLOW_TYPE, TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA);
			}
			else if (syncPipeline.IsImagePipeline())
			{
				_syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.FLOW_TYPE, TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES);
			}
		}

		private async Task LogFieldsMappingDetailsAsync(ISumReporterConfiguration configuration, CancellationToken token)
		{
			IList<FieldInfoDto> documentFields = await _fieldManager.GetMappedDocumentFieldsAsync(token).ConfigureAwait(false);

			var sourceFieldsDetailsTask = GetFieldsDetails(configuration.SourceWorkspaceArtifactId,
				documentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
					.Select(x => x.SourceFieldName), token);

			var destinationFieldsDetailsTask = GetFieldsDetails(configuration.DestinationWorkspaceArtifactId,
				documentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
					.Select(x => x.DestinationFieldName), token);

			await Task.WhenAll(sourceFieldsDetailsTask, destinationFieldsDetailsTask).ConfigureAwait(false);

			_logger.LogInformation(
				"Fields map configuration summary: {summary}",
				SerializeFieldsMappingDetails(documentFields, sourceFieldsDetailsTask.Result, destinationFieldsDetailsTask.Result));
		}

		private async Task<Dictionary<string, RelativityObjectSlim>> GetFieldsDetails(int workspaceId,
			IEnumerable<string> fieldNames, CancellationToken token)
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
					$"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {_DOCUMENT_ARTIFACT_TYPE_ID}",
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
						"Service call failed while querying document fields in workspace {workspaceArtifactId} for mapping details",
						workspaceId);
					throw new SyncKeplerException(
						$"Service call failed while querying document fields in workspace {workspaceId} for mapping details",
						ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex,
						"Failed to query document fields in workspace {workspaceArtifactId} for mapping details",
						workspaceId);
					throw new SyncKeplerException(
						$"Failed to query document fields in workspace {workspaceId} for mapping details", ex);
				}
			}

			return result.Objects.ToDictionary(x => x.Values[0].ToString(), x => x);
		}

		private string SerializeFieldsMappingDetails(IList<FieldInfoDto> mappings,
			IDictionary<string, RelativityObjectSlim> sourceLongTextFieldsDetails,
			IDictionary<string, RelativityObjectSlim> destinationLongTextFieldsDetails)
		{
			const string keyFormat = "[{0}] <--> [{1}]";

			var mappingSummary = mappings
				.GroupBy(x => x.RelativityDataType, x => x)
				.ToDictionary(x => x.Key.ToString(), x => x.Count());

			var longTextFields = mappings.Where(x => x.RelativityDataType == RelativityDataType.LongText)
				.ToDictionary(x => string.Format(keyFormat, x.SourceFieldName, x.DestinationFieldName), x => new
				{
					Source = new
					{
						ArtifactId = sourceLongTextFieldsDetails[x.SourceFieldName].ArtifactID,
						DataGridEnabled = sourceLongTextFieldsDetails[x.SourceFieldName].Values[1]
					},
					Destination = new
					{
						ArtifactId = destinationLongTextFieldsDetails[x.DestinationFieldName].ArtifactID,
						DataGridEnabled = destinationLongTextFieldsDetails[x.DestinationFieldName].Values[1]
					}
				});

			string extractedTextKey = string.Format(keyFormat, _EXTRACTED_TEXT_FIELD_NAME, _EXTRACTED_TEXT_FIELD_NAME);

			var fieldMapInfoObject = new
			{
				FieldMapping = mappingSummary,
				ExtractedText = longTextFields.TryGetValue(extractedTextKey, out var v) ? v : null,
				LongText = longTextFields.Where(x => x.Key != extractedTextKey).Select(x => x.Value).ToArray()
			};

			_logger.LogInformation("Fields map configuration summary: {@summary}",
				_serializer.Serialize(fieldMapInfoObject));
		}
	}
}