using ARMTestServices.Services.Interfaces;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Kepler.Transport;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.ResourceServer;
using Relativity.Services.Workspace;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using IWorkspaceManager = Relativity.Services.Workspace.IWorkspaceManager;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class FileshareHelper
	{
		private readonly ITestHelper _helper;

		public FileshareHelper(ITestHelper helper)
		{
			_helper = helper;
		}

		public async Task UploadDirectoryAsync(string directoryPath, string destinationPath)
		{
			if (!Path.IsPathRooted(destinationPath))
			{
				destinationPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), destinationPath);
			}

			string zippedDirectory = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
			ZipFile.CreateFromDirectory(directoryPath, zippedDirectory);

			string destinationFile = Path.Combine(destinationPath, Path.GetFileName(zippedDirectory));

			using (var fileShareManager = _helper.CreateProxy<IFileshareManager>())
			using (var fileManager = _helper.CreateProxy<IFileManager>())
			{
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

		public async Task<string> GetFilesharePath(int workspaceId)
		{
			using(var proxy = _helper.CreateProxy<IWorkspaceManager>())
			{
				WorkspaceRef workspace = new WorkspaceRef() { ArtifactID = workspaceId };
				
				FileShareResourceServer server = await proxy.GetDefaultWorkspaceFileShareResourceServerAsync(workspace).ConfigureAwait(false);

				return server.UNCPath;
			}
		}
	}
}
