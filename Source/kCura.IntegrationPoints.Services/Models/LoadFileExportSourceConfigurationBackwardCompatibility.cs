

using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Services.Models
{
	public class LoadFileExportSourceConfigurationBackwardCompatibility : ExportUsingSavedSearchSettings
	{
		public LoadFileExportSourceConfigurationBackwardCompatibility(
			LoadFileExportDestinationConfiguration destinationConfiguration,
			LoadFileExportSourceConfiguration sourceConfiguration)
		{
			SourceWorkspaceArtifactId = sourceConfiguration.SourceWorkspaceArtifactId;

			ExportType = destinationConfiguration.ExportType;
			SavedSearchArtifactId = sourceConfiguration.SavedSearchArtifactId;
			// SavedSearch = 
			FolderArtifactId  = sourceConfiguration.FolderArtifactId;
			ViewId  = sourceConfiguration.ViewId;
			// ViewName =
			ProductionId  = sourceConfiguration.ProductionId;
			// ProductionName  = destinationConfiguration. ;
			ExportNativesToFileNamedFrom  = destinationConfiguration.ExportNativesToFileNamedFrom;
			StartExportAtRecord  = destinationConfiguration.StartExportAtRecord;
			AppendOriginalFileName  = destinationConfiguration.AppendOriginalFileName;
			Fileshare  = destinationConfiguration.Fileshare;
			ExportNatives  = destinationConfiguration.ExportNatives;
			OverwriteFiles  = destinationConfiguration.OverwriteFiles;
			ExportImages  = destinationConfiguration.ExportImages;
			SelectedImageFileType  = destinationConfiguration.SelectedImageFileType;
			SelectedDataFileFormat  = destinationConfiguration.SelectedDataFileFormat;
			DataFileEncodingType  = destinationConfiguration.DataFileEncodingType;
			SelectedImageDataFileFormat  = destinationConfiguration.SelectedImageDataFileFormat;
			ColumnSeparator  = destinationConfiguration.ColumnSeparator;
			QuoteSeparator  = destinationConfiguration.QuoteSeparator;
			NewlineSeparator  = destinationConfiguration.NewlineSeparator;
			MultiValueSeparator  = destinationConfiguration.MultiValueSeparator;
			NestedValueSeparator  = destinationConfiguration.NestedValueSeparator;
			SubdirectoryImagePrefix  = destinationConfiguration.SubdirectoryImagePrefix;
			SubdirectoryNativePrefix  = destinationConfiguration.SubdirectoryNativePrefix;
			SubdirectoryTextPrefix  = destinationConfiguration.SubdirectoryTextPrefix;
			SubdirectoryStartNumber  = destinationConfiguration.SubdirectoryStartNumber;
			SubdirectoryDigitPadding  = destinationConfiguration.SubdirectoryDigitPadding;
			SubdirectoryMaxFiles  = destinationConfiguration.SubdirectoryMaxFiles;
			VolumePrefix  = destinationConfiguration.VolumePrefix;
			VolumeStartNumber  = destinationConfiguration.VolumeStartNumber;
			VolumeDigitPadding  = destinationConfiguration.VolumeDigitPadding;
			VolumeMaxSize  = destinationConfiguration.VolumeMaxSize;
			FilePath  = destinationConfiguration.FilePath;
			UserPrefix  = destinationConfiguration.UserPrefix;
			ExportMultipleChoiceFieldsAsNested  = destinationConfiguration.ExportMultipleChoiceFieldsAsNested;
			IncludeNativeFilesPath  = destinationConfiguration.IncludeNativeFilesPath;
			ExportFullTextAsFile  = destinationConfiguration.ExportFullTextAsFile;
			TextPrecedenceFields  = destinationConfiguration.TextPrecedenceFields;
			TextFileEncodingType  = destinationConfiguration.TextFileEncodingType;
			ProductionPrecedence  = destinationConfiguration.ProductionPrecedence;
			IncludeOriginalImages  = destinationConfiguration.IncludeOriginalImages;
			ImagePrecedence  = destinationConfiguration.ImagePrecedence;
		}
	}
}
