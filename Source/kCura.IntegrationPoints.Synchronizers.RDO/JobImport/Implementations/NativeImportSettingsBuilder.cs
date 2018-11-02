using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport.Implementations
{
	public class NativeImportSettingsBuilder : ImportSettingsBaseBuilder<Settings>
	{
		public NativeImportSettingsBuilder(IExtendedImportAPI importApi)
			: base(importApi)
		{
		}

		public override void PopulateFrom(ImportSettings importSettings, Settings target)
		{
			base.PopulateFrom(importSettings, target);

			target.ArtifactTypeId = importSettings.ArtifactTypeId;
			target.BulkLoadFileFieldDelimiter = importSettings.BulkLoadFileFieldDelimiter;
			target.DisableControlNumberCompatibilityMode = importSettings.DisableControlNumberCompatibilityMode;
			target.DisableExtractedTextFileLocationValidation = importSettings.DisableExtractedTextFileLocationValidation;
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
			if (importSettings.ExtractedTextFieldContainsFilePath)
			{
				target.ExtractedTextEncoding = importSettings.ExtractedTextEncoding;
				target.LongTextColumnThatContainsPathToFullText = importSettings.LongTextColumnThatContainsPathToFullText;
			}

			target.LoadImportedFullTextFromServer = importSettings.LoadImportedFullTextFromServer;
			target.DestinationFolderArtifactID = GetDestinationFolderArtifactId(importSettings);
			target.SelectedIdentifierFieldName = GetSelectedIdentifierFieldName(importSettings);
		}

		private int GetDestinationFolderArtifactId(ImportSettings importSettings)
		{
			Workspace currentWorkspace = _importApi.Workspaces().First(x => x.ArtifactID.Equals(importSettings.CaseArtifactId));
			int rv = importSettings.DestinationFolderArtifactId;
			if (currentWorkspace != null && rv == 0)
			{
				rv = importSettings.ArtifactTypeId == (int) ArtifactType.Document ? currentWorkspace.RootFolderID : currentWorkspace.RootArtifactID;
			}
			return rv;
		}

		private string GetSelectedIdentifierFieldName(ImportSettings importSettings)
		{
			IEnumerable<Field> workspaceFields = _importApi.GetWorkspaceFields(importSettings.CaseArtifactId, importSettings.ArtifactTypeId);
			return workspaceFields.First(x => x.ArtifactID == importSettings.IdentityFieldId).Name;
		}
	}
}
