using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("LARGE_Jobs-Large-3")]
	public class LargeJobTests_Large3 : PerformanceTestBase
	{
		public LargeJobTests_Large3() : base(WorkspaceType.Relativity, "Large Job Tests - Large-3 [DO NOT DELETE]", null)
		{
		}

		[Test]
		public async Task RunJob()
		{
			PerformanceTestCase testCase = new PerformanceTestCase
			{
				TestCaseName = "Large-3",
				CopyMode = ImportNativeFileCopyMode.CopyFiles,
				OverwriteMode = ImportOverwriteMode.AppendOnly,
				MapExtractedText = false,
				ExpectedItemsTransferred = 500000,
				NumberOfMappedFields = 100,
			};

			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}
	}
}
#pragma warning restore RG2009 // Hardcoded Numeric Value