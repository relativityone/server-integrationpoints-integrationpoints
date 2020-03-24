using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Tests.Performance.Tests;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Performance.RunnerSetupTests
{
	[TestFixture]
	public class RunnerTests : PerformanceTestBase
	{
		[Test]
		public async Task Runner_GoldFlow()
		{
			SyncRunner agent = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
				new NullAPM(), new ConsoleLogger());


			await SetupConfigurationAsync().ConfigureAwait(false);

			int configurationRdoId = await
				Rdos.CreateSyncConfigurationRDO(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);

			SyncJobParameters args = new SyncJobParameters(configurationRdoId, SourceWorkspace.ArtifactID,
				Configuration.JobHistoryId);

			const int adminUserId = 9;

			SyncJobState jobState = await agent.RunAsync(args, adminUserId).ConfigureAwait(false);

			Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);
		}
	}
}