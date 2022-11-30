using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.Stubs;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

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

        [SetUp]
        public void SetUp()
        {
            _jobProgressUpdaterMock = new Mock<IJobProgressUpdater>();
            _bulkImportJobStub = new Mock<ISyncImportBulkArtifactJob>();

            _testScheduler = new TestScheduler();
            _sut = new JobProgressHandler(_jobProgressUpdaterMock.Object, Enumerable.Empty<IBatch>(), _testScheduler);
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
            // Arrange
            const int totalItemsInBatch = 10;

            BatchStub batch = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = totalItemsInBatch };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                for (int i = 0; i < numberOfEvents; i++)
                {
                    _testScheduler.AdvanceBy(TimeSpan.FromMilliseconds(delayBetweenEvents).Ticks);
                    _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                }
            }

            // Assert
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(expectedNumberOfProgressUpdates));
        }

        [TestCase(0, 0, 0)]
        [TestCase(2, 2, 0)]
        [TestCase(3, 0, 3)]
        [TestCase(3, 2, 1)]
        public void AttachToImportJob_ShouldReportProperNumberOfItems(int numberOfItemProcessedEvents, int numberOfItemErrorEvents, int expectedNumberOfItemsProcessed)
        {
            // Arrange
            BatchStub batch = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = expectedNumberOfItemsProcessed };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                for (int i = 0; i < numberOfItemProcessedEvents; i++)
                {
                    _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(i));
                }

                for (int i = 0; i < numberOfItemErrorEvents; i++)
                {
                    _bulkImportJobStub.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));
                }

                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Aassert
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedNumberOfItemsProcessed, numberOfItemErrorEvents));
        }

        [Test]
        public void AttachToImportJob_ShouldUpdateStatisticsWhenBatchCompletes()
        {
            // Arrange
            const int itemsProcessed = 10;
            const int itemsWithErrors = 15;
            BatchStub batch = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = 1 };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                _bulkImportJobStub.Raise(x => x.OnComplete += null, CreateJobReport(itemsProcessed, itemsWithErrors));
            }

            // Assert
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(itemsProcessed, itemsWithErrors));
        }

        [Test]
        public void AttachToImportJob_ShouldUpdateStatisticsWhenFatalExceptionOccurs()
        {
            // Arrange
            const int itemsProcessed = 10;
            const int itemsWithErrors = 10;
            BatchStub batch = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = 1 };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                _bulkImportJobStub.Raise(x => x.OnFatalException += null, CreateJobReport(itemsProcessed, itemsWithErrors));
            }

            // Assert
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(itemsProcessed, itemsWithErrors));
        }

        [Test]
        public void AttachToImportJob_ShouldAggregateProgressFromMultipleBatches()
        {
            // Arrange
            const int batchCount = 150;
            int batchId = 0;
            Mock<ISyncImportBulkArtifactJob>[] bulkImportJobs = Enumerable.Range(0, batchCount).Select(_ => _bulkImportJobStub).ToArray();

            // Act
            foreach (var bulkImportJob in bulkImportJobs)
            {
                BatchStub batch = new BatchStub { ArtifactId = batchId++, TotalDocumentsCount = 1 };
                using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
                {
                    bulkImportJob.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));

                    bulkImportJob.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0)); // to cover for decrement in OnError handling
                    bulkImportJob.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));
                }
            }

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

            // Assert
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(batchCount, batchCount));
        }

        [Test]
        public void AttachToImportJob_ShouldAggregateProgressAndCompleteFromMultipleBatches()
        {
            // Arrange
            BatchStub batch0 = new BatchStub() { TotalDocumentsCount = 1 };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch0))
            {
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));

                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0)); // to cover for decrement in OnError handling
                _bulkImportJobStub.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));

                _bulkImportJobStub.Raise(x => x.OnComplete += null, CreateJobReport(1, 1));
            }

            BatchStub batch1 = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = 1 };
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch1))
            {
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
            }

            _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

            // Assert
            const int expectedTransferredItemsCount = 2;
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedTransferredItemsCount, 1));
        }

        [Test]
        public void Disposing_AttachToImportJob_ShouldRemoveAllEventHandlers()
        {
            //// Arrange
            // const int batchCount = 5;

            // var bulkImportJobMock = new Mock<ISyncImportBulkArtifactJob>();

            // bulkImportJobMock.SetupAdd(m => m.OnProgress += (i) => { });
            // bulkImportJobMock.SetupAdd(m => m.OnItemLevelError += (i) => { });
            // bulkImportJobMock.SetupAdd(m => m.OnComplete += (i) => { });
            // bulkImportJobMock.SetupAdd(m => m.OnFatalException += (i) => { });

            // Mock<ISyncImportBulkArtifactJob>[] bulkImportJobs =
            //    Enumerable.Range(0, batchCount).Select(_ => bulkImportJobMock).ToArray();

            // int batchId = 0;

            //// Act
            // foreach (var bulkImportJob in bulkImportJobs)
            // {
            //    BatchStub batch = new BatchStub() { ArtifactId = batchId, TotalDocumentsCount = 1 };
            //    using (_sut.AttachToImportJob(bulkImportJob.Object, batch))
            //    {
            //        batchId++;
            //    }
            // }

            //// Assert
            // foreach (var jobMock in bulkImportJobs)
            // {
            //    jobMock.VerifyRemove(m => m.OnProgress -= It.IsAny<SyncJobEventHandler<ImportApiJobProgress>>(), Times.Exactly(batchCount));
            //    jobMock.VerifyRemove(m => m.OnItemLevelError -= It.IsAny<SyncJobEventHandler<ItemLevelError>>(), Times.Exactly(batchCount));
            //    jobMock.VerifyRemove(m => m.OnComplete -= It.IsAny<SyncJobEventHandler<ImportApiJobStatistics>>(), Times.Exactly(batchCount));
            //    jobMock.VerifyRemove(m => m.OnFatalException -= It.IsAny<SyncJobEventHandler<ImportApiJobStatistics>>(), Times.Exactly(batchCount));
            // }
        }

        [Test]
        public void AttachToImportJob_ShouldNotStopReportingProgress_WhenProgressUpdaterThrows()
        {
            // Arrange
            _jobProgressUpdaterMock.Setup(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());
            BatchStub batch = new BatchStub() { TotalDocumentsCount = 1 };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Assert
            const int expectedReportCount = 3;
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(expectedReportCount));
        }

        [Test]
        public void AttachToImportJob_ShouldReportCorrectValues_WhenProgressUpdaterThrows()
        {
            // Arrange
            _jobProgressUpdaterMock.Setup(x => x.UpdateJobProgressAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception());
            BatchStub batch = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = 1 };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);

                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Assert
            const int expectedReportCount = 2;
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedReportCount, 0), Times.AtLeastOnce);
        }

        [Test]
        public void AttachToImportJob_ShouldNotReportNegativeProcessedItems()
        {
            // Act
            BatchStub batch = new BatchStub() { ArtifactId = 1, TotalDocumentsCount = 1 };

            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                _bulkImportJobStub.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));
                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // assert
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(0, 1));
        }

        [Test]
        public void AttachToImportJob_ShouldUpdateWithTotalTransferredAndFailedItems_WhenThereWerePreviouslyExecutedBatches()
        {
            // Arrange
            int batchId = 0;

            const int previouslyExecutedBatchFailedItemsCount = 2;
            const int previouslyExecutedBatchTransferredItemsCount = 8;
            IBatch[] previouslyExecutedBatches = new[]
            {
                new BatchStub { ArtifactId = batchId++, FailedItemsCount = previouslyExecutedBatchFailedItemsCount, TransferredItemsCount = previouslyExecutedBatchTransferredItemsCount },
                new BatchStub { ArtifactId = batchId++, FailedItemsCount = previouslyExecutedBatchFailedItemsCount, TransferredItemsCount = previouslyExecutedBatchTransferredItemsCount }
            };

            BatchStub batch = new BatchStub { ArtifactId = batchId, TotalDocumentsCount = 2 };

            // Act
            JobProgressHandler sut = new JobProgressHandler(_jobProgressUpdaterMock.Object, previouslyExecutedBatches, _testScheduler);
            using (sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                // report ItemLevelError
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _bulkImportJobStub.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));

                // report one success
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));

                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Assert
            int expectedTransferredRecordsCount = (previouslyExecutedBatchTransferredItemsCount * previouslyExecutedBatches.Length) + 1;
            int expectedFailedRecordsCount = (previouslyExecutedBatchFailedItemsCount * previouslyExecutedBatches.Length) + 1;
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedTransferredRecordsCount, expectedFailedRecordsCount));
        }

        [Test]
        public void AttachToImportJob_ShouldUpdateWithTotalTransferredAndFailedItems_WhenThereWereTransferredAndFailedItemsPreviously()
        {
            // Arrange
            const int initialFailedItemsCount = 5;
            const int initialTransferredItemsCount = 6;

            BatchStub batch = new BatchStub() { FailedItemsCount = initialFailedItemsCount, TransferredItemsCount = initialTransferredItemsCount };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                // report ItemLevelError
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _bulkImportJobStub.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));

                // report one success
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));

                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Assert
            const int expectedTransferredRecordsCount = initialTransferredItemsCount + 1;
            const int expectedFailedRecordsCount = initialFailedItemsCount + 1;
            _jobProgressUpdaterMock.Verify(x => x.UpdateJobProgressAsync(expectedTransferredRecordsCount, expectedFailedRecordsCount));
        }

        [Test]
        public void GetBatchItemsProcessedCount_ShouldNotIncludePreviouslyTransferredItemsCount()
        {
            // Arrange
            const int initialFailedItemsCount = 5;
            const int initialTransferredItemsCount = 6;

            BatchStub batch = new BatchStub { ArtifactId = -1, FailedItemsCount = initialFailedItemsCount, TransferredItemsCount = initialTransferredItemsCount };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                // report one success
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));

                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Assert
            _sut.GetBatchItemsProcessedCount(-1).Should().Be(1);
        }

        [Test]
        public void GetBatchItemsFailedCount_Should_ReportCurrentlyTransferredItems()
        {
            // Arrange
            const int initialFailedItemsCount = 5;
            const int initialTransferredItemsCount = 6;

            BatchStub batch = new BatchStub { ArtifactId = -1, FailedItemsCount = initialFailedItemsCount, TransferredItemsCount = initialTransferredItemsCount };

            // Act
            using (_sut.AttachToImportJob(_bulkImportJobStub.Object, batch))
            {
                // report ItemLevelError
                _bulkImportJobStub.Raise(x => x.OnProgress += null, new ImportApiJobProgress(0));
                _bulkImportJobStub.Raise(x => x.OnItemLevelError += null, default(ItemLevelError));

                _testScheduler.AdvanceBy(TimeSpan.FromSeconds(_THROTTLE_SECONDS).Ticks);
            }

            // Assert
            _sut.GetBatchItemsFailedCount(-1).Should().Be(1);
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        private static ImportApiJobStatistics CreateJobReport(int itemsProcessed, int itemsWithErrors)
        {
            return new ImportApiJobStatistics(itemsProcessed + itemsWithErrors, itemsWithErrors, 0, 0);
        }

        public void Dispose()
        {
            _sut?.Dispose();
        }
    }
}
