using System;
using System.IO;
using System.Threading.Tasks;
using ARMTestServices.Services.Interfaces;
using Relativity.Kepler.Transport;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public class FileShareHelper
	{
		private readonly IKeplerServiceFactory _serviceFactory;

		private static bool _isInitialized;

		private readonly AzureStorageHelper _storageHelper;

		private FileShareHelper(AzureStorageHelper storageHelper)
		{
			_serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;

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
				Configure();

				if (!IsAppInstalled())
				{
					InstallARMTestServices();
				}

				_isInitialized = true;
			}
		}

		private void Configure()
		{
			IInstanceSettingsService instanceSettingsService = RelativityFacade.Instance.Resolve<IInstanceSettingsService>();

			var developmentModeSetting = instanceSettingsService.Get("DevelopmentMode", "kCura.ARM");
			if (developmentModeSetting == null)
			{
				instanceSettingsService.Create(new Testing.Framework.Models.InstanceSetting
				{
					Name = "DevelopmentMode",
					Section = "kCura.ARM",
					Value = "True",
					ValueType = InstanceSettingValueType.TrueFalse
				});
			}
		}

		private bool IsAppInstalled()
		{
			LibraryApplication armTestServicesApp = RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
				.Get("ARM Test Services");

			return armTestServicesApp != null;
		}

		private void InstallARMTestServices()
		{
			string rapPath = GetTestServicesRapPathAsync().Result;
			try
			{
				RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
					.InstallToLibrary(rapPath, new LibraryApplicationInstallOptions()
				{
					CreateIfMissing = true
				});
			}
			finally
			{
				File.Delete(rapPath);
			}

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

			using (var fileShareManager = _serviceFactory.GetServiceProxy<IFileshareManager>())
			using (var fileManager = _serviceFactory.GetServiceProxy<IFileManager>())
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

			using (var directoryManager = _serviceFactory.GetServiceProxy<IDirectoryManager>())
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
