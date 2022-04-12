using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
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
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IFileStatisticsCalculator _fileStatisticsCalculator;
		private readonly ISnapshotQueryRequestProvider _queryRequestProvider;
		
		public DocumentJobStartMetricsExecutor(ISyncLog logger, ISyncMetrics syncMetrics, IFieldManager fieldManager, IJobStatisticsContainer jobStatisticsContainer,
			IFileStatisticsCalculator fileStatisticsCalculator, ISnapshotQueryRequestProvider queryRequestProvider)
		{
			_logger = logger;
			_syncMetrics = syncMetrics;
			_fieldManager = fieldManager;
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
                    Dictionary<string, object> fieldsMappingSummary = await _fieldManager.GetFieldsMappingSummaryAsync(token.StopCancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Fields mapping summary: {@fieldsMappingSummary}", fieldsMappingSummary);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred when trying to log fields mapping summary");
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
    }
}
