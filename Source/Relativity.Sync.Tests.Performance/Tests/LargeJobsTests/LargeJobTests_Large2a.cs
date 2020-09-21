using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("LARGE_Jobs-Large-2a")]
	public class LargeJobTests_Large2a : PerformanceTestBase
	{
		public LargeJobTests_Large2a() : base(WorkspaceType.Relativity, "Large Job Tests - Large-2 [DO NOT DELETE]", null)
		{
		}

		[Test]
		public async Task RunJob()
		{
			PerformanceTestCase testCase = new PerformanceTestCase
			{
				TestCaseName = "Large-2a",
				CopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				OverwriteMode = ImportOverwriteMode.AppendOnly,
				MapExtractedText = true,
				ExpectedItemsTransferred = 400000,
				NumberOfMappedFields = 0,
			};

			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}
	}
}
#pragma warning restore RG2009 // Hardcoded Numeric Value