using System;
using System.IO;
using System.Threading.Tasks;
using Polly;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal sealed class RetriableLongTextStreamBuilder : IRetriableStreamBuilder
	{
		private IObjectManager _objectManager;

		private const int _MAX_RETRY_ATTEMPTS = 3;
		private const int _WAIT_INTERVAL_IN_SECONDS = 1;
		private const string _STREAM_RETRY_COUNT_BUCKET_NAME = "Relativity.Sync.LongTextStreamBuilder.Retry.Count";

		private readonly int _relativityObjectArtifactId;
		private readonly int _workspaceArtifactId;
		private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
		private readonly ISyncMetrics _syncMetrics;
		private readonly ISyncLog _logger;
		private readonly string _fieldName;
		private readonly IAsyncPolicy<Stream> _retryPolicy;

		public RetriableLongTextStreamBuilder(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName, ISourceServiceFactoryForUser serviceFactoryForUser,
			IStreamRetryPolicyFactory streamRetryPolicyFactory, ISyncMetrics syncMetrics, ISyncLog logger)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_syncMetrics = syncMetrics;
			_logger = logger;
			_workspaceArtifactId = workspaceArtifactId;
			_relativityObjectArtifactId = relativityObjectArtifactId;
			_fieldName = fieldName;
			_retryPolicy = streamRetryPolicyFactory.Create(ShouldRetry, OnRetry, _MAX_RETRY_ATTEMPTS, TimeSpan.FromSeconds(_WAIT_INTERVAL_IN_SECONDS));
		}

		public async Task<Stream> GetStreamAsync()
		{
			return await _retryPolicy.ExecuteAsync(CreateLongTextStream).ConfigureAwait(false);
		}

		private async Task<Stream> CreateLongTextStream()
		{
			try
			{
				IObjectManager objectManager = await GetObjectManagerAsync().ConfigureAwait(false);
				var exportObject = new RelativityObjectRef {ArtifactID = _relativityObjectArtifactId};
				var fieldRef = new FieldRef {Name = _fieldName};
				IKeplerStream keplerStream = await objectManager.StreamLongTextAsync(_workspaceArtifactId, exportObject, fieldRef).ConfigureAwait(false);
				return await keplerStream.GetStreamAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Long text stream creation failed for field (object: {relativityObjectArtifactId}) in workspace {workspaceArtifactId}.",
					_relativityObjectArtifactId, _workspaceArtifactId);
				throw;
			}
		}

		private async Task<IObjectManager> GetObjectManagerAsync()
		{
			return _objectManager ?? (_objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false));
		}

		private void OnRetry(Stream stream, Exception exception, int retryAttempt)
		{
			stream?.Dispose();
			_logger.LogWarning(exception, "Retrying Kepler Stream creation inside {SelfRecreatingStream}. Attempt {retryAttempt} of {maxNumberOfRetries}",
				nameof(SelfRecreatingStream), retryAttempt, _MAX_RETRY_ATTEMPTS);
			
			_syncMetrics.Send(new StreamRetryMetric
			{
				RetryCounter = Counter.Increment
			});
		}

		private static bool ShouldRetry(Stream stream)
		{
			return stream == null || !stream.CanRead;
		}

		public void Dispose()
		{
			_objectManager?.Dispose();
		}
	}
}