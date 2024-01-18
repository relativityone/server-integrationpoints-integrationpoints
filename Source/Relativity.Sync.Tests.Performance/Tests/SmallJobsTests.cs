using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Helpers;

namespace Relativity.Sync.Tests.Performance.Tests
{
    [TestFixture]
    [Category("SMALL_Jobs")]
    internal class SmallJobsTests : PerformanceTestBase
    {
        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);

            await UseArmWorkspaceAsync(
                    "Small_jobs_tests.zip",
                    null)
                .ConfigureAwait(false);
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
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-1b",
                    ExpectedItemsTransferred = 200,
                    NumberOfMappedFields = 85,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-1c",
                    ExpectedItemsTransferred = 100,
                    NumberOfMappedFields = 190,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-1d",
                    ExpectedItemsTransferred = 200,
                    NumberOfMappedFields = 7,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-2a",
                    ExpectedItemsTransferred = 10,
                    NumberOfMappedFields = 150,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-2b",
                    ExpectedItemsTransferred = 200,
                    NumberOfMappedFields = 85,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-2c",
                    ExpectedItemsTransferred = 100,
                    NumberOfMappedFields = 190,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-2d",
                    ExpectedItemsTransferred = 200,
                    NumberOfMappedFields = 7,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-3a",
                    ExpectedItemsTransferred = 20,
                    NumberOfMappedFields = 300,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-3b",
                    ExpectedItemsTransferred = 200,
                    NumberOfMappedFields = 85,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-3c",
                    ExpectedItemsTransferred = 100,
                    NumberOfMappedFields = 190,
                },
                new PerformanceTestCase
                {
                    TestCaseName = "Small-3d",
                    ExpectedItemsTransferred = 200,
                    NumberOfMappedFields = 7,
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
