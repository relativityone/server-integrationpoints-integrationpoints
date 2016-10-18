using System;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests
{
	[TestFixture]
	public class DocumentManagerLoggingTests : ServiceTestsBase
	{
		[Test]
		public void ItShouldLogErrorFromGetCurrentPromotionStatusAsync()
		{
			// arrange
			var manager = new DocumentManager(Logger, PermissionRepositoryFactory);
			var request = new CurrentPromotionStatusRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				CurrentPromotionStatusModel actual = manager.GetCurrentPromotionStatusAsync(request).Result;
			});

			Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetHistoricalPromotionStatusAsync()
		{
			// arrange
			var manager = new DocumentManager(Logger, PermissionRepositoryFactory);
			var request = new HistoricalPromotionStatusRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				HistoricalPromotionStatusSummaryModel actual = manager.GetHistoricalPromotionStatusAsync(request).Result;
			});

			Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}

		[Test]
		public void ItShouldLogErrorFromGetPercentagePushedToReviewAsync()
		{
			// arrange
			var manager = new DocumentManager(Logger, PermissionRepositoryFactory);
			var request = new PercentagePushedToReviewRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				PercentagePushedToReviewModel actual = manager.GetPercentagePushedToReviewAsync(request).Result;
			});

			Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}