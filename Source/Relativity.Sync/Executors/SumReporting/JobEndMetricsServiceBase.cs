using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Executors.SumReporting
{
	internal abstract class JobEndMetricsServiceBase
	{
		private readonly IBatchRepository _batchRepository;
		private readonly IJobEndMetricsConfiguration _configuration;

		protected JobEndMetricsServiceBase(IBatchRepository batchRepository, IJobEndMetricsConfiguration configuration)
		{
			_batchRepository = batchRepository;
			_configuration = configuration;
		}

		protected async Task WriteRecordsStatisticsAsync(JobEndMetricBase jobEndMetric)
		{
			jobEndMetric.TotalRecordsTransferred = 0;
			jobEndMetric.TotalRecordsTagged = 0;
			jobEndMetric.TotalRecordsFailed = 0;
			jobEndMetric.TotalRecordsRequested = 0;

			IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_configuration.SourceWorkspaceArtifactId, _configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
			foreach (IBatch batch in batches)
			{
				jobEndMetric.TotalRecordsTransferred += batch.TransferredItemsCount;
				jobEndMetric.TotalRecordsTagged += batch.TaggedItemsCount;
				jobEndMetric.TotalRecordsFailed += batch.FailedItemsCount;
				jobEndMetric.TotalRecordsRequested += batch.TotalItemsCount;
			}
		}
	}
}
