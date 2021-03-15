using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class ImageJobStartMetricsExecutor : IExecutor<IImageJobStartMetricsConfiguration>
	{
		private readonly ISyncMetrics _syncMetrics;
		private readonly IJobStatisticsContainer _jobStatisticsContainer;
		private readonly IImageFileRepository _imageFileRepository;
		private readonly ISnapshotQueryRequestProvider _queryRequestProvider;

		public ImageJobStartMetricsExecutor(ISyncMetrics syncMetrics, IJobStatisticsContainer jobStatisticsContainer,
			IImageFileRepository imageFileRepository, ISnapshotQueryRequestProvider queryRequestProvider)
		{
			_syncMetrics = syncMetrics;
			_jobStatisticsContainer = jobStatisticsContainer;
			_imageFileRepository = imageFileRepository;
			_queryRequestProvider = queryRequestProvider;
		}

		public Task<ExecutionResult> ExecuteAsync(IImageJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
		{
			JobStartMetric metric = new JobStartMetric
			{
				Type = TelemetryConstants.PROVIDER_NAME,
				FlowType = TelemetryConstants.FLOW_TYPE_SAVED_SEARCH_IMAGES,
				RetryType = configuration.JobHistoryToRetryId != null ? TelemetryConstants.PROVIDER_NAME : null
			};

			_jobStatisticsContainer.ImagesStatistics = CreateCalculateImagesTotalSizeTaskAsync(configuration, token.StopCancellationToken);

			_syncMetrics.Send(metric);

			return Task.FromResult(ExecutionResult.Success());
		}

		private Task<ImagesStatistics> CreateCalculateImagesTotalSizeTaskAsync(IImageJobStartMetricsConfiguration configuration, CancellationToken token)
		{
			QueryImagesOptions options = new QueryImagesOptions
			{
				ProductionIds = configuration.ProductionImagePrecedence,
				IncludeOriginalImageIfNotFoundInProductions = configuration.IncludeOriginalImageIfNotFoundInProductions
			};

			Task<ImagesStatistics> calculateImagesTotalSizeTask = Task.Run(() => _imageFileRepository.CalculateImagesStatisticsAsync(
				configuration.SourceWorkspaceArtifactId, _queryRequestProvider.GetRequestForCurrentPipeline(), options), token);
			return calculateImagesTotalSizeTask;
		}
	}
}
