using Relativity.Sync.Configuration;

namespace Relativity.Sync.Pipelines
{
	internal class PipelineSelector : IPipelineSelector
	{
		private readonly IRetryDataSourceSnapshotConfiguration _retryDataSourceSnapshotConfiguration;

		public PipelineSelector(IRetryDataSourceSnapshotConfiguration retryDataSourceSnapshotConfiguration)
		{
			_retryDataSourceSnapshotConfiguration = retryDataSourceSnapshotConfiguration;
		}

		public ISyncPipeline GetPipeline()
		{
			if (IsDocumentRetry())
			{
				return new SyncDocumentRetryPipeline();
			}

			return new SyncDocumentRunPipeline();
		}

		private bool IsDocumentRetry()
		{
			return _retryDataSourceSnapshotConfiguration.JobHistoryToRetryId != null;
		}
	}
}