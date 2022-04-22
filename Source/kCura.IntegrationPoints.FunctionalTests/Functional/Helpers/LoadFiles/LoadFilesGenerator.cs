using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using System;
using System.IO;
using System.Threading.Tasks;
using ARMTestServices.Services.Interfaces;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using System.IO.Compression;
using Relativity.Kepler.Transport;
using NUnit.Framework;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles
{
	internal static class LoadFilesGenerator
	{
		private const string NATIVES_LOAD_FILE_HEADER = "Control Number,FILE_PATH";
		private const string NATIVES_DAT_LOAD_FILE_HEADER = "^Control Number^|^FILE_PATH^|^File Size^|^File Name^|^Folder_Path^";

		private static readonly string NATIVES_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\LoadFiles\NativesLoadFile.csv");
		private static readonly string NATIVES_LIMITED_ITEMS_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\LoadFiles\NativesPartialDataLoadFile.csv");
		private static readonly string NATIVES_DAT_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\LoadFiles\NativesLoadFile.dat");
		private static readonly string IMAGES_OPT_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\Images\ImagesLoadFile.opt");
		private static readonly string IMAGES_LIMITED_ITEMS_OPT_LOAD_FILE_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Functional\Helpers\Images\ImagesPartialLoadFile.opt");
		private static readonly string NATIVES_FOR_LOAD_FILE_FOLDER_PATH = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Functional\Helpers\LoadFiles");
		private static readonly string IMAGES_FOR_LOAD_FILE_FOLDER_PATH = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Functional\Helpers\Images");

		public static string CreateNativesLoadFileWithLimitedItems(int limit)
		{
			if (File.Exists(NATIVES_LIMITED_ITEMS_LOAD_FILE_PATH))
			{
				return NATIVES_LIMITED_ITEMS_LOAD_FILE_PATH;
			}

			using (FileStream nativesLoadFileStream = new FileStream(NATIVES_LIMITED_ITEMS_LOAD_FILE_PATH, FileMode.Create))
			{
				using (StreamWriter nativesLoadFileWriter = new StreamWriter(nativesLoadFileStream))
				{
					nativesLoadFileWriter.WriteLine(NATIVES_LOAD_FILE_HEADER);

					foreach (var native in Natives.NATIVES.Take(limit))
					{
						nativesLoadFileWriter.WriteLine($"{native.Key},{native.Value}");
					}
				}
			}
			return NATIVES_LIMITED_ITEMS_LOAD_FILE_PATH;
		}

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
				return NATIVES_FOR_LOAD_FILE_FOLDER_PATH;
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
			return NATIVES_FOR_LOAD_FILE_FOLDER_PATH;
		}

		public static string GetOrCreateNativesOptLoadFile()
		{
			if (File.Exists(IMAGES_OPT_LOAD_FILE_PATH))
			{
				return IMAGES_FOR_LOAD_FILE_FOLDER_PATH;
			}

			using (FileStream optLoadFileStream = new FileStream(IMAGES_OPT_LOAD_FILE_PATH, FileMode.Create))
			{
				using (StreamWriter optLoadFileWriter = new StreamWriter(optLoadFileStream))
				{
					for (int i=0; i<10; i++)
					{
						string line = String.Format("IMPORT_SMALL_IMAGES_000000000{0},1000000,.\\IMAGE_62K.tif,Y,,", i);
						optLoadFileWriter.WriteLine(line);
					}
				}
			}
			return IMAGES_FOR_LOAD_FILE_FOLDER_PATH;
		}

		public static string GetOrCreateNativesOptLoadFileWithLimitedItems(int limit)
		{
			if (File.Exists(IMAGES_LIMITED_ITEMS_OPT_LOAD_FILE_PATH))
			{
				return IMAGES_LIMITED_ITEMS_OPT_LOAD_FILE_PATH;
			}

			using (FileStream optLoadFileStream = new FileStream(IMAGES_LIMITED_ITEMS_OPT_LOAD_FILE_PATH, FileMode.Create))
			{
				using (StreamWriter optLoadFileWriter = new StreamWriter(optLoadFileStream))
				{
					for (int i=0; i< limit; i++)
					{
						string line = String.Format("IMPORT_SMALL_IMAGES_000000000{0},1000000,.\\IMAGE_62K.tif,Y,,", i);
						optLoadFileWriter.WriteLine(line);
					}
				}
			}
			return IMAGES_LIMITED_ITEMS_OPT_LOAD_FILE_PATH;
		}

		public static async Task UploadLoadFileToImportDirectory(int workspaceId, string testDataPath)
        {
			string destinationPath;
			using (var proxy = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IWorkspaceManager>())
			{
				WorkspaceRef workspace = new WorkspaceRef() { ArtifactID = workspaceId };

				FileShareResourceServer server = await proxy.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).ConfigureAwait(false);

				destinationPath = Path.Combine(server.UNCPath, $"EDDS{workspaceId}");
			}
			destinationPath = Path.Combine(destinationPath, "DataTransfer\\Import");

			string zippedDirectory = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
			ZipFile.CreateFromDirectory(testDataPath, zippedDirectory);

			using (var fileShareManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IFileshareManager>())
			using (var fileManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IFileManager>())
			{
				string destinationFile = Path.Combine(destinationPath, Path.GetFileName(zippedDirectory));

                bool fileExists = await fileManager.FileExists(destinationFile).ConfigureAwait(false);
                if (!fileExists)
                {
                    using (Stream stream = File.OpenRead(zippedDirectory))
                    {
                        await fileShareManager.UploadStream(new KeplerStream(stream), destinationFile).ConfigureAwait(false);
                    }
                }
            }
		}
    }
}
