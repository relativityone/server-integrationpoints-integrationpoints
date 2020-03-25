using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.Tests
{
	[TestFixture]
	public class RunnerTests : PerformanceTestBase
	{
		public int ConfigurationRdoId { get; set; }


		[SetUp]
		public async Task SetUp()
		{
			ARMHelper.EnableAgents();
			string filePath = await StorageHelper
				.DownloadFileAsync("SmallSaltPepperWorkspace.zip", Path.GetTempPath()).ConfigureAwait(false);

			int sourceWorkspaceId = await ARMHelper.RestoreWorkspaceAsync(filePath).ConfigureAwait(false);


			Configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			Configuration.ImportNativeFileCopyMode = ImportNativeFileCopyMode.SetFileLinks;

			await SetupConfigurationAsync().ConfigureAwait(false);

			ConfigurationRdoId = await
				Rdos.CreateSyncConfigurationRDO(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);
		}


		[Test]
		public async Task Runner_GoldFlow()
		{
			// Arrange
			const int adminUserId = 9;

			SyncRunner agent = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
				new NullAPM(), new ConsoleLogger());

			SyncJobParameters args = new SyncJobParameters(ConfigurationRdoId, SourceWorkspace.ArtifactID,
				Configuration.JobHistoryId);

			// Act
			SyncJobState jobState = await agent.RunAsync(args, adminUserId).ConfigureAwait(false);
			RelativityObject jobHistory = await Rdos
				.GetJobHistory(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryId)
				.ConfigureAwait(false);

			// Assert
			Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);

			jobHistory["ItemsTransferred"].Value.Should().Be(jobHistory["TotalItems"].Value);
		}
	}
}