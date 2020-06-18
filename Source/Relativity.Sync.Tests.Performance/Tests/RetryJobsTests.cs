using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
					TestCaseName = "Retry-Links",
					CopyMode = ImportNativeFileCopyMode.SetFileLinks,
					NumberOfMappedFields = 100,
				},
				//new PerformanceTestCase
				//{
				//	TestCaseName = "Retry-CopyNatives",
				//	CopyMode = ImportNativeFileCopyMode.CopyFiles,
				//	NumberOfMappedFields = 100,
				//},
			};

			return testCases.Select(x => new TestCaseData(x)
			{
				TestName = $"{x.TestCaseName} ({x.ExpectedItemsTransferred} docs, {x.NumberOfMappedFields} fields)"
			});

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[TestCaseSource(nameof(TestCases))]
		public async Task Run(PerformanceTestCase testCase)
		{
			// Arrange
			Configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOnly;
			Configuration.ImportNativeFileCopyMode = testCase.CopyMode;

			await SetupConfigurationAsync(
				sourceWorkspaceId: _sourceWorkspaceIdArm, 
				targetWorkspaceId: null,
				savedSearchName: "30% Of All Documents").ConfigureAwait(false);

			int targetWorkspaceId = TargetWorkspace.ArtifactID;

			ConfigurationRdoId = await
				Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);

			IEnumerable<FieldMap> generatedFields =
				await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields: null)
					.ConfigureAwait(false);

			Configuration.FieldsMapping = Configuration.FieldsMapping.Concat(generatedFields).ToArray();

			if (testCase.MapExtractedText)
			{
				IEnumerable<FieldMap> extractedTextMapping =
					await GetGetExtractedTextMappingAsync().ConfigureAwait(false);
				Configuration.FieldsMapping = Configuration.FieldsMapping.Concat(extractedTextMapping).ToArray();
			}

			Logger.LogInformation("Fields mapping ready");

			ConfigurationRdoId = await
				Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);

			Logger.LogInformation("Configuration RDO created");

			SyncJobParameters args = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID,
				Configuration.JobHistoryId);

			SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
				new NullAPM(), TestLogHelper.GetLogger());

			Logger.LogInformation("Staring the job");

			// Act
			SyncJobState jobState = await syncRunner.RunAsync(args, User.ArtifactID).ConfigureAwait(false);
		}
	}
}