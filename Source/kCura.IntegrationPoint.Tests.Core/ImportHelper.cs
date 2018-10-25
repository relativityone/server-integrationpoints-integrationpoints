using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace kCura.IntegrationPoint.Tests.Core
{
	public class ImportHelper
	{
		private const int _CONTROL_NUMBER_FIELD_ARTIFACT_ID = 1003667;

		private readonly bool _withNatives;

		public List<string> Messages { get; } = new List<string>();

		public List<string> ErrorMessages { get; } = new List<string>();

		public ImportHelper(bool withNatives = true)
		{
			_withNatives = withNatives;
		}
		public bool HasErrors => ErrorMessages.Any();
		
		public bool ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			Messages.Clear();
			ErrorMessages.Clear();

			CreateFolders(workspaceArtifactId, documentsTestData);
			WinEDDS.Config.ConfigSettings[nameof(WinEDDS.Config.TapiForceHttpClient)] = true.ToString();
			WinEDDS.Config.ConfigSettings[nameof(WinEDDS.Config.TapiForceBcpHttpClient)] = true.ToString();
			var importApi = new ImportAPI(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword,
				SharedVariables.RelativityWebApiUrl);

			ImportNativeFiles(workspaceArtifactId, documentsTestData.AllDocumentsDataTable.CreateDataReader(), importApi,
				_CONTROL_NUMBER_FIELD_ARTIFACT_ID, documentsTestData.Documents.First().FolderId);

			ImportImagesAndExtractedText(workspaceArtifactId, documentsTestData.Images, importApi, _CONTROL_NUMBER_FIELD_ARTIFACT_ID);

			return !HasErrors;
		}

		private void CreateFolders(int workspaceArtifactId, DocumentsTestData documentsTestData)
		{
			var queue = new Queue<FolderWithDocuments>(documentsTestData.Documents);

			while (queue.Count > 0)
			{
				FolderWithDocuments folderWithDocuments = queue.Dequeue();
				if ((folderWithDocuments.ParentFolderWithDocuments != null) && !folderWithDocuments.ParentFolderWithDocuments.FolderId.HasValue)
				{
					queue.Enqueue(folderWithDocuments);
				}
				folderWithDocuments.FolderId = FolderService.CreateFolder(workspaceArtifactId, folderWithDocuments.FolderName, folderWithDocuments.ParentFolderWithDocuments?.FolderId);
			}
		}

		private void ImportNativeFiles(int workspaceArtifactId, IDataReader dataReader, ImportAPI importApi, int identifyFieldArtifactId, int? folderId)
		{
			ImportBulkArtifactJob importJob = importApi.NewNativeDocumentImportJob();
			importJob.OnMessage += ImportJobOnMessage;
			importJob.OnComplete += ImportJobOnComplete;
			importJob.OnFatalException += ImportJobOnFatalException;
			importJob.Settings.CaseArtifactId = workspaceArtifactId;
			importJob.Settings.ExtractedTextFieldContainsFilePath = false;

			// Indicates file path for the native file.
			importJob.Settings.NativeFilePathSourceFieldName = "Native File";
			importJob.Settings.NativeFileCopyMode = _withNatives ? NativeFileCopyModeEnum.CopyFiles : NativeFileCopyModeEnum.DoNotImportNativeFiles;
			importJob.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
			importJob.Settings.FileNameColumn = "File Name";
			importJob.Settings.CopyFilesToDocumentRepository = _withNatives;
			importJob.Settings.DestinationFolderArtifactID = folderId.Value;

			if (!_withNatives)
			{
				importJob.Settings.DisableNativeLocationValidation = null;
				importJob.Settings.DisableNativeValidation = null;
				importJob.Settings.CopyFilesToDocumentRepository = false;
			}

			// Specify the ArtifactID of the document identifier field, such as a control number.
			importJob.Settings.IdentityFieldId = identifyFieldArtifactId;

			importJob.SourceData.SourceData = dataReader;

			Console.WriteLine(@"Executing import native files...");

			importJob.Execute();
		}

		private void ImportImagesAndExtractedText(int workspaceArtifactId, DataTable dataTable,
			ImportAPI importApi, int identifyFieldArtifactId)
		{
			ImageImportBulkArtifactJob importJob = importApi.NewImageImportJob();

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

			Console.WriteLine(@"Executing native import...");

			importJob.Execute();
		}

		private void ImportJobOnFatalException(JobReport jobreport)
		{
			if (jobreport.ErrorRows.Any())
			{
				jobreport.ErrorRows.ToList().ForEach(error =>
				{
					ErrorMessages.Add(error.Message);
				});
			}
		}

		private void ImportJobOnComplete(JobReport jobreport)
		{
			if (jobreport.ErrorRows.Any())
			{
				jobreport.ErrorRows.ToList().ForEach(error => ErrorMessages.Add(error.Message));
			}
		}

		private void ImportJobOnMessage(Relativity.DataReaderClient.Status status)
		{
			Messages.Add(status.Message);
		}
	}
}