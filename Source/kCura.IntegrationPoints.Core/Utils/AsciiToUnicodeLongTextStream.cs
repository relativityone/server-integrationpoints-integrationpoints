using System;
using System.IO;
using System.Text;
using kCura.EDDS.DocumentCompareGateway;

namespace kCura.IntegrationPoints.Core.Utils
{	
	public class AsciiToUnicodeLongTextStream : ILongTextStream
	{
		private Stream _asciiStream;

		private const int _BYTES_PER_UNICODE_CHARACTER = 2;

		public override bool CanRead => _asciiStream.CanRead;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => _asciiStream.Length * _BYTES_PER_UNICODE_CHARACTER;

		public override long Position
		{
			get { return _asciiStream.Position * _BYTES_PER_UNICODE_CHARACTER; }
			set { SetLength(value); }
		}

		public override bool IsUnicode => true;

		public bool LeaveOpen { get; }

		/// <summary>
		/// Wrapper for the ASCII stream to provide data in Unicode format. 
		/// </summary>
		public AsciiToUnicodeLongTextStream(Stream asciiStream, bool leaveOpen = false)
		{
			_asciiStream = asciiStream;
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
				throw new ArgumentOutOfRangeException($"Argument ${nameof(offset)} cannot be lower than zero", nameof(offset));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException($"Argument ${nameof(count)} cannot be lower than zero", nameof(count));
			}

			if (buffer.Length < offset + count)
			{
				throw new ArgumentException($"The size of ${nameof(offset)} and ${nameof(count)} is greater than the size of ${nameof(buffer)}");
			}

			if (_asciiStream == null)
			{
				throw new ObjectDisposedException("The stream is already closed");
			}

			int bytesToReadFromAsciiStream = count / _BYTES_PER_UNICODE_CHARACTER;
			
			var asciiBuffer = new byte[bytesToReadFromAsciiStream];
			int asciiReadBytes = _asciiStream.Read(asciiBuffer, 0, asciiBuffer.Length);

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
				if (LeaveOpen || !disposing || this._asciiStream == null)
				{
					return;
				}

				_asciiStream.Close();
			}
			finally
			{
				this._asciiStream = null;

				base.Dispose(disposing);
			}
		}
	}
}