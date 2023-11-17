using System.IO;
using Relativity.Storage;

namespace kCura.IntegrationPoint.Tests.Core.CustomProviderHelpers
{
    public class FakeStorageStream : StorageStream
    {
        private readonly FileStream _stream;

        public FakeStorageStream(FileStream stream)
        {
            _stream = stream;
        }

        public override string StoragePath => _stream.Name;

        public override StorageInterface StorageInterface => StorageInterface.Adls2;

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Position;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            _stream.Dispose();
            base.Dispose(disposing);
        }
    }
}
