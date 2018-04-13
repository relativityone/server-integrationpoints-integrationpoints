using System.IO;

namespace kCura.IntegrationPoints.UITests.Auxiliary
{
	public class FileHelper
	{
		public static void CopyDirectory(string originalPath, string destinationPath)
		{
			CopyFilesInDirectory(originalPath, destinationPath);
		}

		private static void CopyFilesInDirectory(string originalPath, string destinationPath)
		{
			if (!Directory.Exists(destinationPath))
			{
				Directory.CreateDirectory(destinationPath);
			}

			var directory = new DirectoryInfo(originalPath);

			FileInfo[] files = directory.GetFiles();
			foreach (var file in files)
			{
				string fileName = file.Name;
				string originalFilePath = $"{originalPath}\\{fileName}";
				string destinationFilePath = $"{destinationPath}\\{fileName}";
				CopyFile(originalFilePath, destinationFilePath);
			}

			DirectoryInfo[] subdirectories = directory.GetDirectories();
			foreach (var subdirectory in subdirectories)
			{
				string subdirectoryName = subdirectory.Name;
				string originalSubdirectoryPath = $"{originalPath}\\{subdirectoryName}";
				string destinationSubdirectoryPath = $"{destinationPath}\\{subdirectoryName}";
				CopyFilesInDirectory(originalSubdirectoryPath, destinationSubdirectoryPath);
			}
		}

		public static void CopyFile(string originalLocation, string destinationLocation)
		{
			File.Copy(originalLocation, destinationLocation, true);
		}
	}
}
