using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.Relativity.DataReaderClient;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Transfer;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class JobProgressHandlerTests : IDisposable
	{
		private Mock<IJobProgressUpdater> _jobProgressUpdaterMock;
		private Mock<ISyncImportBulkArtifactJob> _bulkImportJobStub;


		private JobProgressHandler _sut;
		private TestScheduler _testScheduler;

		private const int _THROTTLE_SECONDS = 5;

		private static readonly Lazy<PropertyInfo> _jobReportTotalRowsProperty = new Lazy<PropertyInfo>(GetJobReportTotalRowsPropertyInfo);

		[SetUp]
		public void SetUp()
		{
			_jobProgressUpdaterMock = new Mock<IJobProgressUpdater>();
			_bulkImportJobStub = new Mock<ISyncImportBulkArtifactJob>();


			_testScheduler = new TestScheduler();
			_sut = new JobProgressHandler(_jobProgressUpdaterMock.Object, _testScheduler);
		}

		[TestCase(0, 0, 1)]
		[TestCase(0, 123 * _THROTTLE_SECONDS, 1)]
		[TestCase(1, 0, 1)]
		[TestCase(1, 500 * _THROTTLE_SECONDS, 1)]
		[TestCase(2, 500 * _THROTTLE_SECONDS, 2)]
		[TestCase(2, 1000 * _THROTTLE_SECONDS, 2)]
		[TestCase(3, 500 * _THROTTLE_SECONDS, 2)]
		[TestCase(4, 500 * _THROTTLE_SECONDS, 3)]
		[TestCase(4, 1000 * _THROTTLE_SECONDS, 4)]
		[TestCase(5, 500 * _THROTTLE_SECONDS, 3)]
		[TestCase(20, 500 * _THROTTLE_SECONDS, 11)]
		public void AttachToImportJob_ShouldThrottleProgressEvents(int numberOfEvents, int delayBetweenEvents, int expectedNumberOfProgressUpdates)
		{
			// arrange
			const int totalItemsInBatch = 10;

			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 1, totalItemsInBatch))
			{
				for (int i = 0; i < numberOfEvents; i++)
				{
					_testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(delayBetweenEvents).Ticks);
					_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);
				}
			}

			// assert
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(expectedNumberOfProgressUpdates));
		}

		[TestCase(0, 0, 0)]
		[TestCase(2, 2, 0)]
		[TestCase(3, 0, 3)]
		[TestCase(3, 2, 1)]
		public void AttachToImportJob_ShouldReportProperNumberOfItems(int numberOfItemProcessedEvents, int numberOfItemErrorEvents, int expectedNumberOfItemsProcessed)
		{
			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 1, expectedNumberOfItemsProcessed))
			{
				for (int i = 0; i < numberOfItemProcessedEvents; i++)
				{
					_bulkImportJobStub.Raise(x => x.OnProgress += null, i);
				}

				for (int i = 0; i < numberOfItemErrorEvents; i++)
				{
					_bulkImportJobStub.Raise(x => x.OnItemLevelError += null, new ItemLevelError());
				}

				_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
			}

			// assert
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedNumberOfItemsProcessed, numberOfItemErrorEvents));
		}

		[Test]
		public void AttachToImportJob_ShouldUpdateStatisticsWhenBatchCompletes()
		{
			// arrange
			const int itemsProcessed = 10;
			const int itemsWithErrors = 15;

			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 1, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnComplete += null, CreateJobReport(itemsProcessed, itemsWithErrors));
			}

			// assert
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(itemsProcessed, itemsWithErrors));
		}
		
		[Test]
		public void AttachToImportJob_ShouldUpdateStatisticsWhenFatalExceptionOccurs()
		{
			// arrange
			const int itemsProcessed = 10;
			const int itemsWithErrors = 10;

			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 1, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnFatalException += null, CreateJobReport(itemsProcessed, itemsWithErrors));
			}


			// assert
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(itemsProcessed, itemsWithErrors));
		}

		[Test]
		public void AttachToImportJob_ShouldAggregateProgressFromMultipleBatches()
		{
			// arrange
			const int batchCount = 150;
			int batchId = 0;
			Mock<ISyncImportBulkArtifactJob>[] bulkImportJobs = Enumerable.Range(0, batchCount).Select(_ => _bulkImportJobStub).ToArray();

			//act
			foreach (var bulkImportJob in bulkImportJobs)
			{
				using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batchId++, 1))
				{
					bulkImportJob.Raise(x => x.OnProgress += null, 0);

					bulkImportJob.Raise(x => x.OnProgress += null, 0); // to cover for decrement in OnError handling
					bulkImportJob.Raise(x => x.OnItemLevelError += null, new ItemLevelError());
				}
			}

			_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

			// assert
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(batchCount, batchCount));
		}

		[Test]
		public void AttachToImportJob_ShouldAggregateProgressAndCompleteFromMultipleBatches()
		{
			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 0, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);

				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0); // to cover for decrement in OnError handling
				_bulkImportJobStub.Raise(x => x.OnItemLevelError += null, new ItemLevelError());

				_bulkImportJobStub.Raise(x => x.OnComplete += null, CreateJobReport(1, 1));
			}

			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 1, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);
			}

			_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

			// assert
			const int expectedTransferredItemsCount = 2;
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedTransferredItemsCount, 1));
		}

		[Test]
		public void Disposing_AttachToImportJob_ShouldRemoveAllEventHandlers()
		{
			// arrange
			const int batchCount = 5;

			var bulkImportJobMock = new Mock<ISyncImportBulkArtifactJob>();

			bulkImportJobMock.SetupAdd(m => m.OnProgress += (i) => { });
			bulkImportJobMock.SetupAdd(m => m.OnItemLevelError += (i) => { });
			bulkImportJobMock.SetupAdd(m => m.OnComplete += (i) => { });
			bulkImportJobMock.SetupAdd(m => m.OnFatalException += (i) => { });

			Mock<ISyncImportBulkArtifactJob>[] bulkImportJobs =
				Enumerable.Range(0, batchCount).Select(_ => bulkImportJobMock).ToArray();

			int batchId = 0;

			// act
			foreach (var bulkImportJob in bulkImportJobs)
			{
				using (_sut.AttachToImportJob(bulkImportJob.Object, batchId, 1))
				{
					batchId++;
				}
			}

			// assert
			foreach (var jobMock in bulkImportJobs)
			{
				jobMock.VerifyRemove(m => m.OnProgress -= It.IsAny<IImportNotifier.OnProgressEventHandler>(), Times.Exactly(batchCount));
				jobMock.VerifyRemove(m => m.OnItemLevelError -= It.IsAny<OnSyncImportBulkArtifactJobItemLevelErrorEventHandler>(), Times.Exactly(batchCount));
				jobMock.VerifyRemove(m => m.OnComplete -= It.IsAny<IImportNotifier.OnCompleteEventHandler>(), Times.Exactly(batchCount));
				jobMock.VerifyRemove(m => m.OnFatalException -= It.IsAny<IImportNotifier.OnFatalExceptionEventHandler>(), Times.Exactly(batchCount));
			}
		}
		
		[Test]
		public void AttachToImportJob_ShouldNotStopReportingProgress_WhenProgressUpdaterThrows()
		{
			// arrange
			_jobProgressUpdaterMock.Setup(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()))
				.Throws(new Exception());

			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 0, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);
				_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);
				_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
			}

			// assert
			const int expectedReportCount = 3;
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(expectedReportCount));
		}

		[Test]
		public void AttachToImportJob_ShouldReportCorrectValues_WhenProgressUpdaterThrows()
		{
			// arrange
			_jobProgressUpdaterMock.Setup(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()))
				.Throws(new Exception());

			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 0, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);
				_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

				_bulkImportJobStub.Raise(x => x.OnProgress += null, 0);
				_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
			}

			// assert
			const int expectedReportCount = 2;
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedReportCount, 0), Times.AtLeastOnce);
		}

		[Test]
		public void AttachToImportJob_ShouldNotReportNegativeProcessedItems()
		{
			// act
			using (_sut.AttachToImportJob(_bulkImportJobStub.Object, 0, 1))
			{
				_bulkImportJobStub.Raise(x => x.OnItemLevelError += null, new ItemLevelError());
				_testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
			}

			//assert
			_jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(0, 1));
		}

		[TearDown]
		public void TearDown()
		{
			Dispose();
		}

		private static PropertyInfo GetJobReportTotalRowsPropertyInfo()
		{
			return typeof(JobReport).GetProperty(nameof(JobReport.TotalRows));
		}
		
		private static JobReport CreateJobReport(int itemsProcessed, int itemsWithErrors)
		{
			JobReport jobReport = CreateJobReport();
			var jobError = new JobReport.RowError(0, "", "");
			for (int i = 0; i < itemsWithErrors; i++)
			{
				jobReport.ErrorRows.Add(jobError);
			}
			_jobReportTotalRowsProperty.Value.SetValue(jobReport, itemsProcessed + itemsWithErrors);
			return jobReport;
		}
		
		private static JobReport CreateJobReport()
		{
			JobReport jobReport = (JobReport)Activator.CreateInstance(typeof(JobReport), true);
			return jobReport;
		}

		public void Dispose()
		{
			_sut?.Dispose();
		}
	}
}