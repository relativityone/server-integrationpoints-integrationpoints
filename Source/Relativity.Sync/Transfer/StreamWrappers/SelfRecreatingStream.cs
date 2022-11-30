using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.API;

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
        private Lazy<Stream> _innerStream;

        private readonly IRetriableStreamBuilder _streamBuilder;
        private readonly IAPILog _logger;

        public SelfRecreatingStream(IRetriableStreamBuilder streamBuilder, IAPILog logger)
        {
            _streamBuilder = streamBuilder;
            _logger = logger;
            _innerStream = new Lazy<Stream>(() => GetInnerStreamAsync().GetAwaiter().GetResult());
        }

        public override void Flush()
        {
            _innerStream.Value.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Value.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.Value.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Value.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Value.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                if (!_innerStream.Value.CanRead)
                {
                    _innerStream.Value.Dispose();
                    _innerStream = new Lazy<Stream>(() => GetInnerStreamAsync().GetAwaiter().GetResult());
                }
                return _innerStream.Value.CanRead;
            }
        }

        public override bool CanSeek => _innerStream.Value.CanSeek;

        public override bool CanWrite => _innerStream.Value.CanWrite;

        public override long Length => _innerStream.Value.Length;

        public override long Position
        {
            get => _innerStream.Value.Position;
            set => _innerStream.Value.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                if (_innerStream.IsValueCreated)
                {
                    _innerStream.Value.Dispose();
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
