using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
    [TestFixture]
    [Category("LARGE_Jobs-Large-4")]
    internal class LargeJobTests_Large4 : PerformanceTestBase
    {
        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);

            await UseExistingWorkspaceAsync(
                    "Large Job Tests - Large-4 [DO NOT DELETE]",
                    "Large Job Tests - Destination-4 [DO NOT DELETE]")
                .ConfigureAwait(false);
        }

        [Test]
        public async Task RunJob()
        {
            PerformanceTestCase testCase = new PerformanceTestCase
            {
                TestCaseName = "Large-4",
                CopyMode = ImportNativeFileCopyMode.CopyFiles,
                OverwriteMode = ImportOverwriteMode.OverlayOnly,
                MapExtractedText = false,
                ExpectedItemsTransferred = 250000,
                NumberOfMappedFields = 100,
            };

            await RunTestCaseAsync(testCase).ConfigureAwait(false);
        }
    }
}
#pragma warning restore RG2009 // Hardcoded Numeric Value