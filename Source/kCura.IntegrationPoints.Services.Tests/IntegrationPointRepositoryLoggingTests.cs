using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests
{
	[TestFixture]
	public class IntegrationPointRepositoryLoggingTests
	{
		[SetUp]
		public void SetUp()
		{
			_logger = Substitute.For<ILog>();
			_integrationPointRepository = new IntegrationPointRepository(_logger);
		}

		private ILog _logger;
		private IIntegrationPointRepository _integrationPointRepository;

		[Test]
		public void ItShouldLogErrorFromCreateIntegrationPointAsync()
		{
			// arrange
			var request = new CreateIntegrationPointRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				IntegrationPointModel actual = _integrationPointRepository.CreateIntegrationPointAsync(request).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetAllIntegrationPointsAsync()
		{
			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				IList<IntegrationPointModel> actual = _integrationPointRepository.GetAllIntegrationPointsAsync(0).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetIntegrationPointArtifactTypeIdAsync()
		{
			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				int actual = _integrationPointRepository.GetIntegrationPointArtifactTypeIdAsync(0).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetIntegrationPointAsync()
		{
			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				IntegrationPointModel actual = _integrationPointRepository.GetIntegrationPointAsync(0, 0).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetSourceProviderArtifactIdAsync()
		{
			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				int actual = _integrationPointRepository.GetSourceProviderArtifactIdAsync(0, string.Empty).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromRunIntegrationPointAsync()
		{
			// act & assert
			Assert.Throws<AggregateException>(() => { _integrationPointRepository.RunIntegrationPointAsync(0, 0).Wait(); });

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromUpdateIntegrationPointAsync()
		{
			// arrange
			var request = new UpdateIntegrationPointRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				IntegrationPointModel actual = _integrationPointRepository.UpdateIntegrationPointAsync(request).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}