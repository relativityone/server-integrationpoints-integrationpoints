using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	public class SmallJobsTests : PerformanceTestBase
	{
		[SetUp]
		public void SetUp()
		{
			ARMHelper.EnableAgents();

			Configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			Configuration.ImportNativeFileCopyMode = ImportNativeFileCopyMode.SetFileLinks;
		}

		[TestCase("SmallSaltPepperWorkspace.zip", true, 0, 5)]
		public async Task RunTestCase(string armedWorkspaceName, bool mapExtractedText,
			int numberOfMappedFields, int expectedTransferredItems)
		{
			// Arrange
			const int adminUserId = 9;

			string filePath = await StorageHelper
				.DownloadFileAsync(armedWorkspaceName, Path.GetTempPath()).ConfigureAwait(false);

			int sourceWorkspaceId = await ARMHelper.RestoreWorkspaceAsync(filePath).ConfigureAwait(false);

			await SetupConfigurationAsync(sourceWorkspaceId).ConfigureAwait(false);

			ConfigurationRdoId = await
				Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);

			IEnumerable<FieldMap> generatedFields =
				await GetMappingAndCreateFieldsInDestinationWorkspaceAsync(numberOfMappedFields)
					.ConfigureAwait(false);

			Configuration.FieldsMapping = Configuration.FieldsMapping.Concat(generatedFields).ToArray();

			if (mapExtractedText)
			{
				IEnumerable<FieldMap> extractedTextMapping = await GetGetExtractedTextMapping().ConfigureAwait(false);
				Configuration.FieldsMapping = Configuration.FieldsMapping.Concat(extractedTextMapping).ToArray();
			}

			ConfigurationRdoId = await
				Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);


			SyncJobParameters args = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID,
				Configuration.JobHistoryId);


			SyncRunner syncRunner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
				new NullAPM(), new ConsoleLogger());

			// Act
			SyncJobState jobState = await syncRunner.RunAsync(args, adminUserId).ConfigureAwait(false);
			RelativityObject jobHistory = await Rdos
				.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryId)
				.ConfigureAwait(false);

			// Assert
			Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);

			int totalItems = (int) jobHistory["Total Items"].Value;
			int itemsTranferred = (int) jobHistory["Items Transferred"].Value;

			itemsTranferred.Should().Be(totalItems);
			itemsTranferred.Should().Be(expectedTransferredItems);
		}

		public int ConfigurationRdoId { get; set; }
	}
}
