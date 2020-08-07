using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("ReferencePerformance")]
	public class ReferenceJobsTests : PerformanceTestBase
	{
		public ReferenceJobsTests() : base(WorkspaceType.ARM, "1066387_Small_jobs_tests_20200603140140.zip", null)
		{
		}

		public static IEnumerable<TestCaseData> Cases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Small-1a",
					ExpectedItemsTransferred = 10,
					NumberOfMappedFields = 150
				}
			};

			return testCases.Select(x => new TestCaseData(x)
			{
				TestName = $"{x.TestCaseName} ({x.ExpectedItemsTransferred} docs, {x.NumberOfMappedFields} fields)"
			});

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[TestCaseSource(nameof(Cases))]
		public async Task RunJob(PerformanceTestCase testCase)
		{
			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}
	}
}
