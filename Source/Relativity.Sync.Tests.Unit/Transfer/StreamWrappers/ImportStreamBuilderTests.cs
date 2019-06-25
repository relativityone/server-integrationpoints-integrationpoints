using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
	[TestFixture]
	[Parallelizable(ParallelScope.Self)]
	internal sealed class ImportStreamBuilderTests
	{
		private Mock<IRetriableStreamBuilder> _streamBuilderMock;
		private Mock<ISyncLog> _logger;

		[SetUp]
		public void SetUp()
		{
			_streamBuilderMock = new Mock<IRetriableStreamBuilder>();
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
			_streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(stream);

			var instance = new ImportStreamBuilder(_logger.Object);

			// Act
			StreamEncoding streamEncoding = encoding is UnicodeEncoding
				? StreamEncoding.Unicode
				: StreamEncoding.ASCII;
			Stream result = instance.Create(_streamBuilderMock.Object, streamEncoding);

			//// Assert
			string streamOutput = ReadOutUnicodeString(result);
			streamOutput.Should().Be(streamInput);
		}

		[Test]
		public void ItShouldWrapStreamInSelfDisposingStream()
		{
			// Arrange
			var stream = new Mock<Stream>();
			stream.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
			_streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(stream.Object);
			
			var instance = new ImportStreamBuilder(_logger.Object);

			// Act
			Stream result = instance.Create(_streamBuilderMock.Object, StreamEncoding.Unicode);

			// Assert
			result.CanRead.Should().BeFalse();
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
