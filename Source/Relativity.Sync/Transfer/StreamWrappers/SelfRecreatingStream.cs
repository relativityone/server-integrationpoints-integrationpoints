using System;
using System.IO;
using System.Threading.Tasks;

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
		private bool _disposed;

		private readonly IRetriableStreamBuilder _streamBuilder;
		private readonly ISyncLog _logger;

		internal Lazy<Stream> InnerStream { get; private set; }

		public SelfRecreatingStream(IRetriableStreamBuilder streamBuilder, ISyncLog logger)
		{
			_streamBuilder = streamBuilder;
			_logger = logger;
			InnerStream = new Lazy<Stream>(() => GetInnerStreamAsync().GetAwaiter().GetResult());
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
					InnerStream = new Lazy<Stream>(() => GetInnerStreamAsync().GetAwaiter().GetResult());
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
				if (InnerStream.IsValueCreated)
				{
					InnerStream.Value.Dispose();
				}
			}
		}

		private async Task<Stream> GetInnerStreamAsync()
		{
			try
			{
				return await _streamBuilder.GetStreamAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unable to create inner stream inside {0}", nameof(SelfRecreatingStream));
				throw;
			}
		}
	}
}