using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Polly;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Unit.Stubs;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
	[TestFixture]
	public class SelfRecreatingStreamTests
	{
		private Mock<Stream> _readableInnerStreamMock;
		private Mock<Stream> _unreadableInnerStreamMock;
		private Mock<IStreamRetryPolicyFactory> _streamRetryPolicyFactoryMock;
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private Mock<IObjectManager> _objectManager;

		[SetUp]
		public void SetUp()
		{
			_readableInnerStreamMock = new Mock<Stream>();
			_readableInnerStreamMock.Setup(x => x.CanRead).Returns(true);
			_unreadableInnerStreamMock = new Mock<Stream>();
			_unreadableInnerStreamMock.Setup(x => x.CanRead).Returns(false);
			_objectManager = new Mock<IObjectManager>();
			// setup mock for object manager
			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_serviceFactory
				.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);

			_streamRetryPolicyFactoryMock = new Mock<IStreamRetryPolicyFactory>();
			_streamRetryPolicyFactoryMock
				.Setup(x => x.Create(
					It.IsAny<Action<int>>(),
					It.IsAny<int>(),
					It.IsAny<TimeSpan>())
				).Returns<Action<int>, int, TimeSpan>((f, i, _) => BuildNoWaitPolicy(i, f));
		}

		[Test]
		public void ItShouldNotCallStreamCreatingFunctionWhenConstructing()
		{
			// arrange
			int numberOfCalls = 0;
			const int expectedNumberOfCalls = 0;

			// act
			SelfRecreatingStream selfRecreatingStream = BuildInstance(async om => await GetStreamFunctionReturningValidStream(ref numberOfCalls).ConfigureAwait(false));

			// assert
			numberOfCalls.Should().Be(expectedNumberOfCalls);
			selfRecreatingStream.InnerStream.IsValueCreated.Should().BeFalse();
		}

		[Test]
		public void ItShouldCreateObjectProperlyWhenStreamIsValid()
		{
			// arrange
			int numberOfCalls = 0;
			const int expectedNumberOfCalls = 1;

			// act
			SelfRecreatingStream selfRecreatingStream = BuildInstance(async om => await GetStreamFunctionReturningValidStream(ref numberOfCalls).ConfigureAwait(false));
			bool canRead = selfRecreatingStream.CanRead;

			// assert
			canRead.Should().BeTrue();
			numberOfCalls.Should().Be(expectedNumberOfCalls);
			selfRecreatingStream.InnerStream.Value.Should().Be(_readableInnerStreamMock.Object);
		}

		[Test]
		public void ItShouldCreateObjectProperlyWhenStreamIsValidOnRetry()
		{
			// arrange
			int numberOfCalls = 0;
			const int expectedNumberOfCalls = 2;

			// act
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStreamOnRetry(ref numberOfCalls));
			bool canRead = selfRecreatingStream.CanRead;

			// assert
			canRead.Should().BeTrue();
			numberOfCalls.Should().Be(expectedNumberOfCalls);
			selfRecreatingStream.InnerStream.Value.Should().Be(_readableInnerStreamMock.Object);
		}

		[Test]
		public void ItShouldCreateObjectWithUnreadableStreamWhenStreamIsUnreadable()
		{
			// arrange
			int numberOfCalls = 0;
			const int expectedNumberOfCalls = 4;

			// act
			SelfRecreatingStream selfRecreatingStream = BuildInstance(om => GetStreamFunctionReturningUnreadableStream(ref numberOfCalls));
			bool canSeek = selfRecreatingStream.CanSeek;

			// assert
			numberOfCalls.Should().Be(expectedNumberOfCalls);
			selfRecreatingStream.InnerStream.Value.Should().Be(_unreadableInnerStreamMock.Object);
		}

		[Test]
		public void ItShouldInvokeFlushOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));

			// act
			selfRecreatingStream.Flush();

			// assert
			_readableInnerStreamMock.Verify(x => x.Flush(), Times.Once);
		}

		[Test]
		public void ItShouldInvokeSeekOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
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
		public void ItShouldInvokeSetLengthOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			const long length = 4;

			// act
			selfRecreatingStream.SetLength(length);

			// assert
			_readableInnerStreamMock.Verify(x => x.SetLength(length), Times.Once);
		}

		[Test]
		public void ItShouldInvokeReadOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			byte[] buffer = Array.Empty<byte>();
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
		public void ItShouldInvokeWriteOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			byte[] buffer = Array.Empty<byte>();
			const int offset = 16;
			const int count = 18;
			_readableInnerStreamMock.Setup(x => x.Write(buffer, offset, count));

			// act
			selfRecreatingStream.Write(buffer, offset, count);

			// assert
			_readableInnerStreamMock.Verify(x => x.Write(buffer, offset, count), Times.Once);
		}

		[Test]
		public void ItShouldReturnTrueForReadableInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));

			// act
			bool result = selfRecreatingStream.CanRead;

			// assert
			const int expectedCanReadCallCount = 3;
			_readableInnerStreamMock.VerifyGet(x => x.CanRead, Times.Exactly(expectedCanReadCallCount));
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldRetryAndReturnFalseForUnreadableInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			const int expectedNumberOfCalls = 8;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			_readableInnerStreamMock.Setup(x => x.CanRead).Returns(false);

			// act
			bool canRead = selfRecreatingStream.CanRead;

			// assert
			canRead.Should().BeFalse();
			numberOfCalls.Should().Be(expectedNumberOfCalls);
		}

		[Test]
		public void ItShouldInvokeCanSeekGetterOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			_readableInnerStreamMock.Setup(x => x.CanSeek).Returns(true);

			// act
			bool result = selfRecreatingStream.CanSeek;

			// assert
			_readableInnerStreamMock.VerifyGet(x => x.CanSeek, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeCanWriteGetterOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			_readableInnerStreamMock.Setup(x => x.CanWrite).Returns(true);

			// act
			bool result = selfRecreatingStream.CanWrite;

			// assert
			_readableInnerStreamMock.VerifyGet(x => x.CanWrite, Times.Once);
			result.Should().BeTrue();
		}

		[Test]
		public void ItShouldInvokeLengthGetterOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			const long length = 20;
			_readableInnerStreamMock.Setup(x => x.Length).Returns(length);

			// act
			long result = selfRecreatingStream.Length;

			// assert
			_readableInnerStreamMock.VerifyGet(x => x.Length, Times.Once);
			result.Should().Be(length);
		}

		[Test]
		public void ItShouldInvokePositionGetterOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			const long position = 22;
			_readableInnerStreamMock.Setup(x => x.Position).Returns(position);

			// act
			long result = selfRecreatingStream.Position;

			// assert
			_readableInnerStreamMock.VerifyGet(x => x.Position, Times.Once);
			result.Should().Be(position);
		}

		[Test]
		public void ItShouldInvokePositionSetterOnInnerStream()
		{
			// arrange
			int numberOfCalls = 0;
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => GetStreamFunctionReturningValidStream(ref numberOfCalls));
			const long position = 24;

			// act
			selfRecreatingStream.Position = position;

			// assert
			_readableInnerStreamMock.VerifySet(x => x.Position = position, Times.Once);
		}

		[Test]
		public void ItShouldDisposeInnerStreamOnDispose()
		{
			// arrange
			var stream = new DisposalCheckStream();
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => Task.FromResult((Stream)stream));

			// act
			bool canRead = selfRecreatingStream.CanRead;
			selfRecreatingStream.Dispose();

			// assert
			stream.IsDisposed.Should().BeTrue();
		}

		[Test]
		public void ItShouldAllowMultipleDisposeCalls()
		{
			// arrange
			SelfRecreatingStream selfRecreatingStream = BuildInstance((om) => Task.FromResult(_readableInnerStreamMock.Object));

			// act
			selfRecreatingStream.Dispose();
			Action action = () => selfRecreatingStream.Dispose();

			// Assert
			action.Should().NotThrow();
		}

		private IAsyncPolicy<Stream> BuildNoWaitPolicy(int retryCount, Action<int> onRetry)
		{
			return Policy
				.HandleResult<Stream>(s => s == null || !s.CanRead)
				.Or<Exception>()
				.RetryAsync(retryCount, (_, i) => onRetry(i));
		}

		private SelfRecreatingStream BuildInstance(Func<IObjectManager, Task<Stream>> getStreamFunction)
		{
			return new SelfRecreatingStream(_serviceFactory.Object, getStreamFunction, _streamRetryPolicyFactoryMock.Object, new EmptyLogger());
		}

		private Task<Stream> GetStreamFunctionReturningValidStream(ref int numberOfCalls)
		{
			++numberOfCalls;
			return Task.FromResult(_readableInnerStreamMock.Object);
		}

		private Task<Stream> GetStreamFunctionReturningValidStreamOnRetry(ref int numberOfCalls)
		{
			++numberOfCalls;
			return Task.FromResult(numberOfCalls == 1 ? _unreadableInnerStreamMock.Object : _readableInnerStreamMock.Object);
		}

		private Task<Stream> GetStreamFunctionReturningUnreadableStream(ref int numberOfCalls)
		{
			++numberOfCalls;
			return Task.FromResult(_unreadableInnerStreamMock.Object);
		}
	}
}