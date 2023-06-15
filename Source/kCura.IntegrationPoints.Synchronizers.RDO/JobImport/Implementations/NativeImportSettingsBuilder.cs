using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
    public class NativeImportSettingsBuilder : ImportSettingsBaseBuilder<Settings>
    {
        public NativeImportSettingsBuilder(IImportAPI importApi)
            : base(importApi)
        {
        }

        public override void PopulateFrom(ImportSettings importSettings, Settings target)
        {
            base.PopulateFrom(importSettings, target);

            target.ArtifactTypeId = importSettings.DestinationConfiguration.ArtifactTypeId;
            target.BulkLoadFileFieldDelimiter = null;
            target.DisableControlNumberCompatibilityMode = false;
            target.DisableExtractedTextFileLocationValidation = false;
            target.DisableNativeLocationValidation = importSettings.DisableNativeLocationValidation;
            target.DisableNativeValidation = importSettings.DisableNativeValidation;
            target.FileNameColumn = importSettings.FileNameColumn;
            target.FileSizeColumn = importSettings.FileSizeColumn;
            target.FileSizeMapped = importSettings.FileSizeMapped;
            target.FolderPathSourceFieldName = importSettings.FolderPathSourceFieldName;
            target.MultiValueDelimiter = importSettings.MultiValueDelimiter;
            target.NativeFilePathSourceFieldName = importSettings.NativeFilePathSourceFieldName;
            target.NestedValueDelimiter = importSettings.NestedValueDelimiter;
            target.OIFileIdColumnName = importSettings.OIFileIdColumnName;
            target.OIFileIdMapped = importSettings.OIFileIdMapped;
            target.OIFileTypeColumnName = importSettings.OIFileTypeColumnName;
            target.SupportedByViewerColumn = importSettings.SupportedByViewerColumn;
            target.MoveDocumentsInAppendOverlayMode = importSettings.MoveDocumentsInAnyOverlayMode;

            // only set if the extracted file map links to extracted text location
            if (importSettings.DestinationConfiguration.ExtractedTextFieldContainsFilePath)
            {
                target.ExtractedTextEncoding = importSettings.ExtractedTextEncoding;
                target.LongTextColumnThatContainsPathToFullText = importSettings.DestinationConfiguration.LongTextColumnThatContainsPathToFullText;
            }

            target.LoadImportedFullTextFromServer = importSettings.LoadImportedFullTextFromServer;
            target.DestinationFolderArtifactID = GetDestinationFolderArtifactId(importSettings);
            target.SelectedIdentifierFieldName = GetSelectedIdentifierFieldName(importSettings);
        }

        private int GetDestinationFolderArtifactId(ImportSettings importSettings)
        {
            Workspace currentWorkspace = _importApi.Workspaces().FirstOrDefault(x => x.ArtifactID.Equals(importSettings.DestinationConfiguration.CaseArtifactId));

            if (currentWorkspace == null)
            {
                throw new IntegrationPointsException($"No workspace (id: {importSettings.DestinationConfiguration.CaseArtifactId}) found among available workspaces.");
            }

            int rv = importSettings.DestinationConfiguration.DestinationFolderArtifactId;
            if (rv == 0)
            {
                rv = importSettings.DestinationConfiguration.ArtifactTypeId == (int) global::Relativity.ArtifactType.Document ? currentWorkspace.RootFolderID : currentWorkspace.RootArtifactID;
            }
            return rv;
        }

        private string GetSelectedIdentifierFieldName(ImportSettings importSettings)
        {
            IEnumerable<Field> workspaceFields = _importApi.GetWorkspaceFields(importSettings.DestinationConfiguration.CaseArtifactId, importSettings.DestinationConfiguration.ArtifactTypeId);
            return workspaceFields.First(x => x.ArtifactID == importSettings.DestinationConfiguration.IdentityFieldId).Name;
        }
    }
}
