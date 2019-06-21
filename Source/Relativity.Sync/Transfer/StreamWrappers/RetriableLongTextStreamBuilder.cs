using System;
using System.IO;
using System.Threading.Tasks;
using Polly;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	internal sealed class RetriableLongTextStreamBuilder : IRetriableStreamBuilder
	{
		private IObjectManager _objectManager;
		private Stream _stream;

		private const int _MAX_RETRY_ATTEMPTS = 3;
		private const int _WAIT_INTERVAL_IN_SECONDS = 1;

		private readonly int _relativityObjectArtifactId;
		private readonly int _workspaceArtifactId;
		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly ISyncMetrics _syncMetrics;
		private readonly ISyncLog _logger;
		private readonly string _fieldName;
		private readonly IAsyncPolicy<Stream> _retryPolicy;

		public RetriableLongTextStreamBuilder(int workspaceArtifactId, int relativityObjectArtifactId, string fieldName, ISourceServiceFactoryForUser serviceFactory,
			IStreamRetryPolicyFactory streamRetryPolicyFactory, ISyncMetrics syncMetrics, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
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
				_logger.LogError(ex, "Long text stream creation failed for field {fieldName} (object: {relativityObjectArtifactId}) in workspace {workspaceArtifactId}.", _fieldName,
					_relativityObjectArtifactId, _workspaceArtifactId);
				throw;
			}
		}

		private async Task<IObjectManager> GetObjectManagerAsync()
		{
			return _objectManager ?? (_objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false));
		}

		private void OnRetry(int retryAttempt)
		{
			_stream?.Dispose();
			_stream = null;
			_logger.LogWarning("Retrying Kepler Stream creation inside {0}. Attempt {1} of {2}", nameof(SelfRecreatingStream), retryAttempt, _MAX_RETRY_ATTEMPTS);
			_syncMetrics.CountOperation(nameof(RetriableLongTextStreamBuilder), ExecutionStatus.Failed);
		}

		private static bool ShouldRetry(Stream stream)
		{
			return stream == null || !stream.CanRead;
		}

		public void Dispose()
		{
			_objectManager?.Dispose();
			_stream?.Dispose();
		}
	}
}