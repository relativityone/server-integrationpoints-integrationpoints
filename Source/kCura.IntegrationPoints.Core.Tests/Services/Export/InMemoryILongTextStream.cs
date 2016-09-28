using System.IO;
using System.Text;
using kCura.EDDS.DocumentCompareGateway;
using kCura.IntegrationPoints.Core.Services.Exporter;

namespace kCura.IntegrationPoints.Core.Tests.Services.Export
{
	public class InMemoryILongTextStreamFactory : IILongTextStreamFactory
	{
		private readonly string _context;
		private readonly bool _isUnicode;

		public InMemoryILongTextStreamFactory(string content, bool isUnicode)
		{
			_isUnicode = isUnicode;
			_context = content;
		}

		public ILongTextStream CreateLongTextStream(int documentArtifactId, int fieldArtifactId)
		{
			return new InMemoryILongTextStream(_context, _isUnicode);
		}
	}

	public class InMemoryILongTextStream : ILongTextStream
	{
		private readonly MemoryStream _memoryStream;

		public InMemoryILongTextStream(string content, bool isUniCode)
		{
			byte[] bytes = isUniCode ? Encoding.Unicode.GetBytes(content) : Encoding.ASCII.GetBytes(content);
			_memoryStream = new MemoryStream(bytes);
		}

		public override bool IsUnicode
		{
			get { return true; }
		}

		public override bool CanRead
		{
			get { return _memoryStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _memoryStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _memoryStream.CanWrite; }
		}

		public override void Flush()
		{
			_memoryStream.Flush();
		}

		public override long Length
		{
			get { return _memoryStream.Length; }
		}

		public override long Position
		{
			get { return _memoryStream.Position; }
			set { _memoryStream.Position = value; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _memoryStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			return _memoryStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_memoryStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_memoryStream.Write(buffer, offset, count);
		}
	}
}