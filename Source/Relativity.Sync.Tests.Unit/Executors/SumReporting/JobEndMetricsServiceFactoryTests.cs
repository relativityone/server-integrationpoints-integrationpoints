﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
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

		private static readonly ISyncPipeline[] DocumentTypePipelines = new ISyncPipeline[]
		{
			new SyncDocumentRunPipeline(),
			new SyncDocumentRetryPipeline()
		};

		// TODO: REL-465065
		private static ISyncPipeline[] ImageTypePipelines = new ISyncPipeline[]
		{
		//	new SyncImageRunPipeline(),
		//	new SyncImageRetryPipeline()
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
			var logger = new Mock<ISyncLog>();

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

		public void CreateJobEndMetricsService_ShouldReturnDocumentJobEndMetricsService_WhenPipelineIsDocumentType(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			var result = _sut.CreateJobEndMetricsService();

			// Assert
			result.Should().BeOfType<DocumentJobEndMetricsService>();
		}

		[TestCaseSource(nameof(ImageTypePipelines))]
		public void CreateJobEndMetricsService_ShouldReturnImageJobEndMetricsService_WhenPipelineIsImageType(ISyncPipeline syncPipeline)
		{
			// Arrange
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(syncPipeline);

			// Act
			var result = _sut.CreateJobEndMetricsService();

			// Assert
			result.Should().BeOfType<ImageJobEndMetricsService>();
		}

		[Test]

		public void CreateJobEndMetricsService_ShouldReturnEmptyJobEndMetricsService_WhenPipelineTypeIsUnableToDetermine()
		{
			// Arrange
			var testPipeline = new Mock<ISyncPipeline>();
			_pipelineSelectorFake.Setup(x => x.GetPipeline()).Returns(testPipeline.Object);

			// Act
			var result = _sut.CreateJobEndMetricsService();

			// Assert
			result.Should().BeOfType<EmptyJobEndMetricsService>();
		}
	}
}
