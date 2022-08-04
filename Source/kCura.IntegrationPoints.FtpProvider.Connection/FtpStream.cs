using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.FtpProvider.Connection
{
    public class FtpStream : Stream
    {
        protected readonly FtpWebRequest _request;
        protected readonly Stream _underlyingStream;

        public FtpStream(Stream underlyingStream, FtpWebRequest request)
        {
            _underlyingStream = underlyingStream;
            _request = request;
        }

        public override bool CanRead
        {
            get { return _underlyingStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _underlyingStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _underlyingStream.CanWrite; }
        }

        public override long Length
        {
            get { return _underlyingStream.Length; }
        }

        public override long Position
        {
            get { return _underlyingStream.Position; }
            set { _underlyingStream.Position = value; }
        }

        public override bool CanTimeout
        {
            get { return _underlyingStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return _underlyingStream.ReadTimeout; }
            set { _underlyingStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _underlyingStream.WriteTimeout; }
            set { _underlyingStream.WriteTimeout = value; }
        }

        protected override void Dispose(bool disposing)
        {
            _request.Abort();
            _underlyingStream.Dispose();
        }

        public override void Flush()
        {
            _underlyingStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _underlyingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _underlyingStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _underlyingStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _underlyingStream.Write(buffer, offset, count);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _underlyingStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _underlyingStream.FlushAsync(cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _underlyingStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _underlyingStream.EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _underlyingStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _underlyingStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _underlyingStream.EndWrite(asyncResult);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _underlyingStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return _underlyingStream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            _underlyingStream.WriteByte(value);
        }
    }
}