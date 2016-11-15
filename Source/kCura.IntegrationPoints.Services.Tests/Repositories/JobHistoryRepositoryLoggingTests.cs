using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Services.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	[TestFixture]
	public class JobHistoryRepositoryLoggingTests : TestBase
	{
		private IJobHistoryRepository _jobHistoryRepository;
		private ILog _logger;

		[SetUp]
		public override void SetUp()
		{
			_logger = Substitute.For<ILog>();
			_jobHistoryRepository = new JobHistoryRepository(_logger);
		}

		[Test]
		public void ItShouldLogErrorFromGetJobHistory()
		{
			// arrange
			var request = new JobHistoryRequest();

			// act & assert
			Assert.That(() =>
			{
				JobHistorySummaryModel actual = _jobHistoryRepository.GetJobHistory(request);
			}, Throws.Exception);

			_logger.Received().LogError(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
		}
	}
}