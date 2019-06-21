using System;
using System.IO;
using System.Threading.Tasks;
using Polly;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Transfer.StreamWrappers
{
	/// <summary>
	/// This stream wraps a function that retrieves inner stream and
	/// attempts to retrieve an inner stream again if it is unreadable
	/// or if an inner stream retrieval method throws an exception.
	/// Is is required due to the Object Manager GetLongTextStream method,
	/// which sometimes returns an unreadable stream.
	/// </summary>
	internal sealed class SelfRecreatingStream : Stream
	{
		// NOTE: This can't be readonly - we create a circular reference by
		// passing in an instance method to the factory, so we need to set
		// this field to null on dispose.
		private IAsyncPolicy<Stream> _getStreamRetryPolicy;

		private bool _disposed;

		private const int _MAX_RETRY_ATTEMPTS = 3;
		private const int _WAIT_INTERVAL_IN_SECONDS = 1;

		private readonly Lazy<IObjectManager> _objectManager;
		private readonly Func<IObjectManager, Task<Stream>> _streamFactory;
		private readonly ISyncLog _logger;
		private readonly Func<Stream> _innerStreamValueFactory;

		internal Lazy<Stream> InnerStream { get; private set; }

		public SelfRecreatingStream(
			ISourceServiceFactoryForUser serviceFactory,
			Func<IObjectManager, Task<Stream>> streamFactory,
			IStreamRetryPolicyFactory streamRetryPolicyFactory,
			ISyncLog logger
			)
		{
			_objectManager = new Lazy<IObjectManager>(() => serviceFactory.CreateProxyAsync<IObjectManager>().GetAwaiter().GetResult());
			_logger = logger;
			_streamFactory = streamFactory;
			_getStreamRetryPolicy = streamRetryPolicyFactory.Create(
				OnRetry,
				_MAX_RETRY_ATTEMPTS,
				TimeSpan.FromSeconds(_WAIT_INTERVAL_IN_SECONDS)
				);
			_innerStreamValueFactory = () => _getStreamRetryPolicy.ExecuteAsync(GetInnerStream).GetAwaiter().GetResult();

			InnerStream = new Lazy<Stream>(_innerStreamValueFactory);
		}

		public override void Flush()
		{
			InnerStream.Value.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return InnerStream.Value.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			InnerStream.Value.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return InnerStream.Value.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			InnerStream.Value.Write(buffer, offset, count);
		}

		public override bool CanRead
		{
			get
			{
				if (!InnerStream.Value.CanRead)
				{
					InnerStream = new Lazy<Stream>(_innerStreamValueFactory);
				}

				return InnerStream.Value.CanRead;
			}
		}

		public override bool CanSeek => InnerStream.Value.CanSeek;

		public override bool CanWrite => InnerStream.Value.CanWrite;

		public override long Length => InnerStream.Value.Length;

		public override long Position
		{
			get => InnerStream.Value.Position;
			set => InnerStream.Value.Position = value;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_getStreamRetryPolicy = null; // See note at top
				if (InnerStream.IsValueCreated)
				{
					InnerStream.Value.Dispose();
				}
				if (_objectManager.IsValueCreated)
				{
					_objectManager.Value.Dispose();
				}
			}
		}

		private async Task<Stream> GetInnerStream()
		{
			try
			{
				Stream stream = await _streamFactory(_objectManager.Value).ConfigureAwait(false);
				return stream;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unable to create inner stream inside {0}", nameof(SelfRecreatingStream));
				throw;
			}
		}

		private void OnRetry(int retryAttempt)
		{
			if (InnerStream.IsValueCreated)
			{
				InnerStream.Value.Dispose();
			}
			_logger.LogWarning("Retrying Kepler Stream creation inside {0}. Attempt {1} of {2}", nameof(SelfRecreatingStream), retryAttempt, _MAX_RETRY_ATTEMPTS);
		}
	}
}