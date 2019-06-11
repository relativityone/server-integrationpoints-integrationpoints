using System;
using System.IO;
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
		private ISyncPolicy<Stream> _getStreamRetryPolicy;

		private bool _disposed = false;

		private const int _MAX_RETRY_ATTEMPTS = 3;
		private const int _WAIT_INTERVAL_IN_SECONDS = 1;

		private readonly Func<Stream> _getStreamFunction;
		private readonly ISyncLog _logger;

		internal Stream InnerStream { get; private set; }

		public SelfRecreatingStream(
			Func<Stream> getStreamFunction,
			IStreamRetryPolicyFactory streamRetryPolicyFactory,
			ISyncLog logger)
		{
			_logger = logger;
			_getStreamFunction = getStreamFunction;
			_getStreamRetryPolicy = streamRetryPolicyFactory.Create(
				OnRetry,
				_MAX_RETRY_ATTEMPTS,
				TimeSpan.FromSeconds(_WAIT_INTERVAL_IN_SECONDS));

			InnerStream = _getStreamRetryPolicy.Execute(GetInnerStream);
		}

		public override void Flush()
		{
			InnerStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return InnerStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			InnerStream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return InnerStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			InnerStream.Write(buffer, offset, count);
		}

		public override bool CanRead
		{
			get
			{
				if (!InnerStream.CanRead)
				{
					InnerStream = _getStreamRetryPolicy.Execute(GetInnerStream);
				}

				return InnerStream.CanRead;
			}
		}

		public override bool CanSeek => InnerStream.CanSeek;

		public override bool CanWrite => InnerStream.CanWrite;

		public override long Length => InnerStream.Length;

		public override long Position
		{
			get => InnerStream.Position;
			set => InnerStream.Position = value;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_getStreamRetryPolicy = null; // See note at top
				InnerStream?.Dispose();
			}
		}

		private Stream GetInnerStream()
		{
			try
			{
				Stream stream = _getStreamFunction.Invoke();
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
			InnerStream?.Dispose();
			_logger.LogWarning("Retrying Kepler Stream creation inside {0}. Attempt {1} of {2}", nameof(SelfRecreatingStream), retryAttempt, _MAX_RETRY_ATTEMPTS);
		}
	}
}