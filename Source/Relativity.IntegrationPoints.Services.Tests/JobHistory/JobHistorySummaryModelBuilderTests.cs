using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.JobHistory;

namespace Relativity.IntegrationPoints.Services.Tests.JobHistory
{
    [TestFixture, Category("Unit")]
    public class JobHistorySummaryModelBuilderTests : TestBase
    {
        private JobHistorySummaryModelBuilder _summaryModelBuilder;

        public override void SetUp()
        {
            _summaryModelBuilder = new JobHistorySummaryModelBuilder();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void ItShouldCountTotalAvailable(int expectedTotalAvailable)
        {
            int page = 0;
            int pageSize = 0;

            IList<JobHistoryModel> data = new List<JobHistoryModel>();
            for (int i = 0; i < expectedTotalAvailable; i++)
            {
                data.Add(new JobHistoryModel());
            }

            var summaryModel = _summaryModelBuilder.Create(page, pageSize, data);

            Assert.That(summaryModel.TotalAvailable, Is.EqualTo(expectedTotalAvailable));
        }


        [Test]
        [TestCase(new[] {0, 0, 0}, 0)]
        [TestCase(new[] {10, 5, 1}, 16)]
        [TestCase(new[] {999}, 999)]
        [TestCase(new[] {1, 2, 3}, 6)]
        public void ItShouldCountTotalDocumentsPushed(int[] documentsPushed, int expectedResult)
        {
            int page = 0;
            int pageSize = 0;

            IList<JobHistoryModel> data = new List<JobHistoryModel>();
            for (int i = 0; i < documentsPushed.Length; i++)
            {
                data.Add(new JobHistoryModel
                {
                    ItemsTransferred = documentsPushed[i]
                });
            }

            var summaryModel = _summaryModelBuilder.Create(page, pageSize, data);

            Assert.That(summaryModel.TotalDocumentsPushed, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(10, 1)]
        [TestCase(10, 0)]
        [TestCase(10, 10)]
        [TestCase(1, 10)]
        [TestCase(0, 10)]
        [TestCase(1, 5)]
        [TestCase(5, 20)]
        public void ItShouldRetrieveData(int page, int pageSize)
        {
            var data = new List<JobHistoryModel>();
            var expectedData = data.Skip(page*pageSize).Take(pageSize);

            var summaryModel = _summaryModelBuilder.Create(page, pageSize, data);

            Assert.That(summaryModel.Data, Is.EquivalentTo(expectedData));
        }
    }
}