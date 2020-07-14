using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("LARGE_Jobs")]
	public class LargeJobTests_Large1 : PerformanceTestBase
	{
		public LargeJobTests_Large1()
			: base(WorkspaceType.Relativity, "Large Job Tests - Large-1", null)
		{
		}

		[Test]
		[Category("Large-1")]
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

			await RunTestCase(testCase).ConfigureAwait(false);
		}
	}
}
#pragma warning restore RG2009 // Hardcoded Numeric Value