using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class ApplicationInstallationHelper
	{
		private readonly TestContext _testContext;
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(ApplicationInstallationHelper));

		private static Guid RipGuid => Guid.Parse(Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING);
		private static Guid LegalHoldGuid => Guid.Parse("98F31698-90A0-4EAD-87E3-DAC723FED2A6");

		public ApplicationInstallationHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public Task InstallIntegrationPointsAsync()
		{
			return InstallApplicationAsync(RipGuid, "Integration Points");
		}

		public Task InstallLegalHoldAsync()
		{
			return InstallApplicationAsync(LegalHoldGuid, "Relativity Legal Hold");
		}

		public async Task<bool> IsIntegrationPointsInstalledAsync()
		{
			int? workspaceID = _testContext.WorkspaceId;

			return await new RelativityApplicationManager(_testContext.Helper)
				.IsApplicationInstalledAndUpToDateAsync(workspaceID.Value, RipGuid).ConfigureAwait(false);
		}

		private async Task InstallApplicationAsync(Guid guid, string appName)
		{
			int? workspaceID = _testContext.WorkspaceId;
			string workspaceName = _testContext.WorkspaceName;

			Assert.NotNull(workspaceID, $"{nameof(workspaceID)} is null. Was workspace created correctly?.");

			Log.Information("Checking application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", appName, guid, workspaceName, workspaceID);
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				var ipAppManager = new RelativityApplicationManager(_testContext.Helper);
				bool isAppInstalledAndUpToDate = await ipAppManager.IsApplicationInstalledAndUpToDateAsync(workspaceID.Value, guid).ConfigureAwait(false);
				if (!isAppInstalledAndUpToDate)
				{
					Log.Information("Installing application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", appName, guid, workspaceName, workspaceID);
					await ipAppManager.InstallApplicationFromLibraryAsync(workspaceID.Value, guid).ConfigureAwait(false);
					Log.Information("Application '{AppName}' ({AppGUID}) has been installed in workspace '{WorkspaceName}' ({WorkspaceId}) after {AppInstallTime} seconds.",
						appName, guid, workspaceName, workspaceID, stopwatch.Elapsed.Seconds);
				}
				else
				{
					Log.Information("Application '{AppName}' ({AppGUID}) is already installed in workspace '{WorkspaceName}' ({WorkspaceId}).", appName, guid, workspaceName, workspaceID);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Detecting or installing application '{AppName}' ({AppGUID}) in the workspace '{WorkspaceName}' ({WorkspaceId}) failed.", appName, guid, workspaceName, workspaceID);
				throw;
			}
		}
	}
}
