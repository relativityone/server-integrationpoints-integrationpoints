using System;
using System.IO;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	/// <summary>
	/// Needed to directly check if a class disposes a <see cref="Stream"/>,
	/// since <see cref="Stream.Dispose"/> is not virtual.
	/// </summary>
	internal sealed class DisposalCheckStream : Stream
	{
		public bool IsDisposed { get; set; }

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override bool CanRead { get; }
		public override bool CanSeek { get; }
		public override bool CanWrite { get; }
		public override long Length { get; }
		public override long Position { get; set; }

		protected override void Dispose(bool disposing)
		{
			IsDisposed = true;
		}
	}
}
