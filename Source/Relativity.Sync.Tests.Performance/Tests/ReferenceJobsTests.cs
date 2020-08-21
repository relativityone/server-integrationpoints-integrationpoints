using NUnit.Framework;
using Relativity.Sync.Configuration;
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
		public ReferenceJobsTests() : base(WorkspaceType.ARM, "Performance_Reference_Workspace.zip", null)
		{
		}

		public static IEnumerable<TestCaseData> Cases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Reference-1",
					CopyMode = ImportNativeFileCopyMode.CopyFiles,
					ExpectedItemsTransferred = 10,
					MapExtractedText = true,
					NumberOfMappedFields = 15
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
