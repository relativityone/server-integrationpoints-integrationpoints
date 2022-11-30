using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
    [TestFixture]
    public class SelfRecreatingStreamTests
    {
        private Mock<Stream> _streamMock;
        private Mock<IRetriableStreamBuilder> _streamBuilderMock;

        [SetUp]
        public void SetUp()
        {
            _streamMock = new Mock<Stream>();
            _streamMock.Setup(x => x.CanRead).Returns(true);
            _streamBuilderMock = new Mock<IRetriableStreamBuilder>();
            _streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(_streamMock.Object);
        }

        [Test]
        public void ItShouldNotCallStreamCreatingFunctionWhenConstructing()
        {
            // act
            Func<SelfRecreatingStream> action = () => new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // assert
            action.Should().NotThrow();
            _streamBuilderMock.Verify(sb => sb.GetStreamAsync(), Times.Never);
        }

        [Test]
        public void ItShouldPassCanReadCall()
        {
            // arrange
            const int expectedCallCount = 2;

            _streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(_streamMock.Object);
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // act
            bool canRead = selfRecreatingStream.CanRead;

            // assert
            canRead.Should().BeTrue();
            _streamBuilderMock.Verify(sb => sb.GetStreamAsync(), Times.Once);
            _streamMock.VerifyGet(s => s.CanRead, Times.Exactly(expectedCallCount));
        }

        [Test]
        public void ItPassCanReadCallToValidStreamOnRetry()
        {
            // arrange
            const int expectedCallCount = 2;
            Mock<Stream> unreadableStreamMock = new Mock<Stream>();
            unreadableStreamMock.Setup(s => s.CanRead).Returns(false);
            _streamBuilderMock.SetupSequence(sb => sb.GetStreamAsync())
                .ReturnsAsync(unreadableStreamMock.Object)
                .ReturnsAsync(_streamMock.Object);

            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // act
            bool canRead = selfRecreatingStream.CanRead;

            // assert
            canRead.Should().BeTrue();
            _streamBuilderMock.Verify(sb => sb.GetStreamAsync(), Times.Exactly(expectedCallCount));
            unreadableStreamMock.VerifyGet(s => s.CanRead, Times.AtLeastOnce);
            _streamMock.VerifyGet(s => s.CanRead, Times.Once);
        }

        [Test]
        public void ItShouldPassCanSeekCall()
        {
            // arrange
            const bool canSeekResult = true;
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());
            _streamMock.Setup(x => x.CanSeek).Returns(canSeekResult);

            // act
            bool canSeek = selfRecreatingStream.CanSeek;

            // assert
            _streamMock.VerifyGet(s => s.CanSeek, Times.Once);
            canSeek.Should().Be(canSeekResult);
        }

        [Test]
        public void ItShouldPassFlushCall()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // act
            selfRecreatingStream.Flush();

            // assert
            _streamMock.Verify(x => x.Flush(), Times.Once);
        }

        [Test]
        public void ItShouldPassSeekCall()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            const long offset = 0;
            const long seekReturnValue = 2;
            _streamMock.Setup(x => x.Seek(offset, SeekOrigin.Begin)).Returns(seekReturnValue);

            // act
            long result = selfRecreatingStream.Seek(offset, SeekOrigin.Begin);

            // assert
            _streamMock.Verify(x => x.Seek(offset, SeekOrigin.Begin), Times.Once);
            result.Should().Be(seekReturnValue);
        }

        [Test]
        public void ItShouldPassSetLengthCall()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            const long length = 4;

            // act
            selfRecreatingStream.SetLength(length);

            // assert
            _streamMock.Verify(x => x.SetLength(length), Times.Once);
        }

        [Test]
        public void ItShouldPassReadCall()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            byte[] buffer = Array.Empty<byte>();
            const int offset = 10;
            const int count = 12;
            const int readReturnValue = 14;
            _streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(readReturnValue);

            // act
            int result = selfRecreatingStream.Read(buffer, offset, count);

            // assert
            _streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
            result.Should().Be(readReturnValue);
        }

        [Test]
        public void ItShouldPassWriteCall()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            byte[] buffer = Array.Empty<byte>();
            const int offset = 16;
            const int count = 18;
            _streamMock.Setup(x => x.Write(buffer, offset, count));

            // act
            selfRecreatingStream.Write(buffer, offset, count);

            // assert
            _streamMock.Verify(x => x.Write(buffer, offset, count), Times.Once);
        }

        [Test]
        public void ItShouldRetryAndReturnFalseForUnreadableInnerStream()
        {
            // arrange
            const int expectedNumberOfCalls = 2;
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            _streamMock.Setup(x => x.CanRead).Returns(false);

            // act
            bool canRead = selfRecreatingStream.CanRead;

            // assert
            canRead.Should().BeFalse();
            _streamMock.Verify(s => s.CanRead, Times.Exactly(expectedNumberOfCalls));
        }

        [Test]
        public void ItShouldPassCanWriteCall()
        {
            // arrange
            const bool canWriteResult = true;

            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());
            _streamMock.Setup(x => x.CanWrite).Returns(canWriteResult);

            // act
            bool result = selfRecreatingStream.CanWrite;

            // assert
            _streamMock.VerifyGet(x => x.CanWrite, Times.Once);
            result.Should().Be(canWriteResult);
        }

        [Test]
        public void ItShouldInvokeLengthGetterOnInnerStream()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            const long length = 20;
            _streamMock.Setup(x => x.Length).Returns(length);

            // act
            long result = selfRecreatingStream.Length;

            // assert
            _streamMock.VerifyGet(x => x.Length, Times.Once);
            result.Should().Be(length);
        }

        [Test]
        public void ItShouldInvokePositionGetterOnInnerStream()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            const long position = 22;
            _streamMock.Setup(x => x.Position).Returns(position);

            // act
            long result = selfRecreatingStream.Position;

            // assert
            _streamMock.VerifyGet(x => x.Position, Times.Once);
            result.Should().Be(position);
        }

        [Test]
        public void ItShouldInvokePositionSetterOnInnerStream()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            const long position = 24;

            // act
            selfRecreatingStream.Position = position;

            // assert
            _streamMock.VerifySet(x => x.Position = position, Times.Once);
        }

        [Test]
        public void ItShouldNotDisposeInnerStreamOnDisposeWhenNoCallsBeenMade()
        {
            // arrange
            var stream = new DisposalCheckStream();
            _streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(stream);

            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // act
            selfRecreatingStream.Dispose();

            // assert
            stream.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void ItShouldDisposeInnerStreamOnDispose()
        {
            // arrange
            var stream = new DisposalCheckStream();
            _streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(stream);

            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // act
            bool canRead = selfRecreatingStream.CanRead;
            selfRecreatingStream.Dispose();

            // assert
            stream.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void ItShouldDisposeReturnReadableStreamAndDisposeUnreadable()
        {
            // arrange
            const int expectedCallCount = 2;

            var unreadableStream = new DisposalCheckStream();
            unreadableStream.SetCanRead(false);

            var readableStream = new DisposalCheckStream();
            readableStream.SetCanRead(true);
            _streamBuilderMock.SetupSequence(sb => sb.GetStreamAsync()).ReturnsAsync(unreadableStream).ReturnsAsync(readableStream);

            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());

            // act
            bool canRead = selfRecreatingStream.CanRead;

            // assert
            unreadableStream.IsDisposed.Should().BeTrue();
            readableStream.IsDisposed.Should().BeFalse();
            _streamBuilderMock.Verify(sb => sb.GetStreamAsync(), Times.Exactly(expectedCallCount));
        }

        [Test]
        public void ItShouldAllowMultipleDisposeCalls()
        {
            // arrange
            SelfRecreatingStream selfRecreatingStream = new SelfRecreatingStream(_streamBuilderMock.Object, new EmptyLogger());


            // act
            selfRecreatingStream.Dispose();
            Action action = () => selfRecreatingStream.Dispose();

            // Assert
            action.Should().NotThrow();
        }
    }
}
