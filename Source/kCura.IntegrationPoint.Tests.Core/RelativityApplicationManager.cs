using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Kepler.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Exceptions;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.LibraryApplication;
using Relativity.Services.Interfaces.LibraryApplication.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class RelativityApplicationManager
	{
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

		public async Task ImportApplicationToLibraryAsync(string appPath)
		{
			using (var libraryApplicationManager = _helper.CreateProxy<ILibraryApplicationManager>())
			{
				await UpdateAppInLibraryAndWaitForInstallationAsync(libraryApplicationManager, appPath).ConfigureAwait(false);
			}
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

		public async Task<bool> IsApplicationInstalledAndUpToDateAsync(int workspaceArtifactID, Guid guid)
		{
			const int batchSize = 100;
			int currentIndex = 1;
			using (var applicationInstallManager = _helper.CreateProxy<IApplicationInstallManager>())
			{
				GetAllInstallStatusResponse installStatusResponse;
				do
				{
#pragma warning disable CS0618 // Type or member is obsolete
					installStatusResponse = await applicationInstallManager
						.GetAllInstallStatusAsync(_ADMIN_CASE_ID, guid, currentIndex, batchSize)
#pragma warning restore CS0618 // Type or member is obsolete
						.ConfigureAwait(false);

					GetInstallStatusResponse installStatusResponseForWorkspace = installStatusResponse
						.Results
						.FirstOrDefault(x => x.WorkspaceIdentifier.ArtifactID == workspaceArtifactID);

					if (installStatusResponseForWorkspace != null)
					{
						return IsInstallSuccessfullyCompleted(installStatusResponseForWorkspace.InstallStatus);
					}

					currentIndex += batchSize;
				}
				while (installStatusResponse.ResultCount == batchSize);
			}
			return false;
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

		private async Task UpdateAppInLibraryAndWaitForInstallationAsync(
			ILibraryApplicationManager libraryApplicationManager,
			string appPath)
		{
			using (FileStream fileStream = File.OpenRead(appPath))
			{
				using (var keplerStream = new KeplerStream(fileStream))
				{
					int appID = await SendUpdateAppRequestAsync(libraryApplicationManager, keplerStream)
						.ConfigureAwait(false);

					Func<Task<GetInstallStatusResponse>> currentInstallStatusGetter =
						() => libraryApplicationManager.GetLibraryInstallStatusAsync(_ADMIN_CASE_ID, appID);

					await WaitForInstallCompletionWithinTimeoutAsync(currentInstallStatusGetter)
						.ConfigureAwait(false);
				}
			}
		}

		private static async Task<int> SendUpdateAppRequestAsync(ILibraryApplicationManager libraryApplicationManager, KeplerStream rapStream)
		{
			var request = new UpdateLibraryApplicationRequest
			{
				CreateIfMissing = true,
				RefreshCustomPages = true
			};

			UpdateLibraryApplicationResponse updateAppResponse = await libraryApplicationManager
				.UpdateAsync(_ADMIN_CASE_ID, rapStream, request)
				.ConfigureAwait(false);
			int appID = updateAppResponse.ApplicationIdentifier.ArtifactID;
			return appID;
		}

		private async Task WaitForInstallCompletionWithinTimeoutAsync(Func<Task<GetInstallStatusResponse>> getCurrentStatus)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(_APP_INSTALLATION_TIMEOUT_IN_MINUTES));

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
			InstallStatus installStatus;
			do
			{
				cancellationToken.ThrowIfCancellationRequested();

				GetInstallStatusResponse installStatusResponse = await getCurrentStatus().ConfigureAwait(false);
				installStatus = installStatusResponse.InstallStatus;
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
