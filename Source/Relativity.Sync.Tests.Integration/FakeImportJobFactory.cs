using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed class FakeImportJobFactory : IImportJobFactory
	{
		private readonly IImportBulkArtifactJob _importBulkArtifactJob;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISyncLog _logger;

		public FakeImportJobFactory(IImportBulkArtifactJob importBulkArtifactJob, ISemaphoreSlim semaphoreSlim, IJobHistoryErrorRepository jobHistoryErrorRepository, ISyncLog logger)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
			_semaphoreSlim = semaphoreSlim;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_logger = logger;
		}

		public Executors.IImportJob CreateImportJob(ISynchronizationConfiguration configuration, IBatch batch)
		{
			return new ImportJob(_importBulkArtifactJob, _semaphoreSlim, _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryTagArtifactId, _logger);
		}
	}
}