using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.Performance.Tests;

namespace Relativity.Sync.Tests.Performance.PerformanceFrameworkTests
{
	[TestFixture]
	[Category("PerformanceFrameworkTests")]
	internal class UseArmedSourceWorkspaceTests : PerformanceTestBase
	{
		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			await UseArmWorkspaceAsync(
					"SmallSaltPepperWorkspace.zip",
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
					TestCaseName = "All Documents",
					ExpectedItemsTransferred = 5,
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

		[TestCaseSource(nameof(Cases))]
		public async Task RunJob(PerformanceTestCase testCase)
		{
			await RunTestCaseAsync(testCase).ConfigureAwait(false);
		}

		/// <summary>
		/// For Testing Purpose - Running tests on existing source workspace should not remove it after finish.
		/// </summary>
		protected override async Task ChildSuiteTeardown()
		{
			await base.ChildSuiteTeardown().ConfigureAwait(false);

			var sourceWorkspace = await Environment.GetWorkspaceAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);

			sourceWorkspace.Should().NotBeNull();
		}
	}
}
