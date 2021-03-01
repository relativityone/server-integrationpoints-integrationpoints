using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Storage
{
	internal sealed class Configuration : IConfiguration
	{
		private readonly IRdoGuidProvider _rdoGuidProvider;
		private readonly IRdoManager _rdoManager;
		private readonly int _workspaceArtifactId;
		private readonly int _syncConfigurationArtifactId;
		private readonly ISyncLog _logger;

		private readonly ISemaphoreSlim _semaphoreSlim;

		private SyncConfigurationRdo _configuration;
		private RdoTypeInfo _configurationTypeInfo;

		private Configuration(SyncJobParameters syncJobParameters, IRdoGuidProvider rdoGuidProvider, IRdoManager rdoManager, ISyncLog logger, ISemaphoreSlim semaphoreSlim)
		{
			_rdoGuidProvider = rdoGuidProvider;
			_rdoManager = rdoManager;
			_workspaceArtifactId = syncJobParameters.WorkspaceId;
			_syncConfigurationArtifactId = syncJobParameters.SyncConfigurationArtifactId;
			_logger = logger;
			_semaphoreSlim = semaphoreSlim;
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
		
		public async Task UpdateFieldValueAsync<T>(Expression<Func<SyncConfigurationRdo, object>> memberExpression, T value)
		{
			Guid guid = _rdoGuidProvider.GetGuidFromFieldExpression(memberExpression);

			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			try
			{
				var requestRdo = new SyncConfigurationRdo
				{
					ArtifactId = _syncConfigurationArtifactId
				};

				PropertyInfo propertyInfo = _configurationTypeInfo.Fields[guid].PropertyInfo;
				propertyInfo.SetValue(requestRdo, value);

				await _rdoManager.SetValuesAsync(_workspaceArtifactId, requestRdo, memberExpression)
					.ConfigureAwait(false);
				
				propertyInfo.SetValue(_configuration, value);
			}
			finally
			{
				_semaphoreSlim.Release();
			}
		}

		private async Task ReadAsync()
		{
			_logger.LogVerbose("Reading Sync Configuration {artifactId}.", _syncConfigurationArtifactId);
			_configurationTypeInfo = _rdoGuidProvider.GetValue<SyncConfigurationRdo>();


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
			ISemaphoreSlim semaphoreSlim, IRdoGuidProvider rdoGuidProvider, IRdoManager rdoManager)
		{
			Configuration configuration = new Configuration(syncJobParameters,rdoGuidProvider, rdoManager, logger, semaphoreSlim);
			await configuration.ReadAsync().ConfigureAwait(false);
			return configuration;
		}

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}