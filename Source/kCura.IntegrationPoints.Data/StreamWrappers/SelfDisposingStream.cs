using System;
using System.IO;
using System.Runtime.CompilerServices;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.StreamWrappers
{
    /// <summary>
    /// Import API does not provide any way to dispose streams we're passing.
    /// This stream will dispose itself when read.
    /// It's working under assumption that IAPI won't acces this stream twice.
    /// </summary>
    internal sealed class SelfDisposingStream : Stream
    {
        private readonly IAPILog _logger;

        internal Stream InnerStream { get; private set; }

        public SelfDisposingStream(
            Stream stream,
            IAPILog logger)
        {
            _logger = logger.ForContext<SelfDisposingStream>();
            InnerStream = stream;
        }

        public override void Flush()
        {
            LogUnsupportedCall();
            InnerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            LogUnsupportedCall();
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            LogUnsupportedCall();
            InnerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                int readBytes = InnerStream.Read(buffer, offset, count);
                if (readBytes == 0)
                {
                    DisposeInnerStream();
                }

                return readBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading {0} bytes from {1} at index: {2}", count, nameof(SelfDisposingStream), offset);
                throw;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            LogUnsupportedCall();
            InnerStream.Write(buffer, offset, count);
        }

        public override bool CanRead => InnerStream.CanRead;

        public override bool CanSeek => InnerStream.CanSeek;

        public override bool CanWrite => InnerStream.CanWrite;

        public override long Length => InnerStream.Length;

        public override long Position
        {
            get { return InnerStream.Position; }
            set { InnerStream.Position = value; }
        }

        private void DisposeInnerStream()
        {
            InnerStream?.Dispose();
            InnerStream = null;
        }

        private void LogUnsupportedCall([CallerMemberName] string callerMemberName = "")
        {
            _logger.LogWarning("Unsupported operation on {0} ({1})", nameof(SelfDisposingStream), callerMemberName);
        }
    }
}
