﻿using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Kepler.Transport;
using Relativity.Services.ApplicationInstallManager;
using Relativity.Services.LibraryApplicationsManager;
using System;
using System.IO;
using System.Threading.Tasks;
using File = System.IO.File;

namespace kCura.IntegrationPoint.Tests.Core
{
	using global::Relativity.Services.ApplicationInstallManager.Models;

#pragma warning disable CS0618 // Type or member is obsolete
	public class RelativityApplicationManager
	{
		private const string _RIP_GUID_STRING = IntegrationPoints.Core.Constants.IntegrationPoints.APPLICATION_GUID_STRING;

		private const string _TESTING_RAP_NAME = "Integration Points";

		private readonly ITestHelper _helper;

		private readonly ILibraryApplicationsManager _libraryManager;

		public RelativityApplicationManager(ITestHelper helper)
		{
			_helper = helper;
			_libraryManager = helper.CreateProxy<ILibraryApplicationsManager>();
		}
		
		public async Task ImportRipToLibraryAsync()
		{
			string applicationFilePath = SharedVariables.UseLocalRap 
				? GetLocalRipRapPath() 
				: GetBuildPackagesRipRapPath();
			await ImportApplicationToLibraryAsync(_TESTING_RAP_NAME, applicationFilePath).ConfigureAwait(false);
		}

		public async Task ImportApplicationToLibraryAsync(string name, string applicationFilePath)
		{
			using (FileStream fileStream = File.OpenRead(applicationFilePath))
			{
				var keplerStream = new KeplerStream(fileStream);
				await _libraryManager.EnsureApplication(_TESTING_RAP_NAME, keplerStream, true, true).ConfigureAwait(false);
			}
		}

		public void InstallApplicationFromLibrary(int workspaceArtifactId, string appGuidString = _RIP_GUID_STRING)
		{
			using (var applicationInstallManager = _helper.CreateProxy<IApplicationInstallManager>())
			{
				applicationInstallManager.InstallLibraryApplicationByGuid(workspaceArtifactId, new Guid(appGuidString)).Wait();
			}
		}

		public bool IsApplicationInstalledAndUpToDate(int workspaceArtifactId, string appGuidString = _RIP_GUID_STRING)
		{
			using (var applicationInstallManager = _helper.CreateProxy<IApplicationInstallManager>())
			{
				ApplicationInstallStatus installStatus = applicationInstallManager.GetApplicationInstallStatusAsync(workspaceArtifactId, new Guid(appGuidString)).Result;
				return installStatus == ApplicationInstallStatus.Installed;
			}
		}

		private string GetLocalRipRapPath()
		{
			return SharedVariables.RipRapFilePath;
		}

		private string GetBuildPackagesRipRapPath()
		{
			return Path.Combine(SharedVariables.LatestRapLocationFromBuildPackages, SharedVariables.ApplicationPath, SharedVariables.RipRapFilePath);
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
