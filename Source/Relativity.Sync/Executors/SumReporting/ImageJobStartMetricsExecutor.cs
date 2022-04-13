using Relativity.API;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class ImageJobStartMetricsExecutor : IExecutor<IImageJobStartMetricsConfiguration>
	{
		private readonly IAPILog _syncLog;
		private readonly ISyncMetrics _syncMetrics;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IFileStatisticsCalculator _fileStatisticsCalculator;
		private readonly ISnapshotQueryRequestProvider _queryRequestProvider;

		public ImageJobStartMetricsExecutor(IAPILog syncLog, ISyncMetrics syncMetrics, IJobStatisticsContainer jobStatisticsContainer,
			IFileStatisticsCalculator fileStatisticsCalculator, ISnapshotQueryRequestProvider queryRequestProvider)
		{
			_syncLog = syncLog;
			_syncMetrics = syncMetrics;
			_jobStatisticsContainer = jobStatisticsContainer;
			_fileStatisticsCalculator = fileStatisticsCalculator;
			_queryRequestProvider = queryRequestProvider;
		}

		public Task<ExecutionResult> ExecuteAsync(IImageJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
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
					FlowType = TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES,
					RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null
				});
			}

			_jobStatisticsContainer.ImagesStatistics = CreateCalculateImagesTotalSizeTaskAsync(configuration, token);

			return Task.FromResult(ExecutionResult.Success());
		}

		private Task<ImagesStatistics> CreateCalculateImagesTotalSizeTaskAsync(
			IImageJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
		{
			QueryImagesOptions options = new QueryImagesOptions
			{
				ProductionIds = configuration.ProductionImagePrecedence,
				IncludeOriginalImageIfNotFoundInProductions = configuration.IncludeOriginalImageIfNotFoundInProductions
			};

			Task<ImagesStatistics> calculateImagesTotalSizeTask = Task.Run(async () =>
			{
				_syncLog.LogInformation("Image statistics calculation has been started...");
				QueryRequest request = await _queryRequestProvider.GetRequestWithIdentifierOnlyForCurrentPipelineAsync(token.StopCancellationToken).ConfigureAwait(false);
				return await _fileStatisticsCalculator.CalculateImagesStatisticsAsync(
					configuration.SourceWorkspaceArtifactId, request, options, token).ConfigureAwait(false);
			}, token.StopCancellationToken);
			return calculateImagesTotalSizeTask;
		}
	}
}
