using System;
using System.IO;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Core;
using Relativity.Core.DTO;
using Relativity.Core.Service;
using Relativity.Services.ApplicationInstallManager;
using File = System.IO.File;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class RelativityApplicationManager
	{
		private readonly ITestHelper _helper;
		private readonly LibraryApplicationManager _libraryManager;
		private readonly ICoreContext _baseServiceContext;

		public RelativityApplicationManager(ICoreContext coreContext, ITestHelper helper)
		{
			_helper = helper;
			_libraryManager = new LibraryApplicationManager();
			_baseServiceContext = coreContext;
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

		public void ImportApplicationToLibrary()
		{
			LibraryApplication libraryApplication = GetLibraryApplicationDTO(new Guid(IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING));
			
			if (libraryApplication != null && libraryApplication.IsVisible)
			{
				RemoveApplicationFromLibrary(libraryApplication);
			}

			string applicationFilePath = SharedVariables.UseLocalRap ? GetLocalRapPath() : GetBuildPackagesRapPath();

			UpdateLibraryApplicationRap(applicationFilePath, libraryApplication);
		}

		public void InstallApplicationFromLibrary(int workspaceArtifactId)
		{
			using (var applicationInstallManager = _helper.CreateAdminProxy<IApplicationInstallManager>())
			{
				applicationInstallManager.InstallLibraryApplicationByGuid(workspaceArtifactId, new Guid(IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING)).Wait();
			}
		}

		public bool IsGetApplicationInstalled(int workspaceArtifactId)
		{
			var caseApplicationManager = new CaseApplicationManager();
			int appId = caseApplicationManager.FindCurrentApplicationInstallID(_baseServiceContext.ChicagoContext, workspaceArtifactId,
					new Guid(IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING));
			
			return appId > 0;
		}

		public void DeployIntegrationPointsCustomPage()
		{
			string sqlText =
				$@"Update EDDS.eddsdbo.ApplicationServer set state = 0 where AppGuid = '{IntegrationPoints.Core.Constants
					.IntegrationPoints.RELATIVITY_CUSTOMPAGE_GUID}'";

			_baseServiceContext.ChicagoContext.DBContext.ExecuteNonQuerySQLStatement(sqlText);
		}

		private void UpdateLibraryApplicationRap(string applicationFilePath, LibraryApplication libraryApplication)
		{
			var rapData = File.ReadAllBytes(applicationFilePath);
			libraryApplication.FileData = rapData;
			_libraryManager.Update(_baseServiceContext as BaseServiceContext, libraryApplication, false, false);
		}

		private string GetLocalRapPath()
		{
			return SharedVariables.RapFileLocation;
		}

		private string GetBuildPackagesRapPath()
		{
			return Path.Combine(SharedVariables.LatestRapLocationFromBuildPackages, SharedVariables.ApplicationPath, SharedVariables.ApplicationRapFileName);
		}

		
	}
}
