using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
	public class RetryJobsTests : PerformanceTestBase
	{
		public RetryJobsTests() 
			: base("1035267_Sync_Retries_-_100k_Docs,_10GB_Natives_20200618144941.zip")
		{
		}

		public static IEnumerable<TestCaseData> TestCases()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value

			PerformanceTestCase[] testCases = new[]
			{
				new PerformanceTestCase
				{
					TestCaseName = "Retry",
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
				sourceWorkspaceId: _sourceWorkspaceIdArm,
				targetWorkspaceId: targetWorkspaceId,
				savedSearchName: savedSearchName).ConfigureAwait(false);

			IEnumerable<FieldMap> generatedFields = await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields: null).ConfigureAwait(false);
			Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(generatedFields).ToArray());
			if (testCase.MapExtractedText)
			{
				IEnumerable<FieldMap> extractedTextMapping = await GetExtractedTextMappingAsync().ConfigureAwait(false);
				Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(extractedTextMapping).ToArray());
			}
			Logger.LogInformation("Fields mapping ready");
		}

		[TestCaseSource(nameof(TestCases))]
		public async Task Run(PerformanceTestCase testCase)
		{
			// Arrange
			
			//
			_sourceWorkspaceIdArm = 1024345;
			int targetWorkspaceId = 1024388;
			//
			
			// Sync 30% Of All Documents
			await SetupAsync(testCase, targetWorkspaceId, "30% Of All Documents").ConfigureAwait(false);
			await RunJobAsync().ConfigureAwait(false);

			// Sync All Documents using AppendOnly to create item level errors
			await SetupAsync(testCase, TargetWorkspace.ArtifactID, "All Documents").ConfigureAwait(false);
			await RunJobAsync().ConfigureAwait(false);

			// Retry 
			Configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspaceIdArm).ConfigureAwait(false);
			Configuration.JobHistoryToRetryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspaceIdArm).ConfigureAwait(false);
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IRetryDataSourceSnapshotConfiguration>(Configuration);
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
		}

		private async Task RunJobAsync()
		{
			ConfigurationRdoId = await Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration).ConfigureAwait(false);
			Logger.LogInformation("Configuration RDO created");

			SyncJobParameters jobParameters = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId);
			SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

			Logger.LogInformation("Starting the job");
			SyncJobState syncJobState = await syncRunner.RunAsync(jobParameters, User.ArtifactID).ConfigureAwait(false);
		}

	}
}