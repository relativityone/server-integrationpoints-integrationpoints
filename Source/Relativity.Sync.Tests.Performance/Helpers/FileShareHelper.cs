using Relativity.Services.Interfaces.LibraryApplication.Models;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Orchestrators;
using System;
using System.IO;
using ARMTestServices.Services.Interfaces;
using Relativity.Kepler.Transport;
using Relativity.Testing.Framework;

namespace Relativity.Sync.Tests.Performance.Helpers
{
	public class FileShareHelper
	{
		private bool _isInitialized;
		private ApiComponent _component;

		private FileShareHelper()
		{
		}

		public static FileShareHelper CreateInstance()
		{
			FileShareHelper instance = new FileShareHelper();
			instance.Initialize();

			return instance;
		}

		public void Initialize()
		{
			if(!_isInitialized)
			{
				RelativityFacade.Instance.RelyOn<ApiComponent>();
				_component = RelativityFacade.Instance.GetComponent<ApiComponent>();

				//TODO: Replace with SetInstanceSetting from RTF
				RTFSubstitute.CreateInstanceSettingIfNotExist(_component.ServiceFactory,
					"DevelopmentMode", "kCura.ARM", Services.InstanceSetting.ValueType.TrueFalse, "True").Wait();

				if (!IsAppInstalled())
				{
					InstallARMTestServices();
				}

				_isInitialized = true;
			}
		}

		private void InstallARMTestServices()
		{
			if (!File.Exists(TestSettingsConfig.ARMTestServicesRapPath))
			{
				throw new FileNotFoundException(@"ARM Test Services RAP not found. To get latest release version run .\DevelopmentScripts\Get-ARM.ps1");
			}

			LibraryApplicationRequestOptions options = new LibraryApplicationRequestOptions() { CreateIfMissing = true };
			_component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.InstallRelativityApplicationToLibrary(TestSettingsConfig.ARMTestServicesRapPath, options);
		}

		private bool IsAppInstalled()
		{
			LibraryApplicationResponse app = new LibraryApplicationResponse() { Name = "ARM Test Services" };
			bool isAppInstalled = _component.OrchestratorFactory.Create<IOrchestrateRelativityApplications>()
				.IsApplicationInstalledInLibrary(app);

			return isAppInstalled;
		}

		public void UploadFile(string filePath, string relativeDirectory)
		{
			if(Path.IsPathRooted(relativeDirectory))
			{
				throw new ArgumentException($"Directory {relativeDirectory} should be relative");
			}

			string destinationPath = Path.Combine(
				Path.GetPathRoot(Environment.SystemDirectory),
				relativeDirectory,
				Path.GetFileName(filePath));

			using (var fileShareManager = _component.ServiceFactory.GetAdminServiceProxy<IFileshareManager>())
			{
				using (Stream stream = File.OpenRead(filePath)) //Temp path if only name
				{
					fileShareManager.UploadStream(new KeplerStream(stream),
						destinationPath).GetAwaiter().GetResult();
				}
			}
		}

		public void CreateDirectory(string directory)
		{
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
