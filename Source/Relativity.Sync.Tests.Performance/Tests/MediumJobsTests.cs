using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("MEDIUM_Jobs")]
	internal class MediumJobsTests : PerformanceTestBase
	{
		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			await UseArmWorkspaceAsync(
					"Medium_jobs_tests.zip",
					"Medium_jobs_tests_-_Destination.zip")
				.ConfigureAwait(false);
		}

		public static IEnumerable<TestCaseData> Cases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Medium-1",
					CopyMode = ImportNativeFileCopyMode.SetFileLinks,
					OverwriteMode = ImportOverwriteMode.AppendOnly,
					MapExtractedText = false,
					ExpectedItemsTransferred = 30000,
					NumberOfMappedFields = 200,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Medium-2",
					CopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
					OverwriteMode = ImportOverwriteMode.OverlayOnly,
					MapExtractedText = true,
					ExpectedItemsTransferred = 50000,
					NumberOfMappedFields = 2,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Medium-2a",
					CopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
					OverwriteMode = ImportOverwriteMode.AppendOnly,
					MapExtractedText = true,
					ExpectedItemsTransferred = 50000,
					NumberOfMappedFields = 2,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Medium-3",
					CopyMode = ImportNativeFileCopyMode.CopyFiles,
					OverwriteMode = ImportOverwriteMode.AppendOnly,
					MapExtractedText = false,
					ExpectedItemsTransferred = 5000,
					NumberOfMappedFields = 100,
				},
				new PerformanceTestCase
				{
					TestCaseName = "Medium-4",
					CopyMode = ImportNativeFileCopyMode.CopyFiles,
					OverwriteMode = ImportOverwriteMode.AppendOnly,
					MapExtractedText = false,
					ExpectedItemsTransferred = 5000,
					NumberOfMappedFields = 300,
				},
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
