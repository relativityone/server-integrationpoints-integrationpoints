﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Tests.Unit.Pipelines
{
	[TestFixture]
	public class PipelineSelectorTests
	{
		private Mock<IPipelineSelectorConfiguration> _configurationMock;
		private PipelineSelector _sut;
		private Mock<ISyncLog> _loggerMock;

		[SetUp]
		public void Setup()
		{
			_configurationMock = new Mock<IPipelineSelectorConfiguration>();
			_loggerMock = new Mock<ISyncLog>();

			_sut = new PipelineSelector(_configurationMock.Object, _loggerMock.Object);
		}

		[Test]
		public void GetPipeline_Should_ReturnSyncDocumentRunPipeline()
		{
			// Act
			var pipeline = _sut.GetPipeline();

			// Assert
			pipeline.GetType().Should().Be<SyncDocumentRunPipeline>();
		}

		[Test]
		public void GetPipeline_Should_ReturnSyncDocumentRetryPipeline_When_JobHistoryToRetryIsSet()
		{
			// Arrange
			_configurationMock.SetupGet(x => x.JobHistoryToRetryId).Returns(1);

			// Act
			var pipeline = _sut.GetPipeline();

			// Assert
			pipeline.GetType().Should().Be<SyncDocumentRetryPipeline>();
		}
	}
}
