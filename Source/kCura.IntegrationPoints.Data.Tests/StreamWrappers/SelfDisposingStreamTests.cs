using System.IO;
using FluentAssertions;
using kCura.IntegrationPoints.Data.StreamWrappers;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.StreamWrappers
{
    [TestFixture, Category("Unit")]
    public class SelfDisposingStreamTests
    {
        private Mock<Stream> _streamMock;
        private Mock<IAPILog> _loggerMock;
        private SelfDisposingStream _selfDisposingStream;

        [SetUp]
        public void SetUp()
        {
            _streamMock = new Mock<Stream>();
            _loggerMock = new Mock<IAPILog>();
            _loggerMock.Setup(x => x.ForContext<SelfDisposingStream>()).Returns(_loggerMock.Object);
            _selfDisposingStream = new SelfDisposingStream(_streamMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Flush_ShouldInvokeFlushOnInnerStreamAndLogWarning()
        {
            // act
            _selfDisposingStream.Flush();

            // assert
            _streamMock.Verify(x => x.Flush(), Times.Once);
            ShouldLogWarning();
        }

        [Test]
        public void Seek_ShouldInvokeSeekOnInnerStreamAndLogWarning()
        {
            // arrange
            const long offset = 0;
            const long expectedResult = 2;
            _streamMock.Setup(x => x.Seek(offset, SeekOrigin.Begin)).Returns(expectedResult);

            // act
            long result = _selfDisposingStream.Seek(offset, SeekOrigin.Begin);

            // assert
            _streamMock.Verify(x => x.Seek(offset, SeekOrigin.Begin), Times.Once);
            result.Should().Be(expectedResult);
            ShouldLogWarning();
        }

        [Test]
        public void SetLength_ShouldInvokeSetLengthOnInnerStreamAndLogWarning()
        {
            // act
            const long length = 4;
            _selfDisposingStream.SetLength(length);

            // assert
            _streamMock.Verify(x => x.SetLength(length), Times.Once);
            ShouldLogWarning();
        }

        [Test]
        public void Read_ShouldInvokeReadOnInnerStream_WhenBytesAreRead()
        {
            // arrange
            byte[] buffer = { };
            const int offset = 4;
            const int count = 6;
            const int expectedResult = 8;
            _streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(expectedResult);

            // act
            int result = _selfDisposingStream.Read(buffer, offset, count);

            // assert
            _streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
            result.Should().Be(expectedResult);
            ShouldNotLogWarning();
        }

        [Test]
        public void Read_ShouldInvokeReadOnInnerStreamAndDispose_WhenNoBytesAreRead()
        {
            // arrange
            byte[] buffer = { };
            const int offset = 10;
            const int count = 12;
            _streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(0);

            // act
            int result = _selfDisposingStream.Read(buffer, offset, count);

            // assert
            _streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
            _streamMock.Verify(x => x.Close(), Times.Once);
            result.Should().Be(0);
            ShouldNotLogWarning();
        }

        [Test]
        public void Write_ShouldInvokeWriteOnInnerStreamAndLogWarning()
        {
            // arrange
            byte[] buffer = {};
            const int offset = 14;
            const int count = 16;

            // act
            _selfDisposingStream.Write(buffer, offset, count);

            // assert
            _streamMock.Verify(x => x.Write(buffer, offset, count), Times.Once);
            ShouldLogWarning();
        }

        [Test]
        public void CanReadGetter_ShouldInvokeReadGetterOnInnerStream()
        {
            // arrange
            _streamMock.Setup(x => x.CanRead).Returns(true);

            // act
            bool result = _selfDisposingStream.CanRead;

            // assert
            _streamMock.VerifyGet(x => x.CanRead, Times.Once);
            result.Should().BeTrue();
        }

        [Test]
        public void CanSeekGetter_ShouldInvokeCanSeekGetterOnInnerStream()
        {
            // arrange
            _streamMock.Setup(x => x.CanSeek).Returns(true);

            // act
            bool result = _selfDisposingStream.CanSeek;

            // assert
            _streamMock.VerifyGet(x => x.CanSeek, Times.Once);
            result.Should().BeTrue();
        }

        [Test]
        public void CanWriteGetter_ShouldInvokeCanWriteGetterOnInnerStream()
        {
            // arrange
            _streamMock.Setup(x => x.CanWrite).Returns(true);

            // act
            bool result = _selfDisposingStream.CanWrite;

            // assert
            _streamMock.VerifyGet(x => x.CanWrite, Times.Once);
            result.Should().BeTrue();
        }

        [Test]
        public void LengthGetter_ShouldInvokeLengthGetterOnInnerStream()
        {
            // arrange
            const long length = 20;
            _streamMock.Setup(x => x.Length).Returns(length);

            // act
            long result = _selfDisposingStream.Length;

            // assert
            _streamMock.VerifyGet(x => x.Length, Times.Once);
            result.Should().Be(length);
        }

        [Test]
        public void PositionGetter_ShouldInvokePositionGetterOnInnerStream()
        {
            // arrange
            const long position = 22;
            _streamMock.Setup(x => x.Position).Returns(position);

            // act
            long result = _selfDisposingStream.Position;

            // assert
            _streamMock.VerifyGet(x => x.Position, Times.Once);
            result.Should().Be(position);
        }

        [Test]
        public void PositionSetter_ShouldInvokePositionSetterOnInnerStream()
        {
            // arrange
            const long position = 24;

            // act
            _selfDisposingStream.Position = position;

            // assert
            _streamMock.VerifySet(x => x.Position = position, Times.Once);
        }

        private void ShouldLogWarning()
        {
            ShouldLogWarning(Times.Once());
        }

        private void ShouldNotLogWarning()
        {
            ShouldLogWarning(Times.Never());
        }

        private void ShouldLogWarning(Times times)
        {
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), times);
        }
    }
}
