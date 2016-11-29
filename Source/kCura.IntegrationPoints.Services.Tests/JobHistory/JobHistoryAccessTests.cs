using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Services.JobHistory;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.JobHistory
{
	public class JobHistoryAccessTests : TestBase
	{
		private JobHistoryAccess _jobHistoryAccess;


		public override void SetUp()
		{
			_jobHistoryAccess = new JobHistoryAccess(new DestinationWorkspaceParser());
		}

		[Test]
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
			public IList<Data.JobHistory> JobHistories;
			public IList<Data.JobHistory> ExpectedJobHistories;
			public IList<int> Workspaces;

			public static TestData EmptyJobHistoriesList()
			{
				return new TestData
				{
					JobHistories = new List<Data.JobHistory>(),
					ExpectedJobHistories = new List<Data.JobHistory>(),
					Workspaces = new List<int> {1, 2, 3}
				};
			}

			public static TestData EmptyWorkspacesList()
			{
				return new TestData
				{
					JobHistories = new List<Data.JobHistory>
					{
						CreateJobHistory(1),
						CreateJobHistory(2)
					},
					ExpectedJobHistories = new List<Data.JobHistory>(),
					Workspaces = new List<int>()
				};
			}

			public static TestData FullAccess()
			{
				return new TestData
				{
					JobHistories = new List<Data.JobHistory>
					{
						CreateJobHistory(1),
						CreateJobHistory(2)
					},
					ExpectedJobHistories = new List<Data.JobHistory>
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
					JobHistories = new List<Data.JobHistory>
					{
						CreateJobHistory(1),
						CreateJobHistory(2),
						CreateJobHistory(3)
					},
					ExpectedJobHistories = new List<Data.JobHistory>
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
					JobHistories = new List<Data.JobHistory>
					{
						CreateJobHistory(1),
						CreateJobHistory(3)
					},
					ExpectedJobHistories = new List<Data.JobHistory>
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
					JobHistories = new List<Data.JobHistory>
					{
						CreateJobHistory(1),
						CreateJobHistory(2)
					},
					ExpectedJobHistories = new List<Data.JobHistory>(),
					Workspaces = new List<int> { 3, 4, 5 }
				};
			}

			private static Data.JobHistory CreateJobHistory(int workspaceId)
			{
				return new Data.JobHistory
				{
					DestinationWorkspace = $"workspace - {workspaceId}"
				};
			}
		}
	}
}