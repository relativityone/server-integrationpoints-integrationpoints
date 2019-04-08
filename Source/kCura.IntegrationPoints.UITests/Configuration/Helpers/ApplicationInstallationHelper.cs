using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Logging;
using NUnit.Framework;
using Serilog;
using System;
using System.Diagnostics;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class ApplicationInstallationHelper
	{
		private const string _RIP_GUID_STRING = Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING;

		private const string _LEGAL_HOLD_GUID_STRING = "98F31698-90A0-4EAD-87E3-DAC723FED2A6";

		private readonly TestContext _testContext;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(ApplicationInstallationHelper));

		public ApplicationInstallationHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public void InstallIntegrationPoints()
		{
			InstallApplication(_RIP_GUID_STRING, "Integration Points");
		}

		public void InstallLegalHold()
		{
			InstallApplication(_LEGAL_HOLD_GUID_STRING, "Legal Hold");
		}

		private void InstallApplication(string guid, string appName)
		{
			int? workspaceID = _testContext.WorkspaceId;
			string workspaceName = _testContext.WorkspaceName;

			Assert.NotNull(workspaceID, $"{nameof(workspaceID)} is null. Was workspace created correctly?.");

			Log.Information("Checking application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", appName, guid, workspaceName, workspaceID);
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				var ipAppManager = new RelativityApplicationManager(_testContext.Helper);
				bool isAppInstalledAndUpToDate = ipAppManager.IsApplicationInstalledAndUpToDate(workspaceID.Value, guid);
				if (!isAppInstalledAndUpToDate)
				{
					Log.Information("Installing application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", appName, guid, workspaceName, workspaceID);
					ipAppManager.InstallApplicationFromLibrary(workspaceID.Value, guid);
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
