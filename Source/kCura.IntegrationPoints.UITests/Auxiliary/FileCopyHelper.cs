using System.IO;

namespace kCura.IntegrationPoints.UITests.Auxiliary
{
	public class FileCopyHelper
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
				string originalFilePath = Path.Combine(originalPath, fileName);
				string destinationFilePath = Path.Combine(destinationPath, fileName);
				CopyFile(originalFilePath, destinationFilePath);
			}

			DirectoryInfo[] subdirectories = directory.GetDirectories();
			foreach (var subdirectory in subdirectories)
			{
				string subdirectoryName = subdirectory.Name;
				string originalSubdirectoryPath = Path.Combine(originalPath, subdirectoryName);
				string destinationSubdirectoryPath = Path.Combine(destinationPath, subdirectoryName);
				CopyFilesInDirectory(originalSubdirectoryPath, destinationSubdirectoryPath);
			}
		}

		public static void CopyFile(string originalPath, string destinationPath)
		{
			File.Copy(originalPath, destinationPath, true);
		}
	}
}
