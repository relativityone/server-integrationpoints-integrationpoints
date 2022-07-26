using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer.StreamWrappers;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    internal sealed class ImportStreamBuilderTests
    {
        private const int _DOC_ARTIFACT_ID = 1111;
        private Mock<IRetriableStreamBuilder> _streamBuilderMock;
        private Func<IStopwatch> _stopwatchFake;
        private Mock<IJobStatisticsContainer> _jobStatisticsContainerFake;

        [SetUp]
        public void SetUp()
        {
            _streamBuilderMock = new Mock<IRetriableStreamBuilder>();
            _stopwatchFake = () => new StopwatchWrapper();
            _jobStatisticsContainerFake = new Mock<IJobStatisticsContainer>();
        }

        private static IEnumerable<TestCaseData> EncodingTestCases()
        {
            yield return new TestCaseData(Encoding.ASCII);
            yield return new TestCaseData(Encoding.Unicode);
        }

        [TestCaseSource(nameof(EncodingTestCases))]
        public void Create_ShouldWrapStreamBasedOnEncoding(Encoding encoding)
        {
            // Arrange
            const string streamInput = "hello world!";
            var stream = new MemoryStream(encoding.GetBytes(streamInput));
            _streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(stream);

            var instance = new ImportStreamBuilder(_stopwatchFake, _jobStatisticsContainerFake.Object, new EmptyLogger());

            // Act
            StreamEncoding streamEncoding = encoding is UnicodeEncoding
                ? StreamEncoding.Unicode
                : StreamEncoding.ASCII;
            Stream result = instance.Create(_streamBuilderMock.Object, streamEncoding, _DOC_ARTIFACT_ID);

            //// Assert
            string streamOutput = ReadOutUnicodeString(result);
            streamOutput.Should().Be(streamInput);
        }

        [Test]
        public void Create_ShouldWrapStreamInSelfDisposingStream()
        {
            // Arrange
            var stream = new Mock<Stream>();
            stream.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
            _streamBuilderMock.Setup(sb => sb.GetStreamAsync()).ReturnsAsync(stream.Object);
            
            var instance = new ImportStreamBuilder(_stopwatchFake, _jobStatisticsContainerFake.Object, new EmptyLogger());

            // Act
            Stream result = instance.Create(_streamBuilderMock.Object, StreamEncoding.Unicode, _DOC_ARTIFACT_ID);

            // Assert
            result.CanRead.Should().BeFalse();
        }

        [Test]
        public void Create_ShouldWrapStreamInMetricsStream()
        {
            // Arrange
            var instance = new ImportStreamBuilder(_stopwatchFake, _jobStatisticsContainerFake.Object, new EmptyLogger());

            // Act
            Stream stream = instance.Create(_streamBuilderMock.Object, StreamEncoding.Unicode, _DOC_ARTIFACT_ID);
            SelfDisposingStream outerStream = stream as SelfDisposingStream;

            // Assert
            outerStream.Should().NotBeNull();
            outerStream.InnerStream.Should().BeOfType<StreamWithMetrics>();
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
