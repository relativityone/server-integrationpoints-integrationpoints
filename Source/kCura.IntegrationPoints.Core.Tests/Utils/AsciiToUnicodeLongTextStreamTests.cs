using System;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Core.Utils;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Utils
{
	[TestFixture]
	public class AsciiToUnicodeLongTextStreamTests
	{
		private const string _SOME_TEXT = "Some text in English";
		private const int _DEFAULT_BUFFER_LENGTH = 1024;

		[Test]
		public void ReadShouldThrowWhenBufferIsNull()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			// ReSharper disable once AssignNullToNotNullAttribute
			Assert.Throws<ArgumentNullException>(() => stream.Read(null, 0, 1));
		}

		[Test]
		public void ReadShouldThrowWhenOffsetIsLowerThanZero()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(new byte[0], -1, 1));
		}

		[Test]
		public void ReadShouldThrowWhenCountIsLowerThanZero()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(new byte[0], 1, -1));
		}

		[Test]
		public void ReadShouldThrowWhenNotEnoughSpaceInArray()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<ArgumentException>(() => stream.Read(new byte[1], 1, 1));
		}

		[Test]
		public void ReadShouldThrowAfterDisposing()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => stream.Read(new byte[1], 0, 1));
		}

		[Test]
		public void ReadShouldProperlyDecodeAllAsciiCharacters()
		{
			string asciiTableString = string.Concat(Enumerable.Range(1, 127).Select(i => (char) i));
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(asciiTableString);
			var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

			int readBytes = stream.Read(outputBuffer, 0, outputBuffer.Length);

			string result = new string(Encoding.Unicode.GetChars(outputBuffer, 0, readBytes));

			Assert.That(result, Is.EqualTo(asciiTableString));
		}

		[Test]
		public void ReadShouldReturnProperNumberOfBytes()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
			var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

			int readBytes = stream.Read(outputBuffer, 0, outputBuffer.Length);

			Assert.That(readBytes, Is.EqualTo(_SOME_TEXT.Length * 2));
		}

		[Test]
		public void ReadShouldProperlyLimitNumberOfReadBytes()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
			var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

			int readBytes = stream.Read(outputBuffer, 0, 4);

			Assert.That(readBytes, Is.EqualTo(4));
		}

		[Test]
		public void ReadTextShouldHaveHalfLengthOfReadCount()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
			var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

			int readBytes = stream.Read(outputBuffer, 0, 8);

			Assert.That(Encoding.Unicode.GetChars(outputBuffer, 0, readBytes).Length, Is.EqualTo(4));
		}

		[Test]
		public void LengthIsTwiceTheAsciiContent()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
			
			long result = stream.Length;

			Assert.That(result, Is.EqualTo(_SOME_TEXT.Length * 2));
		}

		[Test]
		public void PositionIsEqualToReadNumberOfBytes()
		{
			const int numberOfBytesToRead = 8;

			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
			var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];			

			stream.Read(outputBuffer, 0, numberOfBytesToRead);

			Assert.That(stream.Position, Is.EqualTo(numberOfBytesToRead));
		}

		private AsciiToUnicodeLongTextStream CreateAsciiToUnicodeStream(string content)
		{
			byte[] bytesContent = Encoding.ASCII.GetBytes(content);
			return new AsciiToUnicodeLongTextStream(new MemoryStream(bytesContent));
		}

		[Test]
		public void StreamIsUnicode()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.That(stream.IsUnicode, Is.True);
		}

		[Test]
		public void StreamCanRead()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.That(stream.CanRead, Is.True);
		}

		[Test]
		public void StreamCannotWrite()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.That(stream.CanWrite, Is.False);
		}

		[Test]
		public void StreamThrowsOnWrite()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<NotSupportedException>(() => stream.Write(new byte[0], 0, 0));
		}

		[Test]
		public void StreamCannotSeek()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.That(stream.CanSeek, Is.False);
		}

		[Test]
		public void StreamThrowsOnSeek()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
		}

		[Test]
		public void StreamThrowsOnSetLength()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<NotSupportedException>(() => stream.SetLength(1));
		}

		[Test]
		public void StreamThrowsOnSetPosition()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<NotSupportedException>(() => stream.Position = 1);
		}

		[Test]
		public void StreamThrowsOnFlush()
		{
			AsciiToUnicodeLongTextStream stream = CreateAsciiToUnicodeStream(string.Empty);

			Assert.Throws<NotSupportedException>(() => stream.Flush());
		}

		[Test]
		public void UnderlyingStreamShouldBeClosedWhenDisposing()
		{
			var streamMock = new Mock<Stream>();
			AsciiToUnicodeLongTextStream stream = new AsciiToUnicodeLongTextStream(streamMock.Object);

			stream.Dispose();

			streamMock.Verify(st => st.Close(), Times.Once);
		}

		[Test]
		public void UnderlyingStreamShouldNotBeClosedOnDisposingWhenLeaveOpenIsTrue()
		{
			var streamMock = new Mock<Stream>();
			AsciiToUnicodeLongTextStream stream = new AsciiToUnicodeLongTextStream(streamMock.Object, true);

			stream.Dispose();

			streamMock.Verify(st => st.Close(), Times.Never);
		}
	}
}