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
using File = System.IO.File;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class RelativityApplicationManager
	{
		private const int _APP_INSTALATION_TIMEOUT_IN_MINUTES = 15;
		private const int _ADMIN_CASE_ID = -1;
		private InstallStatusCode[] NotCompletedInstallStatuses => new[]
		{
			InstallStatusCode.Pending,
			InstallStatusCode.InProgress
		};

		private readonly ITestHelper _helper;

		public RelativityApplicationManager(ITestHelper helper)
		{
			_helper = helper;
		}

		public async Task ImportRipToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.UseLocalRap
				? GetLocalRipRapPath()
				: GetBuildPackagesRipRapPath();
			await ImportApplicationToLibraryAsync(applicationFilePath).ConfigureAwait(false);
		}

		public async Task ImportApplicationToLibraryAsync(string appPath)
		{
			using (var libraryApplicationManager = _helper.CreateProxy<ILibraryApplicationManager>())
			{
				await UpdateAppInLibraryAndWaitForInstallationAsync(libraryApplicationManager, appPath).ConfigureAwait(false);
			}
		}

		public Task InstallRipFromLibraryAsync(int workspaceArtifactId)
		{
			Guid ripGuid = Guid.Parse(IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING);
			return InstallApplicationFromLibraryAsync(workspaceArtifactId, ripGuid);
		}

		public async Task InstallApplicationFromLibraryAsync(int workspaceArtifactId, Guid appGuid)
		{
			await ThrowTestSetupExceptionWhenAppIsNotInLibraryAsync(workspaceArtifactId, appGuid).ConfigureAwait(false);
			using (var applicationInstallManager = _helper.CreateProxy<IApplicationInstallManager>())
			{
				var installApplicationRequest = new InstallApplicationRequest
				{
					ConflictResolutions = new List<ApplicationInstallConflictResolution>(),
					UnlockApplications = false,
					WorkspaceIDs = new List<int> { workspaceArtifactId }
				};
				InstallApplicationResponse installResponse = await applicationInstallManager
					.InstallApplicationAsync(_ADMIN_CASE_ID, appGuid, installApplicationRequest)
					.ConfigureAwait(false);

				InstallApplicationResult installInWorkspaceResult = installResponse.Results.Single();
				int applicationInstallID = installInWorkspaceResult.ApplicationInstallID;

				Func<Task<GetInstallStatusResponse>> getCurrentInstallStatusFunction =
					() => applicationInstallManager.GetStatusAsync(_ADMIN_CASE_ID, appGuid, applicationInstallID);
				await WaitForInstallToCompleteWithTimeoutAsync(getCurrentInstallStatusFunction).ConfigureAwait(false);
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
					installStatusResponse = await applicationInstallManager
						.GetAllInstallStatusAsync(_ADMIN_CASE_ID, guid, currentIndex, batchSize)
						.ConfigureAwait(false);

					foreach (GetInstallStatusResponse getInstallStatusResponse in installStatusResponse.Results)
					{
						if (getInstallStatusResponse.WorkspaceIdentifier.ArtifactID == workspaceArtifactID)
						{
							return IsInstallCompleted(getInstallStatusResponse.InstallStatus);
						}
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
					int appID = await SendUpdateAppRequest(libraryApplicationManager, keplerStream)
						.ConfigureAwait(false);

					Func<Task<GetInstallStatusResponse>> getCurrentInstallStatusFunction =
						() => libraryApplicationManager.GetLibraryInstallStatusAsync(_ADMIN_CASE_ID, appID);

					await WaitForInstallToCompleteWithTimeoutAsync(getCurrentInstallStatusFunction)
						.ConfigureAwait(false);
				}
			}
		}

		private static async Task<int> SendUpdateAppRequest(ILibraryApplicationManager libraryApplicationManager, KeplerStream rapStream)
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

		private async Task WaitForInstallToCompleteWithTimeoutAsync(Func<Task<GetInstallStatusResponse>> getCurrentStatus)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(_APP_INSTALATION_TIMEOUT_IN_MINUTES));

			try
			{
				await WaitForInstallToCompleteAsync(getCurrentStatus, cancellationTokenSource.Token)
					.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				string errorMessage = $"Application install did not complete within the timeout period ({_APP_INSTALATION_TIMEOUT_IN_MINUTES} minutes).";
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
			while (NotCompletedInstallStatuses.Contains(installStatus.Code));

			if (!IsInstallCompleted(installStatus))
			{
				string errorMessage = $"Error occured while installing application. Install status: {installStatus.Code}, message: {installStatus.Message}";
				throw new TestSetupException(errorMessage);
			}
		}

		private async Task ThrowTestSetupExceptionWhenAppIsNotInLibraryAsync(int workspaceArtifactId, Guid appGuid)
		{
			bool isAppInstalledInLibrary = await IsAppInstalledInLibraryAsync(appGuid).ConfigureAwait(false);
			if (!isAppInstalledInLibrary)
			{
				string errorMessage = $"Cannot install app {appGuid} in workspace {workspaceArtifactId}, because it is not installed in library";
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

				return IsInstallCompleted(installStatusResponse.InstallStatus);
			}
		}

		private bool IsInstallCompleted(InstallStatus installStatus)
			=> installStatus.Code == InstallStatusCode.Completed;
	}
}
