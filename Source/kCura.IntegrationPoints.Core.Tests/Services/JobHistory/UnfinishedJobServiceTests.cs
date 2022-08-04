using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
    [TestFixture, Category("Unit")]
    public class UnfinishedJobServiceTests : TestBase
    {
        private IRelativityObjectManager _objectManager;
        private UnfinishedJobService _sut;

        private const int _WORKSPACE_ID = 551970;

        public override void SetUp()
        {
            _objectManager = Substitute.For<IRelativityObjectManager>();

            IRelativityObjectManagerFactory relativityObjectManagerFactory = Substitute.For<IRelativityObjectManagerFactory>();
            relativityObjectManagerFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_objectManager);

            _sut = new UnfinishedJobService(relativityObjectManagerFactory);
        }

        [Test]
        public void ItShouldQueryForUnfinishedJobs()
        {
            // ACT
            _sut.GetUnfinishedJobs(_WORKSPACE_ID);

            // ASSERT
            _objectManager.Received(1).Query<Data.JobHistory>(Arg.Is<QueryRequest>(x => CheckCondition(x)));
        }

        private bool CheckCondition(QueryRequest query)
        {
            return (query.Condition.Contains(JobStatusChoices.JobHistoryPending.Guids.FirstOrDefault().ToString()) &&
                    query.Condition.Contains(JobStatusChoices.JobHistoryProcessing.Guids.FirstOrDefault().ToString()) &&
                    query.Condition.Contains(JobStatusChoices.JobHistoryStopping.Guids.FirstOrDefault().ToString()));
        }
    }
}