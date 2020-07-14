﻿using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;

#pragma warning disable RG2009 // Hardcoded Numeric Value
namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("LARGE_Jobs")]
	public class LargeJobTests_Large5 : PerformanceTestBase
	{
		public LargeJobTests_Large5()
			: base(WorkspaceType.Relativity, "Large Job Tests - Large-5", null)
		{
		}

		[Test]
		[Category("Large-5")]
		public async Task RunJob()
		{

			PerformanceTestCase testCase = new PerformanceTestCase
			{
				TestCaseName = "Large-5",
				CopyMode = ImportNativeFileCopyMode.CopyFiles,
				OverwriteMode = ImportOverwriteMode.AppendOnly,
				MapExtractedText = false,
				ExpectedItemsTransferred = 250000,
				NumberOfMappedFields = 200,
			};

			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}
	}
}
#pragma warning restore RG2009 // Hardcoded Numeric Value