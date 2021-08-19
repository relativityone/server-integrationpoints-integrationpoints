using NUnit.Framework;
using Relativity.Services.ResourceServer;
using Relativity.Services.ServiceProxy;
using Relativity.Services.Workspace;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles
{
	internal static class LoadFilesGenerator
	{
		private const string NATIVES_LOAD_FILE_HEADER = "Control Number,FILE_PATH";
		private const string NATIVES_DAT_LOAD_FILE_HEADER = "^Control Number^|^FILE_PATH^|^File Size^|^File Name^|^Folder_Path^";

		private static readonly string NATIVES_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\LoadFiles\NativesLoadFile.csv");
		private static readonly string NATIVES_DAT_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\LoadFiles\NativesLoadFile.dat");

		public static string GetOrCreateNativesLoadFile()
		{
			if (File.Exists(NATIVES_LOAD_FILE_PATH))
			{
				return NATIVES_LOAD_FILE_PATH;
			}

			using (FileStream nativesLoadFileStream = new FileStream(NATIVES_LOAD_FILE_PATH, FileMode.Create))
			{
				using (StreamWriter nativesLoadFileWriter = new StreamWriter(nativesLoadFileStream))
				{
					nativesLoadFileWriter.WriteLine(NATIVES_LOAD_FILE_HEADER);

					foreach (var native in Natives.NATIVES)
					{
						nativesLoadFileWriter.WriteLine($"{native.Key},{native.Value}");
					}
				}
			}

			return NATIVES_LOAD_FILE_PATH;
		}

		public static string GetOrCreateNativesDatLoadFile()
		{
			if (File.Exists(NATIVES_DAT_LOAD_FILE_PATH))
			{
				return NATIVES_DAT_LOAD_FILE_PATH;
			}

			using (FileStream nativesLoadFileStream = new FileStream(NATIVES_DAT_LOAD_FILE_PATH, FileMode.Create))
			{
				using (StreamWriter nativesLoadFileWriter = new StreamWriter(nativesLoadFileStream))
				{
					nativesLoadFileWriter.WriteLine(NATIVES_DAT_LOAD_FILE_HEADER);

					foreach (var native in Natives.GenerateNativesForLoadFileImport())
					{
						nativesLoadFileWriter.WriteLine($"{native.Value}");
					}
				}
			}

			return NATIVES_DAT_LOAD_FILE_PATH;
		}

		public static void CreateDataToImportInDirectory(string username, string password)
        {
			// first create data 

			// then upload it to RDP 

		}

		private static async Task<string> GetDirectoryPathAsync(int workspaceId, string username, string password)
        {
			ServiceFactory userServiceFactory = GetServiceFactoryForUserHelper(username, password);

			using (var proxy = userServiceFactory.CreateProxy<IWorkspaceManager>())
            {
				WorkspaceRef workspace = new WorkspaceRef() { ArtifactID = workspaceId };

				FileShareResourceServer server = await proxy.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).ConfigureAwait(false);

				return Path.Combine(server.UNCPath, $"EDDS{workspaceId}");
			}
		}

		private static ServiceFactory GetServiceFactoryForUserHelper(string username, string password)
        {
			var userCredential = new UsernamePasswordCredentials(username, password);
			string RelativityBaseAdressUrlValue = $"{TestContext.Parameters["ServerBindingType"]}://{TestContext.Parameters["RelativityHostAddress"]}";
			Uri RelativityRestUri = new Uri($"{RelativityBaseAdressUrlValue}/Relativity.Rest/api");
			ServiceFactorySettings userSettings = new ServiceFactorySettings(RelativityRestUri, userCredential);
			ServiceFactory userServiceFactory = new ServiceFactory(userSettings);
			return userServiceFactory;
		}
	}
}
