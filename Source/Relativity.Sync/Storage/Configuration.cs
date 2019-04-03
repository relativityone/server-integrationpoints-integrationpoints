using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Storage
{
	internal sealed class Configuration : IConfiguration
	{
		private readonly ISourceServiceFactoryForAdmin _serviceFactory;
		private readonly int _workspaceArtifactId;
		private readonly int _syncConfigurationArtifactId;
		private readonly ISyncLog _logger;

		private readonly ISemaphoreSlim _semaphoreSlim;

		private readonly Dictionary<Guid, object> _cache = new Dictionary<Guid, object>();

		private static readonly Guid ConfigurationObjectTypeGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		private Configuration(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, ISyncLog logger, ISemaphoreSlim semaphoreSlim)
		{
			_serviceFactory = serviceFactory;
			_workspaceArtifactId = workspaceArtifactId;
			_syncConfigurationArtifactId = syncConfigurationArtifactId;
			_logger = logger;
			_semaphoreSlim = semaphoreSlim;
		}

		public T GetFieldValue<T>(Guid guid)
		{
			_semaphoreSlim.Wait();
			try
			{
				if (!_cache.ContainsKey(guid))
				{
					_logger.LogError("Requesting unknown field with GUID {guid}.", guid);
					throw new ArgumentException($"Field with GUID {guid} does not exist in cache.");
				}

				object value = _cache[guid];
				if (value == null)
				{
					_logger.LogVerbose("Returning default value for field with GUID {guid}.", guid);
					return default(T);
				}

				return (T) value;
			}
			finally
			{
				_semaphoreSlim.Release();
			}
		}

		public async Task UpdateFieldValueAsync<T>(Guid guid, T value)
		{
			if (!_cache.ContainsKey(guid))
			{
				_logger.LogError("Updating unknown field with GUID {guid}.", guid);
				throw new ArgumentException($"Field with GUID {guid} does not exist in cache.");
			}

			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			try
			{
				using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					UpdateRequest request = new UpdateRequest
					{
						Object = new RelativityObjectRef
						{
							ArtifactID = _syncConfigurationArtifactId
						},
						FieldValues = new[]
						{
							new FieldRefValuePair
							{
								Field = new FieldRef
								{
									Guid = guid
								},
								Value = value
							}
						}
					};
					await objectManager.UpdateAsync(_workspaceArtifactId, request).ConfigureAwait(false);
					_cache[guid] = value;
				}
			}
			finally
			{
				_semaphoreSlim.Release();
			}
		}

		private async Task ReadAsync()
		{
			_logger.LogVerbose("Reading Sync Configuration {artifactId}.", _syncConfigurationArtifactId);
			using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = ConfigurationObjectTypeGuid
					},
					Condition = $"(('Artifact ID' == {_syncConfigurationArtifactId}))",
					Fields = new[]
					{
						new FieldRef
						{
							Name = "*"
						}
					}
				};
				const int start = 1;
				const int length = 1;
				QueryResult result = await objectManager.QueryAsync(_workspaceArtifactId, request, start, length).ConfigureAwait(false);

				if (result.TotalCount == 0)
				{
					throw new SyncException($"Cannot find Sync Configuration with given artifact ID {_syncConfigurationArtifactId}.");
				}

				foreach (FieldValuePair fieldValuePair in result.Objects[0].FieldValues)
				{
					foreach (Guid guid in fieldValuePair.Field.Guids)
					{
						_logger.LogVerbose("Field with guid {guid} read.", guid);
						_cache.Add(guid, fieldValuePair.Value);
					}
				}
			}
		}

		public static async Task<IConfiguration> GetAsync(ISourceServiceFactoryForAdmin serviceFactory, int workspaceArtifactId, int syncConfigurationArtifactId, ISyncLog logger,
			ISemaphoreSlim semaphoreSlim)
		{
			Configuration configuration = new Configuration(serviceFactory, workspaceArtifactId, syncConfigurationArtifactId, logger, semaphoreSlim);
			await configuration.ReadAsync().ConfigureAwait(false);
			return configuration;
		}

		public void Dispose()
		{
			_semaphoreSlim?.Dispose();
		}
	}
}