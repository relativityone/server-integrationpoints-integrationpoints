using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
    internal static class ImportJobFactory
    {
        private const int _CONTROL_NUMBER_FIELD_ARTIFACT_ID = 1003667;

        public static ImportBulkArtifactJob CreateNonNativesDocumentImportJob(int sourceWorkspaceArtifactId, int destinationFolderArtifactId, ImportDataTableWrapper documents)
        {
            ImportAPI importApi = new ImportAPI(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword, AppSettings.RelativityWebApiUrl.ToString());
            ImportBulkArtifactJob job = importApi.NewObjectImportJob((int)ArtifactType.Document);
            job.Settings.CaseArtifactId = sourceWorkspaceArtifactId;
            job.Settings.OverwriteMode = OverwriteModeEnum.AppendOverlay;
            job.Settings.DestinationFolderArtifactID = destinationFolderArtifactId;
            job.Settings.ExtractedTextFieldContainsFilePath = false;
            job.Settings.DisableNativeLocationValidation = null;
            job.Settings.DisableNativeValidation = null;
            job.Settings.CopyFilesToDocumentRepository = false;
            job.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles;
            job.Settings.IdentityFieldId = _CONTROL_NUMBER_FIELD_ARTIFACT_ID;
            job.SourceData.SourceData = documents.DataReader;

            return job;
        }
    }
}
