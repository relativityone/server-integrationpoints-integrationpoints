using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
	[TestFixture]
	public class SelfDisposingStreamTests
	{
		private Mock<Stream> _streamMock;

		[SetUp]
		public void SetUp()
		{
			_streamMock = new Mock<Stream>();
		}

		[Test]
		public void ItShouldInvokeFlushOnInnerStreamAndLogWarning()
		{
			// arrange 
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());
			
			// act
			selfDisposingStream.Flush();

			// assert
			_streamMock.Verify(x => x.Flush(), Times.Once);
		}

		[Test]
		public void ItShouldInvokeSeekOnInnerStreamAndLogWarning()
		{
			// arrange
			const long offset = 0;
			const long expectedResult = 2;
			_streamMock.Setup(x => x.Seek(offset, SeekOrigin.Begin)).Returns(expectedResult);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			long result = selfDisposingStream.Seek(offset, SeekOrigin.Begin);

			// assert
			_streamMock.Verify(x => x.Seek(offset, SeekOrigin.Begin), Times.Once);
			result.Should().Be(expectedResult);
		}

		[Test]
		public void ItShouldInvokeSetLengthOnInnerStreamAndLogWarning()
		{
			// arrange 
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());
			
			// act
			const long length = 4;
			selfDisposingStream.SetLength(length);

			// assert
			_streamMock.Verify(x => x.SetLength(length), Times.Once);
		}

		[Test]
		public void ItShouldInvokeReadOnInnerStreamWhenBytesAreRead()
		{
			// arrange
			byte[] buffer = Array.Empty<byte>();
			const int offset = 4;
			const int count = 6;
			const int expectedResult = 8;
			_streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(expectedResult);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			int result = selfDisposingStream.Read(buffer, offset, count);

			// assert
			_streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
			result.Should().Be(expectedResult);
		}

		[Test]
		public void ItShouldInvokeReadOnInnerStreamAndDisposeWhenNoBytesAreRead()
		{
			// arrange
			byte[] buffer = Array.Empty<byte>();
			const int offset = 10;
			const int count = 12;
			_streamMock.Setup(x => x.Read(buffer, offset, count)).Returns(0);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			int result = selfDisposingStream.Read(buffer, offset, count);

			// assert
			_streamMock.Verify(x => x.Read(buffer, offset, count), Times.Once);
			_streamMock.Verify(x => x.Close(), Times.Once);
			result.Should().Be(0);
		}

		[Test]
		public void ItShouldInvokeWriteOnInnerStreamAndLogWarning()
		{
			// arrange
			byte[] buffer = Array.Empty<byte>();
			const int offset = 14;
			const int count = 16;
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			selfDisposingStream.Write(buffer, offset, count);

			// assert
			_streamMock.Verify(x => x.Write(buffer, offset, count), Times.Once);
		}

		[Test]
		public void ItShouldInvokeReadGetterOnInnerStream()
		{
			// arrange
			_streamMock.Setup(x => x.CanRead).Returns(true);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			bool result = selfDisposingStream.CanRead;

			// assert
			_streamMock.VerifyGet(x => x.CanRead, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeCanSeekGetterOnInnerStream()
		{
			// arrange
			_streamMock.Setup(x => x.CanSeek).Returns(true);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			bool result = selfDisposingStream.CanSeek;

			// assert
			_streamMock.VerifyGet(x => x.CanSeek, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeCanWriteGetterOnInnerStream()
		{
			// arrange
			_streamMock.Setup(x => x.CanWrite).Returns(true);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			bool result = selfDisposingStream.CanWrite;

			// assert
			_streamMock.VerifyGet(x => x.CanWrite, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeLengthGetterOnInnerStream()
		{
			// arrange
			const long length = 20;
			_streamMock.Setup(x => x.Length).Returns(length);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			long result = selfDisposingStream.Length;

			// assert
			_streamMock.VerifyGet(x => x.Length, Times.Once);
			result.Should().Be(length);
		}

		[Test]
		public void ItShouldInvokePositionGetterOnInnerStream()
		{
			// arrange
			const long position = 22;
			_streamMock.Setup(x => x.Position).Returns(position);
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			long result = selfDisposingStream.Position;

			// assert
			_streamMock.VerifyGet(x => x.Position, Times.Once);
			result.Should().Be(position);
		}

		[Test]
		public void ItShouldInvokePositionSetterOnInnerStream()
		{
			// arrange
			const long position = 24;
			var selfDisposingStream = new SelfDisposingStream(_streamMock.Object, new EmptyLogger());

			// act
			selfDisposingStream.Position = position;

			// assert
			_streamMock.VerifySet(x => x.Position = position, Times.Once);
		}
	}
}