using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
    [TestFixture]
    [Category("LARGE_Jobs-Large-2")]
    internal class LargeJobTests_Large2 : PerformanceTestBase
    {
        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);

            await UseExistingWorkspaceAsync(
                    "Large Job Tests - Large-2 [DO NOT DELETE]",
                    "Large Job Tests - Destination-2 [DO NOT DELETE]")
                .ConfigureAwait(false);
        }

        [Test]
        public async Task RunJob()
        {
            PerformanceTestCase testCase = new PerformanceTestCase
            {
                TestCaseName = "Large-2",
                CopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
                OverwriteMode = ImportOverwriteMode.OverlayOnly,
                MapExtractedText = true,
                ExpectedItemsTransferred = 400000,
                NumberOfMappedFields = 0,
            };

            await RunTestCaseAsync(testCase).ConfigureAwait(false);
        }
    }
}
#pragma warning restore RG2009 // Hardcoded Numeric Value