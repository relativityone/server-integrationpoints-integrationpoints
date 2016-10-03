using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Model;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Status = kCura.Relativity.DataReaderClient.Status;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
	public class ImportHelper
	{
		private const int _CONTROL_NUMBER_FIELD_ARTIFACT_ID = 1003667;
		private readonly ConfigSettings _configSettings;

		internal ImportHelper(ConfigSettings configSettings)
		{
			_configSettings = configSettings;
		}

		internal void ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			CreateFolders(workspaceArtifactId, documentsTestData);

			var importApi = new ImportAPI(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword,
				SharedVariables.RelativityWebApiUrl);

			foreach (var folderWithDocuments in documentsTestData.Documents)
			{
				ImportNativeFiles(workspaceArtifactId, folderWithDocuments.Documents.CreateDataReader(), importApi,
					_CONTROL_NUMBER_FIELD_ARTIFACT_ID, folderWithDocuments.FolderId);
			}

			ImportImagesAndExtractedText(workspaceArtifactId, documentsTestData.Images, importApi, _CONTROL_NUMBER_FIELD_ARTIFACT_ID);
		}

		private void CreateFolders(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			var queue = new Queue<FolderWithDocuments>(documentsTestData.Documents);

			while (queue.Count > 0)
			{
				var folderWithDocuments = queue.Dequeue();
				if ((folderWithDocuments.ParentFolderWithDocuments != null) && !folderWithDocuments.ParentFolderWithDocuments.FolderId.HasValue)
				{
					queue.Enqueue(folderWithDocuments);
				}
				folderWithDocuments.FolderId = Folder.CreateFolder(workspaceArtifactId, folderWithDocuments.FolderName, folderWithDocuments.ParentFolderWithDocuments?.FolderId);
			}
		}

		private static void ImportNativeFiles(int workspaceArtifactId, IDataReader dataReader, ImportAPI importApi, int identifyFieldArtifactId, int? folderId)
		{
			var importJob = importApi.NewNativeDocumentImportJob();
			importJob.OnMessage += ImportJobOnMessage;
			importJob.OnComplete += ImportJobOnComplete;
			importJob.OnFatalException += ImportJobOnFatalException;
			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.ExtractedTextFieldContainsFilePath = false;

			// Indicates file path for the native file.
			importJob.Settings.NativeFilePathSourceFieldName = "Native File";
			importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.Append;
			importJob.Settings.FileNameColumn = "File Name";
			importJob.Settings.CopyFilesToDocumentRepository = true;
			importJob.Settings.DestinationFolderArtifactID = folderId.Value;

			// Specify the ArtifactID of the document identifier field, such as a control number.
			importJob.Settings.IdentityFieldId = identifyFieldArtifactId;

			importJob.SourceData.SourceData = dataReader;

			Console.WriteLine("Executing import native files...");

			importJob.Execute();
		}

		private static void ImportImagesAndExtractedText(int workspaceArtifactId, DataTable dataTable,
			ImportAPI importApi, int identifyFieldArtifactId)
		{
			var importJob = importApi.NewImageImportJob();

			importJob.OnMessage += ImportJobOnMessage;
			importJob.OnComplete += ImportJobOnComplete;
			importJob.OnFatalException += ImportJobOnFatalException;

			importJob.Settings.AutoNumberImages = false;

			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.ExtractedTextFieldContainsFilePath = true;
			importJob.Settings.ExtractedTextEncoding = Encoding.UTF8;

			importJob.Settings.DocumentIdentifierField = "Control Number";

			// Indicates filepath for an image.
			importJob.Settings.FileLocationField = "File";
			importJob.Settings.BatesNumberField = "Bates Beg";
			//Indicates that the images must be copied to the document repository
			importJob.Settings.CopyFilesToDocumentRepository = true;

			// Specifies the ArtifactID of a document identifier field, such as a control number.
			importJob.Settings.IdentityFieldId = identifyFieldArtifactId;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.Overlay;
			importJob.SourceData.SourceData = dataTable;

			Console.WriteLine("Executing native import...");

			importJob.Execute();
		}

		private static void ImportJobOnFatalException(JobReport jobreport)
		{
			if (jobreport.ErrorRows.Any())
			{
				jobreport.ErrorRows.ToList().ForEach(error => Console.WriteLine(error.Message));
			}
		}

		private static void ImportJobOnComplete(JobReport jobreport)
		{
			if (jobreport.ErrorRows.Any())
			{
				jobreport.ErrorRows.ToList().ForEach(error => Console.WriteLine(error.Message));
			}
		}

		private static void ImportJobOnMessage(Status status)
		{
			Console.WriteLine(status.Message);
		}
	}
}