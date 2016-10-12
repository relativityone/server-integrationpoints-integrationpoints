using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests
{
    [TestFixture]
    public class JobHistoryManagerLoggingTests
    {
        [Test]
        public void ItShouldGetJobHistoryAsync()
        {
            // arrange
            var logger = Substitute.For<ILog>();
            var manager = new JobHistoryManager(logger);
            var request = new JobHistoryRequest();

            // act & assert
            Assert.Throws<AggregateException>(() =>
            {
                JobHistorySummaryModel actual = manager.GetJobHistoryAsync(request).Result;
            });

            logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}