using Relativity.Services.Interfaces.LibraryApplication.Models;
using System;
using System.IO;
using ARMTestServices.Services.Interfaces;
using Relativity.Kepler.Transport;
using Relativity.Automation.Utility.Api;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Models;
using Relativity.Automation.Utility.Orchestrators;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public class FileShareHelper
	{
		private static bool _isInitialized;

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

		private bool IsAppInstalled()
		{
			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = "ARM Test Services" };
			bool isAppInstalled = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.IsApplicationInstalledInLibrary(app);

			return isAppInstalled;
		}

		private void InstallARMTestServices()
		{
			string rapPath = GetTestServicesRapPathAsync().Result;

			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
			_component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.InstallRelativityApplicationToLibrary(rapPath, options);
		}

		private async Task<string> GetTestServicesRapPathAsync()
		{
			string rapPath = Path.Combine(Path.GetTempPath(), "ARMTestServices.rap");
			if (!File.Exists(rapPath))
			{
				rapPath = await _storageHelper.DownloadFileAsync(@"ARM\ARMTestServices.rap", Path.GetTempPath()).ConfigureAwait(false);
			}

			return rapPath;
		}

		public async Task<string> UploadFileAsync(string filePath, string directory)
		{
			if(!Path.IsPathRooted(directory))
			{
				directory = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), directory);
			}

			string destinationFile = Path.Combine(directory, Path.GetFileName(filePath));

			using (var fileShareManager = _component.ServiceFactory.GetAdminServiceProxy<IFileshareManager>())
			using (var fileManager = _component.ServiceFactory.GetAdminServiceProxy<IFileManager>())
			{
				bool fileExists = await fileManager.FileExists(destinationFile).ConfigureAwait(false);
				if (!fileExists)
				{
					using (Stream stream = File.OpenRead(filePath)) //Temp path if only name
					{
						await fileShareManager.UploadStream(new KeplerStream(stream), destinationFile).ConfigureAwait(false);
					}
				}
			}

			return destinationFile;
		}

		public async Task CreateDirectoryAsync(string directory)
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
					await directoryManager.DirectoryCreate(directory).ConfigureAwait(false);
				}
			}
		}
	}
}
