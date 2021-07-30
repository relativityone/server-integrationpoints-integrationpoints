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
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class DocumentJobStartMetricsExecutor : IExecutor<IDocumentJobStartMetricsConfiguration>
	{
		private readonly ISyncLog _logger;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IFieldManager _fieldManager;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IFileStatisticsCalculator _fileStatisticsCalculator;
		private readonly ISnapshotQueryRequestProvider _queryRequestProvider;

		private const string _EXTRACTED_TEXT_FIELD_NAME = "Extracted Text";

		public DocumentJobStartMetricsExecutor(ISyncLog logger, ISyncMetrics syncMetrics, IFieldManager fieldManager,
			ISourceServiceFactoryForUser serviceFactory, IJobStatisticsContainer jobStatisticsContainer,
			IFileStatisticsCalculator fileStatisticsCalculator, ISnapshotQueryRequestProvider queryRequestProvider)
		{
			_logger = logger;
			_syncMetrics = syncMetrics;
			_fieldManager = fieldManager;
			_serviceFactory = serviceFactory;
			_jobStatisticsContainer = jobStatisticsContainer;
			_fileStatisticsCalculator = fileStatisticsCalculator;
			_queryRequestProvider = queryRequestProvider;
		}

		public async Task<ExecutionResult> ExecuteAsync(IDocumentJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
		{
			if (configuration.Resuming)
			{
				_syncMetrics.Send(new JobResumeMetric
				{
					Type = TelemetryConstants.PROVIDER_NAME,
					RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null
				});
			}
			else
			{
				_syncMetrics.Send(new JobStartMetric
				{
					Type = TelemetryConstants.PROVIDER_NAME,
					FlowType = TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA,
					RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null
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

			_jobStatisticsContainer.NativesBytesRequested = CreateCalculateNativesTotalSizeTaskAsync(configuration, token);

			return ExecutionResult.Success();
		}

		private Task<long> CreateCalculateNativesTotalSizeTaskAsync(IDocumentJobStartMetricsConfiguration configuration,
			CompositeCancellationToken token)
		{
			if(configuration.ImportNativeFileCopyMode == ImportNativeFileCopyMode.CopyFiles)
            {
				return Task.Run(async () =>
				{
					_logger.LogInformation("Natives bytes requested calculation has been started...");
					QueryRequest request = await _queryRequestProvider.GetRequestWithIdentifierOnlyForCurrentPipelineAsync(token.StopCancellationToken).ConfigureAwait(false);
					return await _fileStatisticsCalculator.CalculateNativesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, request, token).ConfigureAwait(false);
				}, token.StopCancellationToken);
            }

			return Task.FromResult(0L);
		}

		private async Task LogFieldsMappingDetailsAsync(IDocumentJobStartMetricsConfiguration configuration, CancellationToken token)
		{
			IList<FieldInfoDto> documentFields = await _fieldManager.GetMappedDocumentFieldsAsync(token).ConfigureAwait(false);

			Task<Dictionary<string, RelativityObjectSlim>> sourceFieldsDetailsTask = GetFieldsDetailsAsync(configuration.SourceWorkspaceArtifactId,
				documentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
					.Select(x => x.SourceFieldName), token);

			Task<Dictionary<string, RelativityObjectSlim>> destinationFieldsDetailsTask = GetFieldsDetailsAsync(configuration.DestinationWorkspaceArtifactId,
				documentFields.Where(x => x.RelativityDataType == RelativityDataType.LongText)
					.Select(x => x.DestinationFieldName), token);

			await Task.WhenAll(sourceFieldsDetailsTask, destinationFieldsDetailsTask).ConfigureAwait(false);

			_logger.LogInformation("Fields map configuration summary: {@summary}",
				GetFieldsMappingSummary(documentFields, sourceFieldsDetailsTask.Result, destinationFieldsDetailsTask.Result));
		}

		private async Task<Dictionary<string, RelativityObjectSlim>> GetFieldsDetailsAsync(int workspaceId,
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
					$"'Name' IN [{concatenatedFieldNames}] AND 'Object Type Artifact Type ID' == {(int)ArtifactType.Document}",
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

		private Dictionary<string, object> GetFieldsMappingSummary(IList<FieldInfoDto> mappings,
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
