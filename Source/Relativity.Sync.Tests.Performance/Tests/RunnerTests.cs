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
			try
			{
				SyncJobState jobState = null;

				SyncRunner agent = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl,
					new NullAPM(), new ConsoleLogger());


				await SetupConfigurationAsync().ConfigureAwait(false);

				int configurationRdoId = await
					Rdos.CreateSyncConfigurationRDO(ServiceFactory, SourceWorkspace.ArtifactID, Configuration).ConfigureAwait(false);

				SyncJobParameters args = new SyncJobParameters(configurationRdoId, SourceWorkspace.ArtifactID, Configuration.JobHistoryId);

				const int adminUserId = 9;
				var progress = new Progress<SyncJobState>();
				progress.ProgressChanged += (sender, state) =>
				{
					jobState = state;
				};

				await agent.RunAsync(args, progress, adminUserId, CancellationToken.None).ConfigureAwait(false);

				Assert.True(jobState.Status == SyncJobStatus.Completed, message: jobState.Message);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
#pragma warning restore CA1031 // Do not catch general exception types

		}
	}
}