using System;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
    [TestFixture]
    public class AsciiToUnicodeStreamTests
    {
        private const string _SOME_TEXT = "Some text in English";
        private const int _DEFAULT_BUFFER_LENGTH = 1024;
        private const int _NUMBER_OF_BYTES_PER_CHARACTER = 2;

        [Test]
        public void ReadShouldThrowWhenBufferIsNull()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => stream.Read(null, 0, 1));
        }

        [Test]
        public void ReadShouldThrowWhenOffsetIsLowerThanZero()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(Array.Empty<byte>(), -1, 1));
        }

        [Test]
        public void ReadShouldThrowWhenCountIsLowerThanZero()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(Array.Empty<byte>(), 1, -1));
        }

        [Test]
        public void ReadShouldThrowWhenNotEnoughSpaceInArray()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<ArgumentException>(() => stream.Read(new byte[1], 1, 1));
        }

        [Test]
        public void ReadShouldThrowAfterDisposing()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            stream.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stream.Read(new byte[1], 0, 1));
        }

        [Test]
        public void ReadShouldProperlyDecodeAllAsciiCharacters()
        {
            const int stringLength = 127;
            string asciiTableString = string.Concat(Enumerable.Range(1, stringLength).Select(i => (char) i));
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(asciiTableString);
            var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

            int readBytes = stream.Read(outputBuffer, 0, outputBuffer.Length);

            string result = new string(Encoding.Unicode.GetChars(outputBuffer, 0, readBytes));

            Assert.That(result, Is.EqualTo(asciiTableString));
        }

        [Test]
        public void ReadShouldReturnProperNumberOfBytes()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
            var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

            int readBytes = stream.Read(outputBuffer, 0, outputBuffer.Length);

            Assert.That(readBytes, Is.EqualTo(_SOME_TEXT.Length * _NUMBER_OF_BYTES_PER_CHARACTER));
        }

        [Test]
        public void ReadShouldProperlyLimitNumberOfReadBytes()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
            var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

            const int numberOfBytesToRead = 4;
            int readBytes = stream.Read(outputBuffer, 0, numberOfBytesToRead);

            Assert.That(readBytes, Is.EqualTo(numberOfBytesToRead));
        }

        [Test]
        public void ReadTextShouldHaveHalfLengthOfReadCount()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
            var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

            const int numberOfBytesToRead = 8;
            int readBytes = stream.Read(outputBuffer, 0, numberOfBytesToRead);

            const int numberOfCharactersAfterConversion = 4;
            Assert.That(Encoding.Unicode.GetChars(outputBuffer, 0, readBytes).Length, Is.EqualTo(numberOfCharactersAfterConversion));
        }

        [Test]
        public void LengthIsTwiceTheAsciiContent()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);

            long result = stream.Length;

            Assert.That(result, Is.EqualTo(_SOME_TEXT.Length * _NUMBER_OF_BYTES_PER_CHARACTER));
        }

        [Test]
        public void PositionIsEqualToReadNumberOfBytes()
        {
            const int numberOfBytesToRead = 8;

            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(_SOME_TEXT);
            var outputBuffer = new byte[_DEFAULT_BUFFER_LENGTH];

            stream.Read(outputBuffer, 0, numberOfBytesToRead);

            Assert.That(stream.Position, Is.EqualTo(numberOfBytesToRead));
        }

        private AsciiToUnicodeStream CreateAsciiToUnicodeStream(string content)
        {
            byte[] bytesContent = Encoding.ASCII.GetBytes(content);
            return new AsciiToUnicodeStream(new MemoryStream(bytesContent));
        }

        [Test]
        public void StreamCanRead()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.That(stream.CanRead, Is.True);
        }

        [Test]
        public void StreamCannotWrite()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.That(stream.CanWrite, Is.False);
        }

        [Test]
        public void StreamThrowsOnWrite()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<NotSupportedException>(() => stream.Write(Array.Empty<byte>(), 0, 0));
        }

        [Test]
        public void StreamCannotSeek()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.That(stream.CanSeek, Is.False);
        }

        [Test]
        public void StreamThrowsOnSeek()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        }

        [Test]
        public void StreamThrowsOnSetLength()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<NotSupportedException>(() => stream.SetLength(1));
        }

        [Test]
        public void StreamThrowsOnSetPosition()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<NotSupportedException>(() => stream.Position = 1);
        }

        [Test]
        public void StreamThrowsOnFlush()
        {
            AsciiToUnicodeStream stream = CreateAsciiToUnicodeStream(string.Empty);

            Assert.Throws<NotSupportedException>(() => stream.Flush());
        }

        [Test]
        public void UnderlyingStreamShouldBeClosedWhenDisposing()
        {
            var streamMock = new Mock<Stream>();
            AsciiToUnicodeStream stream = new AsciiToUnicodeStream(streamMock.Object);

            stream.Dispose();

            streamMock.Verify(st => st.Close(), Times.Once);
        }

        [Test]
        public void UnderlyingStreamShouldNotBeClosedOnDisposingWhenLeaveOpenIsTrue()
        {
            var streamMock = new Mock<Stream>();
            AsciiToUnicodeStream stream = new AsciiToUnicodeStream(streamMock.Object, true);

            stream.Dispose();

            streamMock.Verify(st => st.Close(), Times.Never);
        }
    }
}