using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	[Category("RETRY_Jobs")]
	public class RetryJobsTests : PerformanceTestBase
	{
		public RetryJobsTests() : base(WorkspaceType.ARM, "Sync Retries 100k_Docs-30k_Errors.zip", null)
		{
		}

		public static IEnumerable<TestCaseData> TestCases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Retry 30k docs",
					CopyMode = ImportNativeFileCopyMode.SetFileLinks,
					NumberOfMappedFields = 100
				}
			};

			return testCases.Select(x => new TestCaseData(x)
			{
				TestName = $"{x.TestCaseName} ({x.ExpectedItemsTransferred} docs, {x.NumberOfMappedFields} fields)"
			});

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		private async Task SetupAsync(PerformanceTestCase testCase, int? targetWorkspaceId, string savedSearchName)
		{
			Configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOnly;
			Configuration.ImportNativeFileCopyMode = testCase.CopyMode;

			await SetupConfigurationAsync(
				sourceWorkspaceId: _sourceWorkspaceId,
				targetWorkspaceId: targetWorkspaceId,
				savedSearchName: savedSearchName).ConfigureAwait(false);

			IEnumerable<FieldMap> generatedFields = await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields: null).ConfigureAwait(false);
			Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(generatedFields).ToArray());
			if (testCase.MapExtractedText)
			{
				IEnumerable<FieldMap> extractedTextMapping = await GetExtractedTextMappingAsync(SourceWorkspace.ArtifactID, TargetWorkspace.ArtifactID).ConfigureAwait(false);
				Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(extractedTextMapping).ToArray());
			}
			Logger.LogInformation("Fields mapping ready");
		}

		[TestCaseSource(nameof(TestCases))]
		public async Task Run(PerformanceTestCase testCase)
		{
			// Arrange
			const int expectedTotalItems = 100000;
			const int expectedItemsWithErrors = 30000;

			// Sync 30% Of All Documents
			await SetupAsync(testCase, null, "30% Of All Documents").ConfigureAwait(false);
			await RunJobAsync().ConfigureAwait(false);

			// Sync All Documents using AppendOnly to create item level errors
			await SetupAsync(testCase, TargetWorkspace.ArtifactID, "All Documents").ConfigureAwait(false);
			await RunJobAsync().ConfigureAwait(false);

			RelativityObject jobHistory = await Rdos.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId).ConfigureAwait(false);

			int totalItems = (int)jobHistory["Total Items"].Value;
			int itemsTranferred = (int)jobHistory["Items Transferred"].Value;
			int itemsWithErrors = (int)jobHistory["Items with Errors"].Value;

			totalItems.Should().Be(expectedTotalItems);
			itemsTranferred.Should().Be(expectedTotalItems - expectedItemsWithErrors);
			itemsWithErrors.Should().Be(expectedItemsWithErrors);

			// Retry 
			Configuration.JobHistoryToRetryId = Configuration.JobHistoryArtifactId;
			Configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspaceId).ConfigureAwait(false);
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IRetryDataSourceSnapshotConfiguration>(Configuration);

			Stopwatch stopwatch = Stopwatch.StartNew();
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
			stopwatch.Stop();
			TimeSpan elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
			_testTimes.Add(testCase.TestCaseName, elapsedTime);

			Logger.LogInformation("Elapsed time {0} s", elapsedTime.TotalSeconds.ToString("F", CultureInfo.InvariantCulture));

			// ASSERT
			Configuration.TotalRecordsCount.Should().Be(expectedItemsWithErrors);
		}

		private async Task RunJobAsync()
		{
			ConfigurationRdoId = await Rdos.CreateSyncConfigurationRdoAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration).ConfigureAwait(false);
			Logger.LogInformation("Configuration RDO created");

			SyncJobParameters jobParameters = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId);
			SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

			Logger.LogInformation("Starting the job");
			SyncJobState syncJobState = await syncRunner.RunAsync(jobParameters, User.ArtifactID).ConfigureAwait(false);
		}

	}
}