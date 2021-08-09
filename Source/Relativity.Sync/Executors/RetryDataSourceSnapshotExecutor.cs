using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
	internal class RetryDataSourceSnapshotExecutor : DataSourceSnapshotExecutor, IExecutor<IRetryDataSourceSnapshotConfiguration>
	{
		public RetryDataSourceSnapshotExecutor(ISourceServiceFactoryForUser serviceFactory, 
				IJobProgressUpdaterFactory jobProgressUpdaterFactory, ISyncLog logger, 
				ISnapshotQueryRequestProvider snapshotQueryRequestProvider) 
			: base(serviceFactory, jobProgressUpdaterFactory, logger, snapshotQueryRequestProvider)
		{
		}

		public Task<ExecutionResult> ExecuteAsync(IRetryDataSourceSnapshotConfiguration configuration, CompositeCancellationToken token)
		{
			Logger.LogInformation("Setting {ImportOverwriteMode} from {currentMode} to {appendOverlay} for job retry", 
				nameof(configuration.ImportOverwriteMode), configuration.ImportOverwriteMode, ImportOverwriteMode.AppendOverlay);

			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;

			Logger.LogInformation("{ImportOverwriteMode} was successfully updated to {appendOverlay}", nameof(configuration.ImportOverwriteMode), configuration.ImportOverwriteMode);

			return base.ExecuteAsync(configuration, token);
		}
	}
}
