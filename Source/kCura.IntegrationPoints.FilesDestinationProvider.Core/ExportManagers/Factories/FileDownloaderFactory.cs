using System;
using System.IO;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers.Factories
{
	internal class FileDownloaderFactory : IExportFileDownloaderFactory
	{
		public IExportFileDownloader Create(ExportFile exportFile)
		{
			ValidateExportFile(exportFile);

			string destinationFolderPath = GetDestinationFolderPath(exportFile.CaseInfo);
			return new FileDownloader(
				exportFile.Credential,
				destinationFolderPath,
				exportFile.CaseInfo.DownloadHandlerURL,
				exportFile.CookieContainer);
		}

		internal static string GetDestinationFolderPath(CaseInfo caseInfo)
		{
			string documentPath = GetDocumentPath(caseInfo);
			string workspaceDirectoryName = $"EDDS{caseInfo.ArtifactID}";

			return Path.Combine(documentPath, workspaceDirectoryName);
		}

		private static string GetDocumentPath(CaseInfo caseInfo)
		{
			return caseInfo?.DocumentPath
				   ?? throw new ArgumentException($"{nameof(caseInfo.DocumentPath)} cannot be null");
		}

		private static void ValidateExportFile(ExportFile exportFile)
		{
			if (exportFile == null)
			{
				throw new ArgumentNullException(nameof(exportFile));
			}

			if (exportFile.CaseInfo == null)
			{
				throw new ArgumentException($"{nameof(exportFile.CaseInfo)} cannot be null");
			}
		}
	}
}
