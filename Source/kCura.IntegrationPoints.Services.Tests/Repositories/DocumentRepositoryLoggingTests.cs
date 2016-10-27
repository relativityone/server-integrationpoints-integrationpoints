using System;
using kCura.IntegrationPoints.Services.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	[TestFixture]
	public class DocumentRepositoryLoggingTests
	{
		[SetUp]
		public void SetUp()
		{
			_logger = Substitute.For<ILog>();
			_documentRepository = new DocumentRepository(_logger);
		}

		private IDocumentRepository _documentRepository;
		private ILog _logger;

		[Test]
		public void ItShouldLogErrorFromGetCurrentPromotionStatusAsync()
		{
			// arrange
			var request = new CurrentPromotionStatusRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				var actual = _documentRepository.GetCurrentPromotionStatusAsync(request).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetHistoricalPromotionStatusAsync()
		{
			// arrange
			var request = new HistoricalPromotionStatusRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				var actual = _documentRepository.GetHistoricalPromotionStatusAsync(request).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetPercentagePushedToReviewAsync()
		{
			// arrange
			var request = new PercentagePushedToReviewRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				var actual = _documentRepository.GetPercentagePushedToReviewAsync(request).Result;
			});

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}