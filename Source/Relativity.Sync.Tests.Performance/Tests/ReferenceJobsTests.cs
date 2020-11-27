using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EnvironmentVariable = System.Environment;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("ReferencePerformance")]
	internal class ReferenceJobsTests : PerformanceTestBase
	{
		private AzureTableHelper _tableHelper;

		public const string _PERFORMANCE_RESULTS_TABLE_NAME = "SyncReferenceJobsPerformanceTestsResults";
		
		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			await UseExistingWorkspace(
					"Sample Workspace",
					null)
				.ConfigureAwait(false);

			_tableHelper = AzureTableHelper.CreateFromTestConfig();
		}

		public static IEnumerable<TestCaseData> Cases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Reference-1",
					CopyMode = ImportNativeFileCopyMode.SetFileLinks,
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

			await PublishTestResult(testCase).ConfigureAwait(false);
		}

		private Task PublishTestResult(PerformanceTestCase testCase)
		{
			TestResult testResult = new TestResult(
				testCase.TestCaseName,
				EnvironmentVariable.GetEnvironmentVariable("BUILD_ID"))
			{
				Duration = TestTimes[testCase.TestCaseName].TotalSeconds
			};

			return _tableHelper.InsertAsync(
				_PERFORMANCE_RESULTS_TABLE_NAME,	
				testResult);
		}
	}
}
