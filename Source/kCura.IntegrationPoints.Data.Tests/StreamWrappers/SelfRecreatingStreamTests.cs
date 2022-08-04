using System.IO;
using FluentAssertions;
using kCura.IntegrationPoints.Data.StreamWrappers;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.StreamWrappers
{
    [TestFixture, Category("Unit")]
    public class SelfRecreatingStreamTests
    {
        private Mock<Stream> _readableInnerStreamMock;
        private Mock<Stream> _unreadableInnerStreamMock;
        private Mock<IAPILog> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _readableInnerStreamMock = new Mock<Stream>();
            _readableInnerStreamMock.Setup(x => x.CanRead).Returns(true);
            _unreadableInnerStreamMock = new Mock<Stream>();
            _unreadableInnerStreamMock.Setup(x => x.CanRead).Returns(false);
            _loggerMock = new Mock<IAPILog>();
            _loggerMock.Setup(x => x.ForContext<SelfRecreatingStream>()).Returns(_loggerMock.Object);
        }

        [Test]
        public void Constructor_ShouldCreateObjectProperly_WhenStreamIsValid()
        {
            // arrange
            int numberOfCalls = 0;
            const int expectedNumberOfCalls = 1;

            // act
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);

            // assert
            numberOfCalls.Should().Be(expectedNumberOfCalls);
            selfRecreatingStream.InnerStream.Should().Be(_readableInnerStreamMock.Object);
        }

        [Test]
        public void Constructor_ShouldCreateObjectProperly_WhenStreamIsValidOnRetry()
        {
            // arrange
            int numberOfCalls = 0;
            const int expectedNumberOfCalls = 2;

            // act
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStreamOnRetry(ref numberOfCalls), _loggerMock.Object);

            // assert
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            numberOfCalls.Should().Be(expectedNumberOfCalls);
            selfRecreatingStream.InnerStream.Should().Be(_readableInnerStreamMock.Object);
        }

        [Test]
        public void Constructor_ShouldCreateObjectWithUnreadableStream_WhenStreamIsUnreadable()
        {
            // arrange
            int numberOfCalls = 0;
            const int expectedNumberOfCalls = 4;

            // act
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningUnreadableStream(ref numberOfCalls), _loggerMock.Object);

            // assert
            _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(expectedNumberOfCalls - 1));
            numberOfCalls.Should().Be(expectedNumberOfCalls);
            selfRecreatingStream.InnerStream.Should().Be(_unreadableInnerStreamMock.Object);
        }

        [Test]
        public void Flush_ShouldInvokeFlushOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);

            // act
            selfRecreatingStream.Flush();

            // assert
            _readableInnerStreamMock.Verify(x => x.Flush(), Times.Once);
        }

        [Test]
        public void Seek_ShouldInvokeSeekOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            const long offset = 0;
            const long seekReturnValue = 2;
            _readableInnerStreamMock.Setup(x => x.Seek(offset, SeekOrigin.Begin)).Returns(seekReturnValue);

            // act
            long result = selfRecreatingStream.Seek(offset, SeekOrigin.Begin);

            // assert
            _readableInnerStreamMock.Verify(x => x.Seek(offset, SeekOrigin.Begin), Times.Once);
            result.Should().Be(seekReturnValue);
        }

        [Test]
        public void SetLength_ShouldInvokeSetLengthOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            const long length = 4;

            // act
            selfRecreatingStream.SetLength(length);

            // assert
            _readableInnerStreamMock.Verify(x => x.SetLength(length), Times.Once);
        }

        [Test]
        public void Read_ShouldInvokeReadOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            byte[] buffer = { };
            const int offset = 10;
            const int count = 12;
            const int readReturnValue = 14;
            _readableInnerStreamMock.Setup(x => x.Read(buffer, offset, count)).Returns(readReturnValue);

            // act
            int result = selfRecreatingStream.Read(buffer, offset, count);

            // assert
            _readableInnerStreamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
            result.Should().Be(readReturnValue);
        }

        [Test]
        public void Write_ShouldInvokeWriteOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            byte[] buffer = { };
            const int offset = 16;
            const int count = 18;
            _readableInnerStreamMock.Setup(x => x.Write(buffer, offset, count));

            // act
            selfRecreatingStream.Write(buffer, offset, count);

            // assert
            _readableInnerStreamMock.Verify(x => x.Write(buffer, offset, count), Times.Once);
        }

        [Test]
        public void CanReadGetter_ShouldReturnTrueForReadableInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);

            // act
            bool result = selfRecreatingStream.CanRead;

            // assert
            const int expectedCanReadCallCount = 3;
            _readableInnerStreamMock.VerifyGet(x => x.CanRead, Times.Exactly(expectedCanReadCallCount));
            result.Should().BeTrue();
        }

        [Test]
        public void CanReadGetter_ShouldRetryAndReturnFalseForUnreadableInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            const int expectedNumberOfCalls = 5;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            _readableInnerStreamMock.Setup(x => x.CanRead).Returns(false);

            // act
            bool result = selfRecreatingStream.CanRead;

            // assert
            const int expectedCanReadCallCount = 7;
            _readableInnerStreamMock.VerifyGet(x => x.CanRead, Times.Exactly(expectedCanReadCallCount));
            result.Should().BeFalse();
            numberOfCalls.Should().Be(expectedNumberOfCalls);
        }

        [Test]
        public void CanSeekGetter_ShouldInvokeCanSeekGetterOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            _readableInnerStreamMock.Setup(x => x.CanSeek).Returns(true);

            // act
            bool result = selfRecreatingStream.CanSeek;

            // assert
            _readableInnerStreamMock.VerifyGet(x => x.CanSeek, Times.Once);
            result.Should().BeTrue();
        }

        [Test]
        public void CanWriteGetter_ShouldInvokeCanWriteGetterOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            _readableInnerStreamMock.Setup(x => x.CanWrite).Returns(true);

            // act
            bool result = selfRecreatingStream.CanWrite;

            // assert
            _readableInnerStreamMock.VerifyGet(x => x.CanWrite, Times.Once);
            result.Should().BeTrue();
        }

        [Test]
        public void LengthGetter_ShouldInvokeLengthGetterOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            const long length = 20;
            _readableInnerStreamMock.Setup(x => x.Length).Returns(length);

            // act
            long result = selfRecreatingStream.Length;

            // assert
            _readableInnerStreamMock.VerifyGet(x => x.Length, Times.Once);
            result.Should().Be(length);
        }

        [Test]
        public void PositionGetter_ShouldInvokePositionGetterOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            const long position = 22;
            _readableInnerStreamMock.Setup(x => x.Position).Returns(position);

            // act
            long result = selfRecreatingStream.Position;

            // assert
            _readableInnerStreamMock.VerifyGet(x => x.Position, Times.Once);
            result.Should().Be(position);
        }

        [Test]
        public void PositionSetter_ShouldInvokePositionSetterOnInnerStream()
        {
            // arrange
            int numberOfCalls = 0;
            var selfRecreatingStream = new SelfRecreatingStream(() => GetStreamFunctionReturningValidStream(ref numberOfCalls), _loggerMock.Object);
            const long position = 24;

            // act
            selfRecreatingStream.Position = position;

            // assert
            _readableInnerStreamMock.VerifySet(x => x.Position = position, Times.Once);
        }

        private Stream GetStreamFunctionReturningValidStream(ref int numberOfCalls)
        {
            ++numberOfCalls;
            return _readableInnerStreamMock.Object;
        }

        private Stream GetStreamFunctionReturningValidStreamOnRetry(ref int numberOfCalls)
        {
            ++numberOfCalls;
            return numberOfCalls == 1 ? _unreadableInnerStreamMock.Object : _readableInnerStreamMock.Object;
        }

        private Stream GetStreamFunctionReturningUnreadableStream(ref int numberOfCalls)
        {
            ++numberOfCalls;
            return _unreadableInnerStreamMock.Object;
        }
    }
}
