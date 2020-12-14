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
		private static Guid O365Guid => Guid.Parse("4be29f9a-0d53-4e79-89c4-e83718d59354");
		private static Guid JsonLoaderGuid => Guid.Parse("57151c17-cd92-4a6e-800c-a75bf807d097");
		private static Guid MyFirstProviderGuid => Guid.Parse("616c3c78-aa2c-46b9-b81c-21be354f323d");

		public ApplicationInstallationHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public Task InstallLegalHoldAsync()
		{
			return InstallApplicationAsync(LegalHoldGuid, "Relativity Legal Hold");
		}

		public Task InstallO365Async()
		{
			return InstallApplicationAsync(O365Guid, "Office 365 Integration");
		}

		public Task InstallJsonLoaderAsync()
		{
			return InstallApplicationAsync(JsonLoaderGuid, "JsonLoader");
		}

		public Task InstallMyFirstProviderAsync()
		{
			return InstallApplicationAsync(MyFirstProviderGuid, "MyFirstProvider");
		}
		public Task<bool> IsIntegrationPointsInstalledAsync()
		{
			int? workspaceID = _testContext.WorkspaceId;

			return new RelativityApplicationManager(_testContext.Helper)
				.IsApplicationInstalledAndUpToDateAsync(workspaceID.Value, RipGuid);
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
				Log.Information("Installing application '{AppName}' ({AppGUID}) in workspace '{WorkspaceName}' ({WorkspaceId}).", appName, guid, workspaceName, workspaceID);

				var ipAppManager = new RelativityApplicationManager(_testContext.Helper);		
				await ipAppManager.InstallApplicationFromLibraryAsync(workspaceID.Value, guid).ConfigureAwait(false);
				
				Log.Information("Application '{AppName}' ({AppGUID}) has been installed in workspace '{WorkspaceName}' ({WorkspaceId}) after {AppInstallTime} seconds.",
					appName, guid, workspaceName, workspaceID, stopwatch.Elapsed.Seconds);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Detecting or installing application '{AppName}' ({AppGUID}) in the workspace '{WorkspaceName}' ({WorkspaceId}) failed.", appName, guid, workspaceName, workspaceID);
				throw;
			}
		}
	}
}
