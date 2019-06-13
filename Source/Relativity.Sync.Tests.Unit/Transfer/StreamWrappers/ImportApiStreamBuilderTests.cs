using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Polly;
using Relativity.Services.Exceptions;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
	[TestFixture]
	[Parallelizable(ParallelScope.Self)]
	internal sealed class ImportApiStreamBuilderTests
	{
		private Mock<IStreamRetryPolicyFactory> _streamRetryPolicyFactory;
		private Mock<ISyncLog> _logger;

		[SetUp]
		public void SetUp()
		{
			_streamRetryPolicyFactory = new Mock<IStreamRetryPolicyFactory>();
			SetupRetryPolicyFactory((a, b, c) => Policy.NoOp<Stream>());
			_logger = new Mock<ISyncLog>();
		}

		private static IEnumerable<TestCaseData> EncodingTestCases()
		{
			yield return new TestCaseData(Encoding.ASCII);
			yield return new TestCaseData(Encoding.Unicode);
		}

		[TestCaseSource(nameof(EncodingTestCases))]
		public void ItShouldWrapStreamBasedOnEncoding(Encoding encoding)
		{
			// Arrange
			const string streamInput = "hello world!";
			var stream = new MemoryStream(encoding.GetBytes(streamInput));

			var instance = new ImportStreamBuilder(_streamRetryPolicyFactory.Object, _logger.Object);

			// Act
			StreamEncoding streamEncoding = encoding is UnicodeEncoding
				? StreamEncoding.Unicode
				: StreamEncoding.ASCII;
			Stream result = instance.Create(() => stream, streamEncoding);

			// Assert
			string streamOutput = ReadOutUnicodeString(result);
			streamOutput.Should().Be(streamInput);
		}

		[Test]
		public void ItShouldWrapStreamInSelfRecreatingStream()
		{
			// Arrange
			int timesInvoked = 0;

			Stream GetStream()
			{
				timesInvoked += 1;
				if (timesInvoked == 1)
				{
					throw new ServiceException();
				}

				const string streamInput = "hello world!";
				var stream = new MemoryStream(Encoding.Unicode.GetBytes(streamInput));
				return stream;
			}

			SetupRetryPolicyFactory((onRetry, retryCount, dur) =>
				Policy.HandleResult<Stream>(s => false)
					.Or<Exception>()
					.Retry(retryCount, (_, i) => onRetry(i)));

			var instance = new ImportStreamBuilder(_streamRetryPolicyFactory.Object, _logger.Object);

			// Act
			Stream result = instance.Create(GetStream, StreamEncoding.Unicode);

			// Assert
			Assert.DoesNotThrow(() => ReadOutUnicodeString(result));
		}

		[Test]
		public void ItShouldWrapStreamInSelfDisposingStream()
		{
			// Arrange
			var stream = new Mock<Stream>();
			stream.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
			var instance = new ImportStreamBuilder(_streamRetryPolicyFactory.Object, _logger.Object);

			// Act
			Stream result = instance.Create(() => stream.Object, StreamEncoding.Unicode);

			// Assert
			result.CanRead.Should().BeFalse();
		}

		private void SetupRetryPolicyFactory(Func<Action<int>, int, TimeSpan, ISyncPolicy<Stream>> policyFunc)
		{
			_streamRetryPolicyFactory.Setup(x => x.Create(
				It.IsAny<Action<int>>(),
				It.IsAny<int>(),
				It.IsAny<TimeSpan>())).Returns(policyFunc);
		}

		private string ReadOutUnicodeString(Stream stream)
		{
			const int bufferSize = 1024;
			byte[] buffer = new byte[bufferSize];
			int bytesRead = stream.Read(buffer, 0, buffer.Length);
			string streamOutput = Encoding.Unicode.GetString(buffer, 0, bytesRead);
			return streamOutput;
		}
	}
}
