using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core.BatchProcess;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueue.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class BatchManagerBaseTests : TestBase
    {
        private BatchManagerTest _instance;
        private Job _job;
        private const string _JOB_DETAILS = "{\"BatchInstance\":\"2b7bda1b-11c9-4349-b446-ae5c8ca2c408\"}";

        [SetUp]
        public override void SetUp()
        {
            _job = JobHelper.GetJob(1, null, null, 1, 1, 111, 222, TaskType.SyncEntityManagerWorker, new DateTime(), null, _JOB_DETAILS, 0, new DateTime(), 1, null, null);
            IHelper helper = Substitute.For<IHelper>();
            _instance = new BatchManagerTest(helper, new EmptyDiagnosticLog());
        }

        [TestCase(0)]
        [TestCase(999)]
        [TestCase(1000)]
        [TestCase(1001)]
        [TestCase(10001)]
        public void BatchTask_CorrectTotalItems(int numItems)
        {
            // ARRANGE
            IEnumerable<string> batchIds = CreateItems(numItems);

            // ACT
            long total = _instance.BatchTask(_job, batchIds);

            // ASSERT
            Assert.AreEqual(numItems, total);
        }

        [TestCase(0, 0)]
        [TestCase(999, 1)]
        [TestCase(1000, 1)]
        [TestCase(1001, 2)]
        [TestCase(10001, 11)]
        public void BatchTask_CorrectBatching(int numItems, int expectedBatches)
        {
            // ARRANGE
            IEnumerable<string> batchIds = CreateItems(numItems);

            // ACT
            _instance.BatchTask(_job, batchIds);

            // ASSERT
            Assert.AreEqual(expectedBatches, _instance.BatchCount);
        }

        private IEnumerable<string> CreateItems(int numItems)
        {
            List<string> batchIds = new List<string>();
            for (int i = 0; i < numItems; i++)
            {
                batchIds.Add(i.ToString());
            }
            return batchIds;
        } 

        private class BatchManagerTest : BatchManagerBase<string>
        {
            public int BatchCount { get; private set; }

            public override IEnumerable<string> GetUnbatchedIDs(Job job)
            {
                throw new NotImplementedException();
            }

            public override void CreateBatchJob(Job job, List<string> batchIDs)
            {
                BatchCount++;
            }

            public BatchManagerTest(IHelper helper, IDiagnosticLog diagnosticLog) : base(helper, diagnosticLog)
            {
            }
        }
    }
}
