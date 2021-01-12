﻿using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Testing.Identification;
using EnvironmentVariable = System.Environment;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("ReferencePerformance")]
	[TestType.Performance]
	[TestType.MainFlow]
	[TestLevel.L3]
	internal class ReferenceJobsTests : PerformanceTestBase
	{
		private AzureTableHelper _tableHelper;

		public const string _PERFORMANCE_RESULTS_TABLE_NAME = "SyncReferenceJobsPerformanceTestsResults";

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			await UseArmWorkspace(
					"Nightly_Performance_Tests.zip",
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
					TestCaseName = "Nightly-1",
					CopyMode = ImportNativeFileCopyMode.SetFileLinks,
					ExpectedItemsTransferred = 50000,
					MapExtractedText = true,
					NumberOfMappedFields = 50
				}
			};

			return testCases.Select(x => new TestCaseData(x)
			{
				TestName = $"{x.TestCaseName} ({x.ExpectedItemsTransferred} docs, {x.NumberOfMappedFields} fields)"
			});

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[TestCaseSource(nameof(Cases))]
		[IdentifiedTest("8fe8483e-78d4-433d-b638-131d9f11845f")]
		public async Task RunJob(PerformanceTestCase testCase)
		{
			await RunTestCaseAsync(testCase).ConfigureAwait(false);

			await PublishTestResult(testCase).ConfigureAwait(false);
		}

		private Task PublishTestResult(PerformanceTestCase testCase)
		{
			string buildId = EnvironmentVariable.GetEnvironmentVariable("BUILD_ID");
			if (string.IsNullOrEmpty(buildId))
			{
				return Task.CompletedTask;
			}

			TestResult testResult = new TestResult(
				testCase.TestCaseName,
				buildId)
			{
				Duration = TestTimes[testCase.TestCaseName].TotalSeconds
			};

			return _tableHelper.InsertAsync(
				_PERFORMANCE_RESULTS_TABLE_NAME,	
				testResult);
		}
	}
}
