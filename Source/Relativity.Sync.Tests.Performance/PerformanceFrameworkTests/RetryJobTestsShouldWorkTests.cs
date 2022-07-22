using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.Performance.Tests;

namespace Relativity.Sync.Tests.Performance.PerformanceFrameworkTests
{
    [TestFixture]
    internal class RetryJobTestsShouldWorkTests : RetryJobsTests
    {
        public RetryJobTestsShouldWorkTests()
            : base("Sample Workspace 25_Docs-8_Errors.zip", 25, 8)
        {
        }

        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);
        }

        public static IEnumerable<TestCaseData> NewTestCase()
        {
#pragma warning disable RG2009 // Hardcoded Numeric Value

            PerformanceTestCase[] testCases = new[]
            {
                new PerformanceTestCase
                {
                    TestCaseName = "Retry 8 docs",
                    CopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
                    NumberOfMappedFields = 0
                }
            };

            return testCases.Select(x => new TestCaseData(x)
            {
                TestName = $"{x.TestCaseName} ({x.ExpectedItemsTransferred} docs, {x.NumberOfMappedFields} fields)"
            });

#pragma warning restore RG2009 // Hardcoded Numeric Value
        }

        [TestCaseSource(nameof(NewTestCase))]
        [Category("PerformanceFrameworkTests")]
        public override async Task Run(PerformanceTestCase testCase)
        {
            await base.Run(testCase);
        }
    }
}
