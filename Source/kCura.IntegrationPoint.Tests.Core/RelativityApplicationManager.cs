using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Kepler.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using Polly;
using Polly.Retry;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Services.Interfaces.LibraryApplication.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class RelativityApplicationManager
	{
		private const int _MAX_NUMBER_OF_APP_INSTALL_RETRIES = 3;
		private const int _DELAY_BETWEEN_RETRIES_IN_MINUTES = 1;
		private const int _APP_INSTALLATION_TIMEOUT_IN_MINUTES = 15;
		private const int _ADMIN_CASE_ID = -1;

		private readonly ITestHelper _helper;

		public RelativityApplicationManager(ITestHelper helper)
		{
			_helper = helper;
		}

		public Task ImportRipToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.UseLocalRap
				? GetLocalRipRapPath()
				: GetBuildPackagesRipRapPath();
			return ImportApplicationToLibraryAsync(applicationFilePath);
		}

		public Task ImportApplicationToLibraryAsync(string appPath)
		{
			return UpdateAppInLibraryAndWaitForInstallationAsync(appPath);
		}

		public Task InstallRipFromLibraryAsync(int workspaceArtifactID)
		{
			Guid ripGuid = Guid.Parse(IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING);
			return InstallApplicationFromLibraryAsync(workspaceArtifactID, ripGuid);
		}

		public async Task InstallApplicationFromLibraryAsync(int workspaceArtifactID, Guid appGuid)
		{
			await ThrowWhenAppIsNotInLibraryAsync(workspaceArtifactID, appGuid).ConfigureAwait(false);
			using (var applicationInstallManager = _helper.CreateProxy<IApplicationInstallManager>())
			{
				var installApplicationRequest = new InstallApplicationRequest
				{
					ConflictResolutions = new List<ApplicationInstallConflictResolution>(),
					UnlockApplications = false,
					WorkspaceIDs = new List<int> { workspaceArtifactID }
				};
				InstallApplicationResponse installResponse = await applicationInstallManager
					.InstallApplicationAsync(_ADMIN_CASE_ID, appGuid, installApplicationRequest)
					.ConfigureAwait(false);

				InstallApplicationResult installInWorkspaceResult = installResponse.Results.Single();
				int applicationInstallID = installInWorkspaceResult.ApplicationInstallID;

				Func<Task<GetInstallStatusResponse>> currentInstallStatusGetter =
					() => applicationInstallManager.GetStatusAsync(_ADMIN_CASE_ID, appGuid, applicationInstallID);
				await WaitForInstallCompletionWithinTimeoutAsync(currentInstallStatusGetter).ConfigureAwait(false);
			}
		}
		
		private string GetLocalRipRapPath()
		{
			return SharedVariables.RipRapFilePath;
		}

		private string GetBuildPackagesRipRapPath()
		{
			return Path.Combine(
				SharedVariables.LatestRapLocationFromBuildPackages,
				SharedVariables.ApplicationPath,
				SharedVariables.RipRapFilePath);
		}

		private async Task UpdateAppInLibraryAndWaitForInstallationAsync(string rapPath)
		{
			Console.WriteLine($"Import Application from {rapPath}");
			int appID = await SendUpdateAppRequestAsync(rapPath).ConfigureAwait(false);

			Console.WriteLine($"Application ID has been retrieved: {appID}");

			using (var libraryApplicationManager = _helper.CreateProxy<ILibraryApplicationManager>())
			{
				Func<Task<GetInstallStatusResponse>> currentInstallStatusGetter = () => libraryApplicationManager.GetLibraryInstallStatusAsync(_ADMIN_CASE_ID, appID);
				await WaitForInstallCompletionWithinTimeoutAsync(currentInstallStatusGetter).ConfigureAwait(false);
			}
		}

		private Task<int> SendUpdateAppRequestAsync(string appPath)
		{
			var request = new UpdateLibraryApplicationRequest
			{
				CreateIfMissing = true,
				IgnoreVersion = true,
				RefreshCustomPages = true
			};

			RetryPolicy retryPolicy = Policy
				.Handle<Exception>()
				.WaitAndRetryAsync(_MAX_NUMBER_OF_APP_INSTALL_RETRIES, (retryCount, context) => TimeSpan.FromMinutes(_DELAY_BETWEEN_RETRIES_IN_MINUTES),
					(exception, timeout, retryCount, context) =>
					{
						Console.WriteLine($"Failed to send application install request. Delay {_DELAY_BETWEEN_RETRIES_IN_MINUTES} min and retry. " +
										  $"Retry count: {retryCount}{Environment.NewLine}" +
										  $"Exception:{Environment.NewLine}{exception}");
					});

			return retryPolicy.ExecuteAsync(async () =>
			{
				using (FileStream fileStream = File.OpenRead(appPath))
				using (var keplerStream = new KeplerStream(fileStream))
				using (var libraryApplicationManager = _helper.CreateProxy<ILibraryApplicationManager>())
				{
					UpdateLibraryApplicationResponse updateAppResponse = await libraryApplicationManager
						.UpdateAsync(_ADMIN_CASE_ID, keplerStream, request)
						.ConfigureAwait(false);
					int appID = updateAppResponse.ApplicationIdentifier.ArtifactID;
					return appID;
				}
			});
		}

		private async Task WaitForInstallCompletionWithinTimeoutAsync(Func<Task<GetInstallStatusResponse>> getCurrentStatus)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(_APP_INSTALLATION_TIMEOUT_IN_MINUTES));
			Console.WriteLine("Wait for installation to complete...");
			try
			{
				await WaitForInstallToCompleteAsync(getCurrentStatus, cancellationTokenSource.Token)
					.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				string errorMessage = $"Application install did not complete within the timeout period ({_APP_INSTALLATION_TIMEOUT_IN_MINUTES} minutes).";
				throw new TestSetupException(errorMessage);
			}
		}

		private async Task WaitForInstallToCompleteAsync(
			Func<Task<GetInstallStatusResponse>> getCurrentStatus,
			CancellationToken cancellationToken)
		{
			const int sleepMilliseconds = 1000;

			InstallStatus installStatus;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();

				GetInstallStatusResponse installStatusResponse = await getCurrentStatus().ConfigureAwait(false);
				installStatus = installStatusResponse.InstallStatus;
				Console.WriteLine($"Installing... {installStatus.Code}");
				Thread.Sleep(sleepMilliseconds);
			}
			while (IsInstallIncomplete(installStatus));

			if (!IsInstallSuccessfullyCompleted(installStatus))
			{
				string errorMessage = $"Error occured while installing application. Install status: {installStatus.Code}, message: {installStatus.Message}";
				throw new TestSetupException(errorMessage);
			}
		}

		private async Task ThrowWhenAppIsNotInLibraryAsync(int workspaceArtifactID, Guid appGuid)
		{
			bool isAppInstalledInLibrary = await IsAppInstalledInLibraryAsync(appGuid).ConfigureAwait(false);
			if (!isAppInstalledInLibrary)
			{
				string errorMessage = $"Cannot install app {appGuid} in workspace {workspaceArtifactID}, because it is not installed in library";
				throw new TestSetupException(errorMessage);
			}
		}

		private async Task<bool> IsAppInstalledInLibraryAsync(Guid appGuid)
		{
			using (var libraryApplicationManager = _helper.CreateProxy<ILibraryApplicationManager>())
			{
				try
				{
					await libraryApplicationManager
						.ReadAsync(_ADMIN_CASE_ID, appGuid)
						.ConfigureAwait(false);
				}
				catch (InvalidInputException)
				{
					// app does not exist, or we do not have an access to it
					return false;
				}

				GetInstallStatusResponse installStatusResponse = await libraryApplicationManager
					.GetLibraryInstallStatusAsync(_ADMIN_CASE_ID, appGuid)
					.ConfigureAwait(false);

				return IsInstallSuccessfullyCompleted(installStatusResponse.InstallStatus);
			}
		}

		private bool IsInstallSuccessfullyCompleted(InstallStatus installStatus)
			=> installStatus.Code == InstallStatusCode.Completed;

		private bool IsInstallIncomplete(InstallStatus installStatus)
		{
			InstallStatusCode[] notCompletedInstallStatuses =
			{
				InstallStatusCode.Pending,
				InstallStatusCode.InProgress
			};

			return notCompletedInstallStatuses.Contains(installStatus.Code);
		}
	}
}
