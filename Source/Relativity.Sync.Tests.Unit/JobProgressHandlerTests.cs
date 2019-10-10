using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		private static PropertyInfo _jobReportTotalRowsProperty;

		private const int _THROTTLE_SECONDS = 5;

		[SetUp]
		public void SetUp()
		{
			_dateTime = new Mock<IDateTime>();
			_jobProgressUpdater = new Mock<IJobProgressUpdater>();
			_instance = new JobProgressHandler(_jobProgressUpdater.Object, _dateTime.Object);
		}

		[TestCase(0, 0, 0)]
		[TestCase(0, 123 * _THROTTLE_SECONDS, 0)]
		[TestCase(1, 0, 1)]
		[TestCase(1, 500 * _THROTTLE_SECONDS, 1)]
		[TestCase(2, 500 * _THROTTLE_SECONDS, 1)]
		[TestCase(2, 1000 * _THROTTLE_SECONDS, 2)]
		[TestCase(3, 500 * _THROTTLE_SECONDS, 2)]
		[TestCase(4, 500 * _THROTTLE_SECONDS, 2)]
		[TestCase(4, 1000 * _THROTTLE_SECONDS, 4)]
		[TestCase(5, 500 * _THROTTLE_SECONDS, 3)]
		[TestCase(20, 500 * _THROTTLE_SECONDS, 10)]
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

		[TestCase(0, 0, 0)]
		[TestCase(2, 2, 0)]
		[TestCase(3, 0, 3)]
		[TestCase(3, 2, 1)]
		public void ItShouldReportProperNumberOfItems(int numberOfItemProcessedEvents, int numberOfItemErrorEvents, int expectedNumberOfItemsProcessed)
		{
			// arrange
			JobReport jobReport = CreateConfiguredJobReport(numberOfItemProcessedEvents, numberOfItemErrorEvents);

			// act
			for (int i = 0; i < numberOfItemProcessedEvents; i++)
			{
				_instance.HandleItemProcessed(i);
			}
			for (int i = 0; i < numberOfItemErrorEvents; i++)
			{
				_instance.HandleItemError(new Dictionary<int, int>());
			}
			_instance.HandleProcessComplete(jobReport);

			// assert
			_jobProgressUpdater.Verify(x => x.UpdateJobProgressAsync(expectedNumberOfItemsProcessed, numberOfItemErrorEvents));
		}

		[Test]
		public void ItShouldUpdateStatisticsWhenJobCompletes()
		{
			// act
			_instance.HandleProcessComplete(CreateJobReport());

			// assert
			_jobProgressUpdater.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()));
		}

		[Test]
		public void ItShouldUpdateStatisticsWhenFatalExceptionOccurrs()
		{
			// act
			_instance.HandleFatalException(CreateJobReport());

			// assert
			_jobProgressUpdater.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()));
		}

		private static JobReport CreateConfiguredJobReport(int numberOfItemProcessedEvents, int numberOfItemErrorEvents)
		{
			JobReport jobReport = CreateJobReport();
			var jobError = new JobReport.RowError(0, "", "");
			for (int i = 0; i < numberOfItemErrorEvents; i++)
			{
				jobReport.ErrorRows.Add(jobError);
			}

			if (_jobReportTotalRowsProperty == null)
			{
				_jobReportTotalRowsProperty = typeof(JobReport).GetProperty(nameof(JobReport.TotalRows));
			}

			_jobReportTotalRowsProperty?.SetValue(jobReport, numberOfItemProcessedEvents);
			return jobReport;
		}

		private static JobReport CreateJobReport()
		{
			JobReport jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);
			return jobReport;
		}
	}
}