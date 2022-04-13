using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors.SumReporting
{
	[TestFixture]
	internal class JobEndMetricsServiceFactoryTests
	{
		private IJobEndMetricsServiceFactory _sut;

		private Mock<IPipelineSelector> _pipelineSelectorFake;

		private static readonly ISyncPipeline[] DocumentTypePipelines =
		{
			new SyncDocumentRunPipeline(),
			new SyncDocumentRetryPipeline()
		};

		private static readonly ISyncPipeline[] ImageTypePipelines =
		{
			new SyncImageRunPipeline(),
			new SyncImageRetryPipeline()
		};

		[SetUp]
		public void SetUp()
		{
			_pipelineSelectorFake = new Mock<IPipelineSelector>();

			var batchRepository = new Mock<IBatchRepository>();
			var configuration = new Mock<IJobEndMetricsConfiguration>();
			var fieldManager = new Mock<IFieldManager>();
			var jobStatisticsContainer = new Mock<IJobStatisticsContainer>();
			var syncMetrics = new Mock<ISyncMetrics>();
			var logger = new Mock<IAPILog>();

			_sut = new JobEndMetricsServiceFactory(
				_pipelineSelectorFake.Object,
				batchRepository.Object,
				configuration.Object,
				fieldManager.Object,
				jobStatisticsContainer.Object,
				syncMetrics.Object,
				logger.Object);
		}

		[TestCaseSource(nameof(DocumentTypePipelines))]
		public void CreateJobEndMetricsService_ShouldReturnDocumentJobEndMetricsService_WhenPipelineIsDocumentTypeAndNotSuspending(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			IJobEndMetricsService result = _sut.CreateJobEndMetricsService(isSuspended: false);

			// Assert
			result.Should().BeOfType<DocumentJobEndMetricsService>();
		}

		[TestCaseSource(nameof(ImageTypePipelines))]
		public void CreateJobEndMetricsService_ShouldReturnImageJobEndMetricsService_WhenPipelineIsImageTypeAndNotSuspending(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			IJobEndMetricsService result = _sut.CreateJobEndMetricsService(isSuspended: false);

			// Assert
			result.Should().BeOfType<ImageJobEndMetricsService>();
		}

		[TestCaseSource(nameof(DocumentTypePipelines))]
		public void CreateJobEndMetricsService_ShouldReturnDocumentJobSuspendedMetricsService_WhenPipelineIsDocumentTypeAndSuspending(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			IJobEndMetricsService result = _sut.CreateJobEndMetricsService(isSuspended: true);

			// Assert
			result.Should().BeOfType<DocumentJobSuspendedMetricsService>();
		}

		[TestCaseSource(nameof(ImageTypePipelines))]
		public void CreateJobEndMetricsService_ShouldReturnImageJobSuspendedMetricsService_WhenPipelineIsImageTypeAndSuspending(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			IJobEndMetricsService result = _sut.CreateJobEndMetricsService(isSuspended: true);

			// Assert
			result.Should().BeOfType<ImageJobSuspendedMetricsService>();
		}

		[TestCase(true)]
		[TestCase(false)]
		public void CreateJobEndMetricsService_ShouldReturnEmptyJobEndMetricsService_WhenPipelineTypeIsUnableToDetermine(bool isSuspended)
		{
			// Arrange
			var testPipeline = new Mock<ISyncPipeline>();
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(testPipeline.Object);

			// Act
			IJobEndMetricsService result = _sut.CreateJobEndMetricsService(isSuspended);

			// Assert
			result.Should().BeOfType<EmptyJobEndMetricsService>();
		}
	}
}
