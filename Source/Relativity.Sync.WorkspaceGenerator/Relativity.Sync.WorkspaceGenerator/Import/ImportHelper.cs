using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Relativity.Sync.WorkspaceGenerator.RelativityServices;
using Relativity.Sync.WorkspaceGenerator.Settings;

namespace Relativity.Sync.WorkspaceGenerator.Import
{
    internal sealed class ImportHelper
    {
        private const int _CONTROL_NUMBER_FIELD_ARTIFACT_ID = 1003667;

        private readonly IWorkspaceService _workspaceService;
        private readonly GeneratorSettings _settings;
        private readonly TestCase _testCase;
        private readonly IDataReaderProvider _dataReaderProvider;

        public ImportHelper(IWorkspaceService workspaceService, IDataReaderProvider dataReaderProvider, GeneratorSettings settings, TestCase testCase)
        {
            _workspaceService = workspaceService;
            _settings = settings;
            _testCase = testCase;
            _dataReaderProvider = dataReaderProvider;
        }

        #region Document import
        public async Task<IList<ImportJobResult>> ImportDataAsync(int workspaceArtifactId)
        {
            IList<ImportJobResult> jobResults = new List<ImportJobResult>();

            IDataReader dataReader;
            while ((dataReader = _dataReaderProvider.GetNextDocumentDataReader()) != null)
            {
                Console.WriteLine("Creating ImportAPI client");
                var importApi =
                    new ImportAPI(
                        _settings.RelativityUserName,
                        _settings.RelativityPassword,
                        _settings.RelativityWebApiUri.ToString());

                Console.WriteLine("Importing documents");
                ImportJobResult result = await ConfigureAndRunImportApiJobAsync(workspaceArtifactId, dataReader, importApi).ConfigureAwait(false);

                if (_testCase.GenerateImages)
                {
                    Console.WriteLine("Importing images");
                }

                jobResults.Add(result);
            }

            return jobResults;
        }

        private async Task<ImportJobResult> ConfigureAndRunImportApiJobAsync(int workspaceArtifactId, IDataReader dataReader, ImportAPI importApi)
        {
            ImportBulkArtifactJob importJob = importApi.NewNativeDocumentImportJob();
            importJob.Settings.CaseArtifactId = workspaceArtifactId;
            importJob.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
            importJob.Settings.DestinationFolderArtifactID = await _workspaceService.GetRootFolderArtifactIDAsync(workspaceArtifactId).ConfigureAwait(false);
            importJob.Settings.IdentityFieldId = _CONTROL_NUMBER_FIELD_ARTIFACT_ID; // Specify the ArtifactID of the document identifier field, such as a control number.
            importJob.SourceData.SourceData = dataReader;

            // Extracted text fields
            if (_testCase.GenerateExtractedText)
            {
                importJob.Settings.ExtractedTextFieldContainsFilePath = true;
                importJob.Settings.ExtractedTextEncoding = Encoding.UTF8;
            }
            else
            {
                importJob.Settings.ExtractedTextFieldContainsFilePath = false;
            }

            // Indicates file path for the native file.
            if (_testCase.GenerateNatives)
            {
                importJob.Settings.CopyFilesToDocumentRepository = true;
                importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.CopyFiles;
                importJob.Settings.NativeFilePathSourceFieldName = ColumnNames.NativeFilePath;
                importJob.Settings.FileNameColumn = ColumnNames.FileName;
            }
            else
            {
                importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;
            }

            importJob.OnMessage += status =>
            {
                Console.WriteLine(status.Message);
            };

            return await ImportJobExecutor.ExecuteAsync(importJob).ConfigureAwait(false);
        }
        #endregion

        #region Image import
        public async Task<IEnumerable<ImportJobResult>> ImportImagesAsync(int workspaceArtifactId, int? productionId)
        {
            IList<ImportJobResult> jobResults = new List<ImportJobResult>();

            IDataReaderWrapper dataReader;
            while ((dataReader = _dataReaderProvider.GetNextImageDataReader()) != null)
            {
                Console.WriteLine("Creating ImportAPI client");
                var importApi =
                    new ImportAPI(
                        _settings.RelativityUserName,
                        _settings.RelativityPassword,
                        _settings.RelativityWebApiUri.ToString());


                Console.WriteLine("Importing images");

                ImportJobResult result = await ConfigureAndRunImageImportApiJobAsync(workspaceArtifactId, dataReader.ReadToSimpleDataTable(), importApi, productionId).ConfigureAwait(false);

                jobResults.Add(result);
            }

            return jobResults;
        }

        private async Task<ImportJobResult> ConfigureAndRunImageImportApiJobAsync(int workspaceArtifactId,
            DataTable dataTable, ImportAPI importApi, int? productionId)
        {
            ImageImportBulkArtifactJob importJob = importApi.NewImageImportJob();
            importJob.Settings.CaseArtifactId = workspaceArtifactId;
            importJob.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
            importJob.Settings.DestinationFolderArtifactID = await _workspaceService.GetRootFolderArtifactIDAsync(workspaceArtifactId).ConfigureAwait(false);
            importJob.Settings.IdentityFieldId = _CONTROL_NUMBER_FIELD_ARTIFACT_ID; // Specify the ArtifactID of the document identifier field, such as a control number.
            importJob.SourceData.SourceData = dataTable;

            importJob.Settings.DocumentIdentifierField = ColumnNames.ControlNumber;
            importJob.Settings.FileLocationField = ColumnNames.ImageFilePath;
            importJob.Settings.BatesNumberField = ColumnNames.BegBates;
            importJob.Settings.FileNameField = ColumnNames.ImageFileName;

            importJob.Settings.CopyFilesToDocumentRepository = true;

            if (productionId != null)
            {
                importJob.Settings.ForProduction = true;
                importJob.Settings.ProductionArtifactID = productionId.Value;
            }

            importJob.OnMessage += status =>
            {
                Console.WriteLine(status.Message);
            };

            return await ImportJobExecutor.ExecuteAsync(importJob).ConfigureAwait(false);
        }

        #endregion
    }

}