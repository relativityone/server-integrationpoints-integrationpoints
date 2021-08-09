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
	internal class UseExistingSourceAndDestinationWorkspaceTests : PerformanceTestBase
	{
		public const string _WORKSPACE_NAME = "Sample Workspace";
		public const string _DESTINATION_WORKSPACE_NAME = "Sample Workspace - Destination";

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			var sampleWorkspace = await Environment.GetWorkspaceAsync(_WORKSPACE_NAME).ConfigureAwait(false);
			await Environment.CreateFieldsInWorkspaceAsync(sampleWorkspace.ArtifactID);

			await Environment
				.CreateWorkspaceAsync(_DESTINATION_WORKSPACE_NAME, _WORKSPACE_NAME).ConfigureAwait(false);

			await UseExistingWorkspaceAsync(
					_WORKSPACE_NAME,
					_DESTINATION_WORKSPACE_NAME)
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
					ExpectedItemsTransferred = 25,
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
		/// For Testing Purpose - Running tests on existing workspaces should not remove them after finish.
		/// </summary>
		protected override async Task ChildSuiteTeardown()
		{
			var sampleWorkspace = await Environment.GetWorkspaceAsync(_WORKSPACE_NAME).ConfigureAwait(false);

			sampleWorkspace.Should().NotBeNull();

			var destinationWorkspace = await Environment.GetWorkspaceAsync(_DESTINATION_WORKSPACE_NAME).ConfigureAwait(false);

			destinationWorkspace.Should().NotBeNull();
		}
	}
}
