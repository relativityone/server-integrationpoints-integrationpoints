using Relativity.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer.StreamWrappers;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Transfer.StreamWrappers
{
	public class StreamWithMetricsTests
	{
		private const int _DOC_ID = 1111;

		private Mock<IStopwatch> _stopwatchFake;
		private Mock<IJobStatisticsContainer> _jobStatisticsContainerMock;
		private Mock<IAPILog> _loggerMock;

		[SetUp]
		public void SetUp()
		{
			_stopwatchFake = new Mock<IStopwatch>();
			_jobStatisticsContainerMock = new Mock<IJobStatisticsContainer>();
			_loggerMock = new Mock<IAPILog>();
		}

		[Test]
		public void Read_ShouldInvokeInnerStream()
		{
			// Arrange
			const int bufferSize = 2;
			var wrappedStream = new Mock<Stream>();
			var sut = new StreamWithMetrics(wrappedStream.Object, _stopwatchFake.Object, _DOC_ID,
				_jobStatisticsContainerMock.Object, _loggerMock.Object);

			// Act
			sut.Read(new byte[bufferSize], 0, bufferSize);

			// Assert
			wrappedStream.Verify(x => x.Read(It.Is<byte[]>(buffer => buffer.Length == bufferSize), 0, bufferSize));
		}

		[Test]
		public void Dispose_ShouldLogGatheredMetrics()
		{
			// Arrange
			var longTextStatistics = new List<LongTextStreamStatistics>();
			var jobStatisticsContainer = new Mock<IJobStatisticsContainer>();
			jobStatisticsContainer.SetupGet(x => x.LongTextStatistics).Returns(longTextStatistics);

			var wrappedStream = new Mock<Stream>();
			const long bytesRead = 10;

			var elapsedTime = TimeSpan.FromSeconds(2);
			_stopwatchFake.SetupGet(x => x.Elapsed).Returns(elapsedTime);

			const int readInvocationsCount = 1;

			wrappedStream
				.Setup(x => x.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns((int)bytesRead);

			var sut = new StreamWithMetrics(wrappedStream.Object, _stopwatchFake.Object, _DOC_ID,
				jobStatisticsContainer.Object, _loggerMock.Object);

			// Act
			sut.Read(new byte[1], 0, 1);
			sut.Dispose();

			// Assert
			_loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), 
				_DOC_ID, bytesRead, elapsedTime.TotalSeconds, readInvocationsCount));

			longTextStatistics.Count.Should().Be(1);
			longTextStatistics.Single().TotalBytesRead.Should().Be(bytesRead);
			longTextStatistics.Single().TotalReadTime.Should().Be(elapsedTime);
		}
	}
}
