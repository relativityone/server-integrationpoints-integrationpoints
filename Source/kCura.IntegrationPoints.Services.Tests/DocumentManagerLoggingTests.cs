using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests
{
    [TestFixture]
    public class DocumentManagerLoggingTests
    {
        [Test]
        public void ItShouldLogErrorFromGetPercentagePushedToReviewAsync()
        {
            // arrange
            var logger = Substitute.For<ILog>();
            var manager = new DocumentManager(logger);
            var request = new PercentagePushedToReviewRequest();

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                PercentagePushedToReviewModel actual = manager.GetPercentagePushedToReviewAsync(request).Result;
            });

            logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromGetCurrentPromotionStatusAsync()
        {
            // arrange
            var logger = Substitute.For<ILog>();
            var manager = new DocumentManager(logger);
            var request = new CurrentPromotionStatusRequest();

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                CurrentPromotionStatusModel actual = manager.GetCurrentPromotionStatusAsync(request).Result;
            });

            logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public void ItShouldLogErrorFromGetHistoricalPromotionStatusAsync()
        {
            // arrange
            var logger = Substitute.For<ILog>();
            var manager = new DocumentManager(logger);
            var request = new HistoricalPromotionStatusRequest();

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                HistoricalPromotionStatusSummaryModel actual = manager.GetHistoricalPromotionStatusAsync(request).Result;
            });

            logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}