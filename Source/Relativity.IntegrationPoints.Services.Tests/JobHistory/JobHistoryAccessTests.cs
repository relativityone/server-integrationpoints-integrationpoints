using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.JobHistory;

namespace Relativity.IntegrationPoints.Services.Tests.JobHistory
{
    [TestFixture, Category("Unit")]
    public class JobHistoryAccessTests : TestBase
    {
        private JobHistoryAccess _jobHistoryAccess;


        public override void SetUp()
        {
            _jobHistoryAccess = new JobHistoryAccess(new DestinationParser());
        }
        
        [TestCaseSource(nameof(TestCases))]
        public void ItShouldFilterWorkspaces(TestData testData)
        {
            var result = _jobHistoryAccess.Filter(testData.JobHistories, testData.Workspaces);
            Assert.That(result.Select(x => x.DestinationWorkspace), Is.EquivalentTo(testData.ExpectedJobHistories.Select(x => x.DestinationWorkspace)));
        }

        public static IEnumerable<TestData> TestCases()
        {
            yield return TestData.EmptyJobHistoriesList();
            yield return TestData.EmptyWorkspacesList();
            yield return TestData.FullAccess();
            yield return TestData.TestCase1();
            yield return TestData.TestCase2();
            yield return TestData.TestCase3();
        }

        public class TestData
        {
            public IList<JobHistoryModel> JobHistories { get; private set; }
            public IList<JobHistoryModel> ExpectedJobHistories { get; private set; }
            public IList<int> Workspaces { get; private set; }

            public static TestData EmptyJobHistoriesList()
            {
                return new TestData
                {
                    JobHistories = new List<JobHistoryModel>(),
                    ExpectedJobHistories = new List<JobHistoryModel>(),
                    Workspaces = new List<int> {1, 2, 3}
                };
            }

            public static TestData EmptyWorkspacesList()
            {
                return new TestData
                {
                    JobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(2)
                    },
                    ExpectedJobHistories = new List<JobHistoryModel>(),
                    Workspaces = new List<int>()
                };
            }

            public static TestData FullAccess()
            {
                return new TestData
                {
                    JobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(2)
                    },
                    ExpectedJobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(2)
                    },
                    Workspaces = new List<int> {2, 1}
                };
            }

            public static TestData TestCase1()
            {
                return new TestData
                {
                    JobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(2),
                        CreateJobHistory(3)
                    },
                    ExpectedJobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(3)
                    },
                    Workspaces = new List<int> {3, 1}
                };
            }

            public static TestData TestCase2()
            {
                return new TestData
                {
                    JobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(3)
                    },
                    ExpectedJobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(3)
                    },
                    Workspaces = new List<int> {3, 4, 5}
                };
            }

            public static TestData TestCase3()
            {
                return new TestData
                {
                    JobHistories = new List<JobHistoryModel>
                    {
                        CreateJobHistory(1),
                        CreateJobHistory(2)
                    },
                    ExpectedJobHistories = new List<JobHistoryModel>(),
                    Workspaces = new List<int> { 3, 4, 5 }
                };
            }

            private static JobHistoryModel CreateJobHistory(int workspaceId)
            {
                return new JobHistoryModel
                {
                    DestinationWorkspace = $"workspace - {workspaceId}"
                };
            }
        }
    }
}