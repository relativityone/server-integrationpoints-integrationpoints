﻿using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed class ImportJobFactoryStub : IImportJobFactory
	{
		private readonly ISyncImportBulkArtifactJob _importBulkArtifactJob;
		private readonly ISemaphoreSlim _semaphoreSlim;
		private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
		private readonly ISyncLog _logger;

		public ImportJobFactoryStub(ISyncImportBulkArtifactJob importBulkArtifactJob, ISemaphoreSlim semaphoreSlim, IJobHistoryErrorRepository jobHistoryErrorRepository, ISyncLog logger)
		{
			_importBulkArtifactJob = importBulkArtifactJob;
			_semaphoreSlim = semaphoreSlim;
			_jobHistoryErrorRepository = jobHistoryErrorRepository;
			_logger = logger;
		}

		public Task<Executors.IImportJob> CreateImportJobAsync(ISynchronizationConfiguration configuration, IBatch batch, CancellationToken token)
		{
			Executors.IImportJob importJob = new ImportJob(_importBulkArtifactJob, _semaphoreSlim, _jobHistoryErrorRepository,
				configuration.SourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, _logger);
			return Task.FromResult(importJob);
		}
	}
}