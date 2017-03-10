using System;
using System.IO;
using System.Security.Claims;
using kCura.Relativity.Client;
using Relativity.Core;
using Relativity.Core.Authentication;
using Relativity.Core.DTO;
using Relativity.Core.Service;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class RelativityApplicationManager
	{
		private readonly LibraryApplicationManager _libraryManager;
		private readonly ICoreContext _baseServiceContext;

		public RelativityApplicationManager(ClaimsPrincipal claimsPrincipal)
		{
			_libraryManager = new LibraryApplicationManager();
			_baseServiceContext = GetBaseServiceContext(claimsPrincipal, -1);
		}

		public LibraryApplication GetLibraryApplicationDTO(Guid applicationGuid)
		{
			LibraryApplication libraryApplicationDto = _libraryManager.Read(_baseServiceContext, applicationGuid);

			return libraryApplicationDto;
		}

		public void RemoveApplicationFromLibrary(LibraryApplication libraryApplication)
		{
			libraryApplication.IsVisible = false;
			_libraryManager.Update((BaseServiceContext)_baseServiceContext, libraryApplication);
		}

		public void ImportOrUpgradeRelativityApplication(int workspaceArtifactId, string applicationRapFileName = null)
		{
			if (SharedVariables.UseLocalRap)
			{
				ImportRapFromLocalLocation(workspaceArtifactId);
			}
			else
			{
				ImportRapFromBuildPackages(workspaceArtifactId, applicationRapFileName);
			}
		}

		public void DeployIntegrationPointsCustomPage()
		{
			string sqlText =
				$@"Update EDDS.eddsdbo.ApplicationServer set state = 0 where AppGuid = '{IntegrationPoints.Core.Constants
					.IntegrationPoints.RELATIVITY_CUSTOMPAGE_GUID}'";

			_baseServiceContext.ChicagoContext.DBContext.ExecuteNonQuerySQLStatement(sqlText);
		}

		private void ImportApplicationToWorkspace(int workspaceId, string applicationFilePath)
		{
			const int processWaitTimeoutInMin = 5;

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

					Status.WaitForProcessToComplete(proxy, result.ProcessID, (int)TimeSpan.FromMinutes(processWaitTimeoutInMin).TotalSeconds, 500);
				}
				catch (Exception ex)
				{
					throw new Exception($"An error occurred attempting to import the application file {applicationFilePath}. Error: {ex.Message}.");
				}
			}
		}

		private void ImportRapFromLocalLocation(int workspaceArtifactId)
		{
			string applicationFilePath = SharedVariables.RapFileLocation;

			ImportApplicationToWorkspace(workspaceArtifactId, applicationFilePath);
		}

		private void ImportRapFromBuildPackages(int workspaceArtifactId, string applicationRapFileName = null)
		{
			string applicationFilePath = applicationRapFileName == null
				? Path.Combine(SharedVariables.LatestRapLocationFromBuildPackages, SharedVariables.ApplicationPath,
					SharedVariables.ApplicationRapFileName)
				: Path.Combine(SharedVariables.LatestRapLocationFromBuildPackages, SharedVariables.ApplicationPath,
					applicationRapFileName);

			ImportApplicationToWorkspace(workspaceArtifactId, applicationFilePath);
		}

		private ICoreContext GetBaseServiceContext(ClaimsPrincipal claimsPrincipal, int workspaceId)
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
