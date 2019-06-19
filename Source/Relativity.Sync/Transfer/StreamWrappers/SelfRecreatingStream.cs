using System;
using System.IO;
using System.Threading.Tasks;
using Polly;

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

		private readonly Func<Task<Stream>> _getStreamFunction;
		private readonly ISyncLog _logger;

		internal Lazy<Stream> InnerStream { get; private set; }

		public SelfRecreatingStream(
			Func<Task<Stream>> getStreamFunction,
			IStreamRetryPolicyFactory streamRetryPolicyFactory,
			ISyncLog logger)
		{
			_logger = logger;
			_getStreamFunction = getStreamFunction;
			_getStreamRetryPolicy = streamRetryPolicyFactory.Create(
				OnRetry,
				_MAX_RETRY_ATTEMPTS,
				TimeSpan.FromSeconds(_WAIT_INTERVAL_IN_SECONDS));

			InnerStream = _getStreamRetryPolicy.ExecuteAsync(GetInnerStream).GetAwaiter().GetResult();
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
					InnerStream = _getStreamRetryPolicy.ExecuteAsync(GetInnerStream).GetAwaiter().GetResult();
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
				InnerStream.Value?.Dispose();
			}
		}

		private async Task<Stream> GetInnerStream()
		{
			try
			{
				Stream stream = await _getStreamFunction().ConfigureAwait(false);
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
			InnerStream.Value?.Dispose();
			_logger.LogWarning("Retrying Kepler Stream creation inside {0}. Attempt {1} of {2}", nameof(SelfRecreatingStream), retryAttempt, _MAX_RETRY_ATTEMPTS);
		}
	}
}