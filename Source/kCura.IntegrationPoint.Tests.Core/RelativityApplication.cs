using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using kCura.Relativity.Client;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Services.ApplicationInstallManager;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class RelativityApplication
	{
		public static void ImportOrUpgradeRelativityApplication(int workspaceArtifactId, Guid applicationGuid, ClaimsPrincipal claimsPrincipal, string applicationRapFileName = null)
		{
			if (SharedVariables.UseLocalRap)
			{
				ImportRapFromLocalLocation(workspaceArtifactId);
			}
			else
			{
				ImportRapFromBuildPackages(workspaceArtifactId, applicationGuid, claimsPrincipal, applicationRapFileName);
			}
		}

		private static void ImportApplicationToWorkspace(int workspaceId, string applicationFilePath, bool forceUnlock,
			List<int> appsToOverride = null)
		{
			//List of application ArtifactIDs to override, if already installed
			// TODO: Add this functionality - Gerron Thurman 5/11/2016
			List<int> applicationsToOverride = appsToOverride ?? new List<int>();

			AppInstallRequest appInstallRequest = new AppInstallRequest()
			{
				FullFilePath = applicationFilePath,
				ForceFlag = true
			};

			using (IRSAPIClient proxy = Rsapi.CreateRsapiClient())
			{
				try
				{
					proxy.APIOptions.WorkspaceID = workspaceId;
					ProcessOperationResult result = proxy.InstallApplication(proxy.APIOptions, appInstallRequest);
					if (!result.Success)
					{
						throw new Exception($"Failed to install application file: {applicationFilePath} to workspace: {workspaceId}.");
					}

					Status.WaitForProcessToComplete(proxy, result.ProcessID, (int)TimeSpan.FromMinutes(2).TotalSeconds, 500);
				}
				catch (Exception ex)
				{
					throw new Exception($"An error occurred attempting to import the application file {applicationFilePath}. Error: {ex.Message}.");
				}
			}
		}

		private static void ImportLibraryApplicationToWorkspace(int workspaceArtifactId, Guid applicationGuid)
		{
			int applicationInstallId = 0;
			using (IApplicationInstallManager proxy = Kepler.CreateProxy<IApplicationInstallManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				applicationInstallId = proxy.InstallLibraryApplicationByGuid(workspaceArtifactId, applicationGuid).ConfigureAwait(false).GetAwaiter().GetResult();
			}

			if (applicationInstallId == 0)
			{
				throw new Exception($"Failed to install Library Application. SourceWorkspace: {workspaceArtifactId}. ApplicationGuid {applicationGuid}");
			}
		}

		private static void ImportRapFromLocalLocation(int workspaceArtifactId)
		{
			string applicationFilePath = SharedVariables.RapFileLocation;

			ImportApplicationToWorkspace(workspaceArtifactId, applicationFilePath, true);
		}

		private static void ImportRapFromBuildPackages(int workspaceArtifactId, Guid applicationGuid, ClaimsPrincipal claimsPrincipal, string applicationRapFileName = null)
		{
			string libraryApplicationVersion = ReadRelativityLibraryApplicationVersion(claimsPrincipal, applicationGuid);
			string latestRapVersion = SharedVariables.LatestRapVersionFromBuildPackages;

			int intLibraryApplicationVersion = Convert.ToInt32(String.Join("", libraryApplicationVersion.Split('.')));
			int intLatestRapVersion = Convert.ToInt32(String.Join("", latestRapVersion.Split('.')));

			if (intLatestRapVersion > intLibraryApplicationVersion)
			{
				string applicationFilePath = applicationRapFileName == null
					? Path.Combine(SharedVariables.LatestRapLocationFromBuildPackages, SharedVariables.ApplicationPath,
						SharedVariables.ApplicationRapFileName)
					: Path.Combine(SharedVariables.LatestRapLocationFromBuildPackages, SharedVariables.ApplicationPath,
						applicationRapFileName);

				ImportApplicationToWorkspace(workspaceArtifactId, applicationFilePath, true);
			}
			else
			{
				ImportLibraryApplicationToWorkspace(workspaceArtifactId, applicationGuid);
			}
		}

		private static string ReadRelativityLibraryApplicationVersion(ClaimsPrincipal claimsPrincipal, Guid applicationGuid)
		{
			var libraryManager = new global::Relativity.Core.Service.LibraryApplicationManager();
			ICoreContext baseServiceContext = GetBaseServiceContext(claimsPrincipal, -1);
			global::Relativity.Core.DTO.LibraryApplication libraryApplicationDto = libraryManager.Read(baseServiceContext, applicationGuid);

			return libraryApplicationDto.Version;
		}

		private static ICoreContext GetBaseServiceContext(ClaimsPrincipal claimsPrincipal, int workspaceId)
		{
			try
			{
				return claimsPrincipal.GetServiceContextUnversionShortTerm(workspaceId);
			}
			catch (Exception exception)
			{
				throw new Exception("Unable to initialize the user context.", exception);
			}
		}
	}
}
