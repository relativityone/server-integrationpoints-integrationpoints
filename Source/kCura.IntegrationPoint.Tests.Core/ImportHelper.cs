using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity;

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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="documentsTestData"></param>
        /// <returns></returns>
        public bool ImportData(int workspaceArtifactId, DocumentsTestData documentsTestData)
        {
            Messages.Clear();
            ErrorMessages.Clear();

            var importApi = new ImportAPI(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword,
                SharedVariables.RelativityWebApiUrl);

            ImportNativeFiles(workspaceArtifactId, documentsTestData.AllDocumentsDataTable.CreateDataReader(), importApi,
                _CONTROL_NUMBER_FIELD_ARTIFACT_ID);

            ImportImagesAndExtractedText(workspaceArtifactId, documentsTestData.Images, importApi, _CONTROL_NUMBER_FIELD_ARTIFACT_ID);

            return !HasErrors;
        }
        
        public bool ImportToProductionSet(int workspaceID, int productionID, DataTable data)
        {
            Messages.Clear();
            ErrorMessages.Clear();

            var importApi =
                new ImportAPI(
                    SharedVariables.RelativityUserName,
                    SharedVariables.RelativityPassword,
                    SharedVariables.RelativityWebApiUrl);

            ImageImportBulkArtifactJob importJob = importApi.NewImageImportJob();

            importJob.OnMessage += ImportJobOnMessage;
            importJob.OnComplete += ImportJobOnComplete;
            importJob.OnFatalException += ImportJobOnFatalException;

            importJob.Settings.AutoNumberImages = false;

            importJob.Settings.CaseArtifactId = workspaceID;
            importJob.Settings.ExtractedTextFieldContainsFilePath = true;
            importJob.Settings.ExtractedTextEncoding = Encoding.UTF8;

            importJob.Settings.DocumentIdentifierField = "Control Number";

            // Indicates filepath for an image.
            importJob.Settings.FileLocationField = "File";
            //Indicates that the images must be copied to the document repository
            importJob.Settings.CopyFilesToDocumentRepository = true;
            //For testing purpose
            importJob.Settings.DisableImageTypeValidation = true;

            // Specifies the ArtifactID of a document identifier field, such as a control number.
            importJob.Settings.IdentityFieldId = _CONTROL_NUMBER_FIELD_ARTIFACT_ID;
            importJob.Settings.OverwriteMode = OverwriteModeEnum.Overlay;
            importJob.SourceData.SourceData = data;

            // Production settings
            importJob.Settings.BatesNumberField = TestConstants.FieldNames.BATES_BEG;
            importJob.Settings.ForProduction = true;
            importJob.Settings.ProductionArtifactID = productionID;

            importJob.Execute();

            return !HasErrors;
        }

        private void ImportNativeFiles(int workspaceArtifactId, IDataReader dataReader, ImportAPI importApi, int identifyFieldArtifactId)
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

            importJob.Settings.DestinationFolderArtifactID = Workspace.GetRootFolderArtifactIDAsync(workspaceArtifactId).GetAwaiter().GetResult();
            importJob.Settings.FolderPathSourceFieldName = TestConstants.FieldNames.FOLDER_PATH;

            if (!_withNatives)
            {
                importJob.Settings.DisableNativeLocationValidation = null;
                importJob.Settings.DisableNativeValidation = null;
                importJob.Settings.CopyFilesToDocumentRepository = false;
            }

            // Specify the ArtifactID of the document identifier field, such as a control number.
            importJob.Settings.IdentityFieldId = identifyFieldArtifactId;

            importJob.SourceData.SourceData = dataReader;

            importJob.Execute();
        }

        private void ImportImagesAndExtractedText(int workspaceArtifactId, DataTable dataTable,
            IImportAPI importApi, int identifyFieldArtifactId)
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
            //For testing purpose
            importJob.Settings.DisableImageTypeValidation = true;

            // Specifies the ArtifactID of a document identifier field, such as a control number.
            importJob.Settings.IdentityFieldId = identifyFieldArtifactId;
            importJob.Settings.OverwriteMode = OverwriteModeEnum.Overlay;
            importJob.SourceData.SourceData = dataTable;

            importJob.Execute();
        }

        private void ImportJobOnFatalException(JobReport jobreport)
        {
            if (jobreport.FatalException != null)
            {
                ErrorMessages.Add(jobreport.FatalException.ToString());
            }

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