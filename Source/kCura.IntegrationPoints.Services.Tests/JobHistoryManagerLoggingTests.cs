using System;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests
{
	[TestFixture]
	public class JobHistoryManagerLoggingTests : ServiceTestsBase
	{
		[Test]
		public void ItShouldGetJobHistoryAsync()
		{
			// arrange
			var manager = new JobHistoryManager(Logger, PermissionRepositoryFactory);
			var request = new JobHistoryRequest();

			// act & assert
			Assert.Throws<AggregateException>(() =>
			{
				JobHistorySummaryModel actual = manager.GetJobHistoryAsync(request).Result;
			});

			Logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}