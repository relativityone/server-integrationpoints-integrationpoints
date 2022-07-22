using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
    [TestFixture]
    [Category("RETRY_Jobs")]
    internal class RetryJobsTests : PerformanceTestBase
    {
        private readonly string _sourceWorkspaceArmFile;
        private readonly int _expectedTotalItems;
        private readonly int _expectedItemsWithErrors;

        public RetryJobsTests(string sourceWorkspaceArmFile, int expectedTotalItems,int expectedItemsWithErrors )
        {
            _sourceWorkspaceArmFile = sourceWorkspaceArmFile;
            _expectedTotalItems = expectedTotalItems;
            _expectedItemsWithErrors = expectedItemsWithErrors;
        }

        public RetryJobsTests()
        {
            _sourceWorkspaceArmFile = "Sync Retries 100k_Docs-30k_Errors.zip";
            _expectedTotalItems = 100000;
            _expectedItemsWithErrors = 30000;
        }

        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup().ConfigureAwait(false);

            await UseArmWorkspaceAsync(
                    _sourceWorkspaceArmFile,
                    null)
                .ConfigureAwait(false);
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

        private async Task SetupAsync(PerformanceTestCase testCase, string savedSearchName)
        {
            Configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOnly;
            Configuration.ImportNativeFileCopyMode = testCase.CopyMode;

            await SetupConfigurationAsync(
                savedSearchName: savedSearchName).ConfigureAwait(false);

            IEnumerable<FieldMap> generatedFields = await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields: null).ConfigureAwait(false);
            Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(generatedFields).ToArray());
            if (testCase.MapExtractedText)
            {
                IEnumerable<FieldMap> extractedTextMapping = await GetExtractedTextMappingAsync(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID).ConfigureAwait(false);
                Configuration.SetFieldMappings(Configuration.GetFieldMappings().Concat(extractedTextMapping).ToArray());
            }
            Logger.LogInformation("Fields mapping ready");
        }

        [TestCaseSource(nameof(TestCases))]
        public virtual async Task Run(PerformanceTestCase testCase)
        {
            // Arrange

            // Sync 30% Of All Documents
            await SetupAsync(testCase, "30% Of All Documents").ConfigureAwait(false);
            await RunJobAsync().ConfigureAwait(false);

            // Sync All Documents using AppendOnly to create item level errors
            await SetupAsync(testCase, "All Documents").ConfigureAwait(false);
            await RunJobAsync().ConfigureAwait(false);

            RelativityObject jobHistory = await Rdos.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId, Configuration.JobHistory.TypeGuid).ConfigureAwait(false);

            int totalItems = (int)jobHistory["Total Items"].Value;
            int itemsTranferred = (int)jobHistory["Items Transferred"].Value;
            int itemsWithErrors = (int)jobHistory["Items with Errors"].Value;

            totalItems.Should().Be(_expectedTotalItems);
            itemsTranferred.Should().Be(_expectedTotalItems - _expectedItemsWithErrors);
            itemsWithErrors.Should().Be(_expectedItemsWithErrors);

            // Retry 
            Configuration.JobHistoryToRetryId = Configuration.JobHistoryArtifactId;
            Configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
            ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IRetryDataSourceSnapshotConfiguration>(Configuration);

            Stopwatch stopwatch = Stopwatch.StartNew();
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);
            stopwatch.Stop();
            TimeSpan elapsedTime = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            TestTimes.Add(testCase.TestCaseName, elapsedTime);

            Logger.LogInformation("Elapsed time {0} s", elapsedTime.TotalSeconds.ToString("F", CultureInfo.InvariantCulture));

            // ASSERT
            Configuration.TotalRecordsCount.Should().Be(_expectedItemsWithErrors);
        }

        private async Task RunJobAsync()
        {
            ConfigurationRdoId = await Rdos.CreateSyncConfigurationRdoAsync(SourceWorkspace.ArtifactID, Configuration).ConfigureAwait(false);
            Logger.LogInformation("Configuration RDO created");

            SyncJobParameters jobParameters = FakeHelper.CreateSyncJobParameters();
            SyncRunner syncRunner = new SyncRunner(AppSettings.RelativityUrl, new NullAPM(), Logger);

            Logger.LogInformation("Starting the job");
            await syncRunner.RunAsync(jobParameters, User.ArtifactID).ConfigureAwait(false);
        }

    }
}