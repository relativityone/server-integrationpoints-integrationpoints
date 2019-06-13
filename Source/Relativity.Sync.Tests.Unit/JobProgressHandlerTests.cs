using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class JobProgressHandlerTests
	{
		private Mock<IJobProgressUpdater> _jobProgressUpdater;
		private Mock<IDateTime> _dateTime;

		private JobProgressHandler _instance;

		[SetUp]
		public void SetUp()
		{
			_dateTime = new Mock<IDateTime>();
			_jobProgressUpdater = new Mock<IJobProgressUpdater>();
			_instance = new JobProgressHandler(_jobProgressUpdater.Object, _dateTime.Object);
		}

		[TestCase(0, 0, 0)]
		[TestCase(0, 123, 0)]
		[TestCase(1, 0, 1)]
		[TestCase(1, 500, 1)]
		[TestCase(2, 500, 1)]
		[TestCase(2, 1000, 2)]
		[TestCase(3, 500, 2)]
		[TestCase(4, 500, 2)]
		[TestCase(4, 1000, 4)]
		[TestCase(5, 500, 3)]
		[TestCase(20, 500, 10)]
		public void ItShouldThrottleProgressEvents(int numberOfEvents, int delayBetweenEvents, int expectedNumberOfProgressUpdates)
		{
			DateTime now = DateTime.Now;

			// act
			for (int i = 0; i < numberOfEvents; i++)
			{
				now += TimeSpan.FromMilliseconds(delayBetweenEvents);
				_dateTime.SetupGet(x => x.Now).Returns(now);

				_instance.HandleItemProcessed(i);
			}

			// assert
			_jobProgressUpdater.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(expectedNumberOfProgressUpdates));
		}

		[Test]
		public void ItShouldUpdateProgressWhenCompleted()
		{
			const int failedItems = 1;
			const int totalItemsProcessed = 2;

			// act
			for (int i = 0; i < totalItemsProcessed; i++)
			{
				_instance.HandleItemProcessed(i);
			}

			for (int i = 0; i < failedItems; i++)
			{
				_instance.HandleItemError(new Dictionary<int, int>());
			}
			_instance.HandleProcessComplete(CreateJobReport());

			// assert
			_jobProgressUpdater.Verify(x => x.UpdateJobProgressAsync(totalItemsProcessed - failedItems, failedItems));
		}

		[TestCase(0, 0, 0)]
		[TestCase(2, 2, 0)]
		[TestCase(3, 0, 3)]
		[TestCase(0, 4, 0)]
		[TestCase(3, 2, 1)]
		public void ItShouldReportProperNumberOfItems(int numberOfItemProcessedEvents, int numberOfItemErrorEvents, int expectedNumberOfItemsProcessed)
		{
			// act
			for (int i = 0; i < numberOfItemProcessedEvents; i++)
			{
				_instance.HandleItemProcessed(i);
			}
			for (int i = 0; i < numberOfItemErrorEvents; i++)
			{
				_instance.HandleItemError(new Dictionary<int, int>());
			}
			_instance.HandleProcessComplete(CreateJobReport());

			// assert
			_jobProgressUpdater.Verify(x => x.UpdateJobProgressAsync(expectedNumberOfItemsProcessed, numberOfItemErrorEvents));
		}

		private static JobReport CreateJobReport()
		{
			JobReport jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);
			return jobReport;
		}
	}
}