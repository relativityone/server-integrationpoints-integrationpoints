using Relativity.Services.Interfaces.LibraryApplication.Models;
using System;
using System.IO;
using ARMTestServices.Services.Interfaces;
using Relativity.Kepler.Transport;
using Relativity.Automation.Utility.Api;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Models;
using Relativity.Automation.Utility.Orchestrators;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public class FileShareHelper
	{
		private bool _isInitialized;
		private readonly ApiComponent _component;
		private readonly AzureStorageHelper _storageHelper;

		private FileShareHelper(AzureStorageHelper storageHelper)
		{
			_component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			_storageHelper = storageHelper;
		}

		public static FileShareHelper CreateInstance()
		{
			FileShareHelper instance = new FileShareHelper(
				AzureStorageHelper.CreateFromTestConfig());
			
			instance.Initialize();

			return instance;
		}

		public void Initialize()
		{
			if(!_isInitialized)
			{
				_component.OrchestratorFactory.Create<IOrchestrateInstanceSettings>()
					.SetInstanceSetting("DevelopmentMode", "True", "kCura.ARM", InstanceSettingValueTypeEnum.TrueFalse);

				if (!IsAppInstalled())
				{
					InstallARMTestServices();
				}

				_isInitialized = true;
			}
		}

		private void InstallARMTestServices()
		{
			string rapPath = GetTestServicesRapPath();

			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
			_component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.InstallRelativityApplicationToLibrary(rapPath, options);
		}

		private string GetTestServicesRapPath()
		{
			string rapPath = Path.Combine(Path.GetTempPath(), "ARMTestServices.rap");
			if (!File.Exists(rapPath))
			{
				rapPath = _storageHelper.DownloadFile(@"ARM\ARMTestServices.rap", Path.GetTempPath());
			}

			return rapPath;
		}

		private bool IsAppInstalled()
		{
			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = "ARM Test Services" };
			bool isAppInstalled = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.IsApplicationInstalledInLibrary(app);

			return isAppInstalled;
		}

		public string UploadFile(string filePath, string directory)
		{
			if(!Path.IsPathRooted(directory))
			{
				directory = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), directory);
			}

			string destinationFile = Path.Combine(directory, $"{Guid.NewGuid()}.zip");

			using (var fileShareManager = _component.ServiceFactory.GetAdminServiceProxy<IFileshareManager>())
			{
				using (Stream stream = File.OpenRead(filePath)) //Temp path if only name
				{
					fileShareManager.UploadStream(new KeplerStream(stream), destinationFile).GetAwaiter().GetResult();
				}
			}

			return destinationFile;
		}

		public void CreateDirectory(string directory)
		{
			if (!Path.IsPathRooted(directory))
			{
				directory = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), directory);
			}

			using (var directoryManager = _component.ServiceFactory.GetAdminServiceProxy<IDirectoryManager>())
			{
				bool exists = directoryManager.DirectoryExists(directory).Result;
				if(!exists)
				{
					directoryManager.DirectoryCreate(directory);
				}
			}
		}
	}
}
