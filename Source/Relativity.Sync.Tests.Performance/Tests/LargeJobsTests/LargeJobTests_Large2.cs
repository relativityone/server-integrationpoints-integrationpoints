using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("LARGE_Jobs")]
	public class LargeJobTests_Large2 : PerformanceTestBase
	{
		public LargeJobTests_Large2()
			: base(WorkspaceType.Relativity, "Large Job Tests - Large-2", null)
		{
		}

		[Test]
		[Category("Large-2")]
		public async Task RunJob()
		{

			PerformanceTestCase testCase = new PerformanceTestCase
			{
				TestCaseName = "Large-2",
				CopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				OverwriteMode = ImportOverwriteMode.OverlayOnly,
				MapExtractedText = true,
				ExpectedItemsTransferred = 400000,
				NumberOfMappedFields = 2,
			};

			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}
	}
}
#pragma warning restore RG2009 // Hardcoded Numeric Value