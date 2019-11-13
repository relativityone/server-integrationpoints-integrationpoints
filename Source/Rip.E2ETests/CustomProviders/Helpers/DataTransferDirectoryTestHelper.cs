using System.IO;
using kCura.IntegrationPoint.Tests.Core;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class DataTransferDirectoryTestHelper
	{
		public static string CopyFileToImportFolder(int workspaceID, string inputFile)
		{
			string fileName = Path.GetFileName(inputFile);
			string workspaceFolderName = $"EDDS{workspaceID}";

			string destinationLocation = Path.Combine(
				SharedVariables.FileshareLocation,
				workspaceFolderName,
				"DataTransfer",
				"Import",
				fileName);

			File.Copy(inputFile, destinationLocation, overwrite: true);

			return destinationLocation;
		}
	}
}