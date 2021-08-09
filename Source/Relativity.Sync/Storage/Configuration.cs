using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Storage
{
	internal sealed class Configuration : IConfiguration
	{
		private readonly IRdoManager _rdoManager;
		private readonly int _workspaceArtifactId;
		private readonly int _syncConfigurationArtifactId;
		private readonly ISyncLog _logger;

		private readonly ISemaphoreSlim _semaphoreSlim;

		private SyncConfigurationRdo _configuration;

		private Configuration(SyncJobParameters syncJobParameters, IRdoManager rdoManager, ISemaphoreSlim semaphoreSlim, ISyncLog logger)
		{
			_rdoManager = rdoManager;
			_workspaceArtifactId = syncJobParameters.WorkspaceId;
			_syncConfigurationArtifactId = syncJobParameters.SyncConfigurationArtifactId;
			_semaphoreSlim = semaphoreSlim;
			_logger = logger;
		}

		public T GetFieldValue<T>(Func<SyncConfigurationRdo, T> valueGetter)
		{
			_semaphoreSlim.Wait();

			try
			{
				return valueGetter(_configuration);
			}
			finally
			{
				_semaphoreSlim.Release();
			}
		}
		
		public async Task UpdateFieldValueAsync<T>(Expression<Func<SyncConfigurationRdo, T>> memberExpression, T value)
		{
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			try
			{
				await _rdoManager.SetValueAsync(_workspaceArtifactId, _configuration, memberExpression, value)
					.ConfigureAwait(false);
			}
			finally
			{
				_semaphoreSlim.Release();
			}
		}

		private async Task ReadAsync()
		{
			_logger.LogVerbose("Reading Sync Configuration {artifactId}.", _syncConfigurationArtifactId);

			_configuration = await _rdoManager
				.GetAsync<SyncConfigurationRdo>(_workspaceArtifactId, _syncConfigurationArtifactId)
				.ConfigureAwait(false);

			if (_configuration == null)
			{
				_logger.LogError("Configuration with Id {artifactId} does not exist in workspace {workspaceId}",
					_syncConfigurationArtifactId, _workspaceArtifactId);
				throw new SyncException(
					$"Configuration with Id {_syncConfigurationArtifactId} does not exist in workspace {_workspaceArtifactId}");
			}
		}

		public static async Task<IConfiguration> GetAsync(SyncJobParameters syncJobParameters, ISyncLog logger,
			ISemaphoreSlim semaphoreSlim, IRdoManager rdoManager)
		{
			Configuration configuration = new Configuration(syncJobParameters, rdoManager, semaphoreSlim, logger);
			await configuration.ReadAsync().ConfigureAwait(false);
			return configuration;
		}

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}