using System;
using System.IO;
using System.Text;

namespace Relativity.Sync.Transfer.StreamWrappers
{
    internal sealed class AsciiToUnicodeStream : Stream
    {
        private const int _BYTES_PER_UNICODE_CHARACTER = 2;

        internal Stream AsciiStream { get; private set; }

        public override bool CanRead => AsciiStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => AsciiStream.Length * _BYTES_PER_UNICODE_CHARACTER;

        public override long Position
        {
            get => AsciiStream.Position * _BYTES_PER_UNICODE_CHARACTER;
            set => SetLength(value);
        }

        public bool LeaveOpen { get; }

        /// <summary>
        /// Wrapper for the ASCII stream to provide data in Unicode format.
        /// </summary>
        public AsciiToUnicodeStream(Stream asciiStream, bool leaveOpen = false)
        {
            AsciiStream = asciiStream;
            LeaveOpen = leaveOpen;
        }

        public override void Flush()
        {
            throw new NotSupportedException($"${nameof(Flush)} operation is not supported");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException($"${nameof(Seek)} operation is not supported");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException($"${nameof(SetLength)} operation is not supported");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), $"Argument ${nameof(offset)} cannot be lower than zero");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"Argument ${nameof(count)} cannot be lower than zero");
            }

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException($"The size of ${nameof(offset)} and ${nameof(count)} is greater than the size of ${nameof(buffer)}");
            }

            if (AsciiStream == null)
            {
                throw new ObjectDisposedException("The stream is already closed");
            }

            int bytesToReadFromAsciiStream = count / _BYTES_PER_UNICODE_CHARACTER;

            var asciiBuffer = new byte[bytesToReadFromAsciiStream];
            int asciiReadBytes = AsciiStream.Read(asciiBuffer, 0, asciiBuffer.Length);

            byte[] bytesInUnicode = Encoding.Convert(Encoding.ASCII, Encoding.Unicode, asciiBuffer, 0, asciiReadBytes);

            Array.Copy(bytesInUnicode, 0, buffer, offset, bytesInUnicode.Length);

            return bytesInUnicode.Length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (LeaveOpen || !disposing || this.AsciiStream == null)
                {
                    return;
                }

                AsciiStream.Close();
            }
            finally
            {
                this.AsciiStream = null;

                base.Dispose(disposing);
            }
        }
    }
}
