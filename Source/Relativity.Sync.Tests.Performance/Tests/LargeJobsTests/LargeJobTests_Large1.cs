using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("LARGE_Jobs-Large-1")]
	internal class LargeJobTests_Large1 : PerformanceTestBase
	{
		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			await UseExistingWorkspaceAsync(
					"Large Job Tests - Large-1 [DO NOT DELETE]",
					null)
				.ConfigureAwait(false);
		}

		[Test]
		public async Task RunJob()
		{
			PerformanceTestCase testCase = new PerformanceTestCase
			{
				TestCaseName = "Large-1",
				CopyMode = ImportNativeFileCopyMode.SetFileLinks,
				OverwriteMode = ImportOverwriteMode.AppendOnly,
				MapExtractedText = false,
				ExpectedItemsTransferred = 250000,
				NumberOfMappedFields = 350,
			};

			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}
	}
}
#pragma warning restore RG2009 // Hardcoded Numeric Value