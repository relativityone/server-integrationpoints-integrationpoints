using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Kepler.Transport;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer.StreamWrappers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class RetriableLongTextStreamBuilderTests
	{
		private IRetriableStreamBuilder _instance;
		private Mock<IKeplerStream> _keplerStream;
		private Mock<ISyncMetrics> _syncMetrics;
		private const string _EXPECTED_METRIC_BUCKET_NAME = "Relativity.Sync.LongTextStreamBuilder.Retry.Count";
		private const double _EXPECTED_WAIT_INTERVAL_BETWEEN_CALLS = 1.0;
		private const ExecutionStatus _EXPECTED_METRIC_STATUS = ExecutionStatus.Failed;

		[SetUp]
		public void SetUp()
		{
			const int relativityObjectArtifactId = 234;
			const int workspaceArtifactId = 123;
			const string fieldName = "Field Name";
			
			_keplerStream = new Mock<IKeplerStream>();

			var objectManager = new Mock<IObjectManager>();
			objectManager.Setup(om => om.StreamLongTextAsync(workspaceArtifactId, It.IsAny<RelativityObjectRef>(), It.IsAny<FieldRef>())).ReturnsAsync(_keplerStream.Object);

			var serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(f => f.CreateProxyAsync<IObjectManager>()).ReturnsAsync(objectManager.Object);

			_syncMetrics = new Mock<ISyncMetrics>();

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			containerBuilder.RegisterInstance(serviceFactory.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(_syncMetrics.Object).As<ISyncMetrics>();
			IContainer container = containerBuilder.Build();
			IRetriableStreamBuilderFactory streamBuilderFactory = container.Resolve<IRetriableStreamBuilderFactory>();
			_instance = streamBuilderFactory.Create(workspaceArtifactId, relativityObjectArtifactId, fieldName);
		}

		[Test]
		public async Task ItShouldRunGoldFlow()
		{
			// Arrange
			var streamToReturn = new DisposalCheckStream();
			streamToReturn.SetCanRead(true);

			_keplerStream.Setup(ks => ks.GetStreamAsync()).ReturnsAsync(streamToReturn);

			// Act
			Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);

			// Assert
			result.Should().Be(streamToReturn);
			streamToReturn.IsDisposed.Should().BeFalse();
			_syncMetrics.Verify(m => m.CountOperation(It.IsAny<string>(), It.IsAny<ExecutionStatus>()), Times.Never);

		}

		[Test]
		public async Task ItShouldReturnReadableStream()
		{
			// Arrange
			var readableStream = new DisposalCheckStream();
			readableStream.SetCanRead(true);

			var unreadableStream = new DisposalCheckStream();
			unreadableStream.SetCanRead(false);

			_keplerStream.SetupSequence(ks => ks.GetStreamAsync()).ReturnsAsync(unreadableStream).ReturnsAsync(readableStream);

			// Act

			Stopwatch stopwatch = Stopwatch.StartNew();
			Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);
			stopwatch.Stop();

			// Assert
			result.Should().Be(readableStream);
			stopwatch.Elapsed.TotalSeconds.Should().BeGreaterOrEqualTo(_EXPECTED_WAIT_INTERVAL_BETWEEN_CALLS);
			readableStream.IsDisposed.Should().BeFalse();
			unreadableStream.IsDisposed.Should().BeTrue();
			_syncMetrics.Verify(m => m.CountOperation(_EXPECTED_METRIC_BUCKET_NAME, _EXPECTED_METRIC_STATUS), Times.Once);
		}

		[Test]
		public async Task ItShouldRetryThreeTimes()
		{
			// Arrange
			const int expectedRetryCount = 3;
			const int expectedGetStreamCallCount = 1 + expectedRetryCount; // 1st call + number of retries

			var unreadableStream = new DisposalCheckStream();
			unreadableStream.SetCanRead(false);

			_keplerStream.Setup(ks => ks.GetStreamAsync()).ReturnsAsync(unreadableStream);

			// Act
			Stream result = await _instance.GetStreamAsync().ConfigureAwait(false);

			// Assert
			result.Should().Be(unreadableStream);
			_syncMetrics.Verify(m => m.CountOperation(_EXPECTED_METRIC_BUCKET_NAME, _EXPECTED_METRIC_STATUS), Times.Exactly(expectedRetryCount));
			_keplerStream.Verify(s => s.GetStreamAsync(), Times.Exactly(expectedGetStreamCallCount));
		}
	}
}