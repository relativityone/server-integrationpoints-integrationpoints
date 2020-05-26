using ARMTestServices.Services.Interfaces;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Org.BouncyCastle.Apache.Bzip2;
using Relativity.Kepler.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class FileshareHelper
	{
		private readonly ITestHelper _helper;

		public FileshareHelper(ITestHelper helper)
		{
			_helper = helper;
		}

		public async Task UploadDirectory(string directoryPath, string destinationPath)
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
	}
}
