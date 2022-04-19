using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class SelfDisposingStreamTests
	{
		[Test]
		public void ItShouldInvokeFlushOnInnerStreamAndLogWarning()
		{
			// arrange 
			var streamMock = new Mock<Stream>();
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			selfDisposingStream.Flush();

			// assert
			streamMock.Verify(x => x.Flush(), Times.Once);
		}

		[Test]
		public void ItShouldInvokeSeekOnInnerStreamAndLogWarning()
		{
			// arrange
			const long offset = 0;
			const long expectedResult = 2;
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.Seek(offset, SeekOrigin.Begin)).Returns(expectedResult);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			long result = selfDisposingStream.Seek(offset, SeekOrigin.Begin);

			// assert
			streamMock.Verify(x => x.Seek(offset, SeekOrigin.Begin), Times.Once);
			result.Should().Be(expectedResult);
		}

		[Test]
		public void ItShouldInvokeSetLengthOnInnerStreamAndLogWarning()
		{
			// arrange 
			var streamMock = new Mock<Stream>();
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			const long length = 4;
			selfDisposingStream.SetLength(length);

			// assert
			streamMock.Verify(x => x.SetLength(length), Times.Once);
		}

		[Test]
		public void ItShouldInvokeReadOnInnerStreamWhenBytesAreRead()
		{
			// arrange
			byte[] buffer = Array.Empty<byte>();
			const int offset = 4;
			const int count = 6;
			const int expectedResult = 8;
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(expectedResult);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			int result = selfDisposingStream.Read(buffer, offset, count);

			// assert
			streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
			result.Should().Be(expectedResult);
		}

		[Test]
		public void ItShouldInvokeReadOnInnerStreamAndDisposeWhenNoBytesAreRead()
		{
			// arrange
			byte[] buffer = Array.Empty<byte>();
			const int offset = 10;
			const int count = 12;
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(0);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			int result = selfDisposingStream.Read(buffer, offset, count);

			// assert
			streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
			streamMock.Verify(x => x.Close(), Times.Once);
			result.Should().Be(0);
		}

		[Test]
		public void ItShouldInvokeWriteOnInnerStreamAndLogWarning()
		{
			// arrange
			byte[] buffer = Array.Empty<byte>();
			const int offset = 14;
			const int count = 16;
			var streamMock = new Mock<Stream>();
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			selfDisposingStream.Write(buffer, offset, count);

			// assert
			streamMock.Verify(x => x.Write(buffer, offset, count), Times.Once);
		}

		[Test]
		public void ItShouldInvokeReadGetterOnInnerStream()
		{
			// arrange
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.CanRead).Returns(true);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			bool result = selfDisposingStream.CanRead;

			// assert
			streamMock.VerifyGet(x => x.CanRead, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeCanSeekGetterOnInnerStream()
		{
			// arrange
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.CanSeek).Returns(true);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			bool result = selfDisposingStream.CanSeek;

			// assert
			streamMock.VerifyGet(x => x.CanSeek, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeCanWriteGetterOnInnerStream()
		{
			// arrange
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.CanWrite).Returns(true);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			bool result = selfDisposingStream.CanWrite;

			// assert
			streamMock.VerifyGet(x => x.CanWrite, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeLengthGetterOnInnerStream()
		{
			// arrange
			const long length = 20;
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.Length).Returns(length);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			long result = selfDisposingStream.Length;

			// assert
			streamMock.VerifyGet(x => x.Length, Times.Once);
			result.Should().Be(length);
		}

		[Test]
		public void ItShouldInvokePositionGetterOnInnerStream()
		{
			// arrange
			const long position = 22;
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.Position).Returns(position);
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			long result = selfDisposingStream.Position;

			// assert
			streamMock.VerifyGet(x => x.Position, Times.Once);
			result.Should().Be(position);
		}

		[Test]
		public void ItShouldInvokePositionSetterOnInnerStream()
		{
			// arrange
			const long position = 24;
			var streamMock = new Mock<Stream>();
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, new EmptyLogger());

			// act
			selfDisposingStream.Position = position;

			// assert
			streamMock.VerifySet(x => x.Position = position, Times.Once);
		}

		[Test]
		public void ItShouldLogErrorAndRethrowWhenInnerStreamReadThrows()
		{
			// arrange
			var streamMock = new Mock<Stream>();
			streamMock.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
				.Throws<ArgumentException>();
			var loggerMock = new Mock<IAPILog>();
			var selfDisposingStream = new SelfDisposingStream(streamMock.Object, loggerMock.Object);

			// act
			Func<int> action = () => selfDisposingStream.Read(Array.Empty<byte>(), 0, 1);

			// assert
			action.Should().Throw<ArgumentException>();
			loggerMock.Verify(x => x.LogError(It.Is<Exception>(y => y is ArgumentException), It.IsAny<string>(), It.IsAny<object[]>()));
		}
	}
}
