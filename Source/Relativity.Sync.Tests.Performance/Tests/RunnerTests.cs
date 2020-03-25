using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
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
		[SetUp]
		public void SetUp()
		{
			ARMHelper.EnableAgents();
		}

		[Test]
		public async Task Runner_GoldFlow()
		{
			try
			{
				// Arrange
				string filePath = await StorageHelper
					.DownloadFileAsync("SmallSaltPepperWorkspace.zip", Path.GetTempPath()).ConfigureAwait(false);

				int workspaceID = await ARMHelper.RestoreWorkspaceAsync(filePath).ConfigureAwait(false);


				SyncRunner agent = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
					new NullAPM(), new ConsoleLogger());


				await SetupConfigurationAsync(sourceWorkspaceId: workspaceID).ConfigureAwait(false);

				int configurationRdoId = await
					Rdos.CreateSyncConfigurationRDO(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
						.ConfigureAwait(false);

				SyncJobParameters args = new SyncJobParameters(configurationRdoId, SourceWorkspace.ArtifactID,
					Configuration.JobHistoryId);

				const int adminUserId = 9;

				// Act
				SyncJobState jobState = await agent.RunAsync(args, adminUserId).ConfigureAwait(false);

				Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);

			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
#pragma warning restore CA1031 // Do not catch general exception types

			// Assert
		}
	}
}