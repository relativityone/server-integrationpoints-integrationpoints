using System;
using kCura.EDDS.WebAPI.ExportManagerBase;
using Relativity.Core;
using Relativity.MassImport;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public static class ExportConvertingExtensions
	{
		public static InitializationResults ToInitializationResults(this Export.InitializationResults result)
		{
			return new InitializationResults
			{
				ColumnNames = result.ColumnNames,
				RowCount = result.RowCount,
				RunId = result.RunId
			};
		}

		public static ExportStatistics ToExportStatistics(this EDDS.WebAPI.AuditManagerBase.ExportStatistics stats)
		{
			return new ExportStatistics
			{
				ExportImages = stats.ExportImages,
				Fields = stats.Fields,
				ArtifactTypeID = stats.ArtifactTypeID,
				AppendOriginalFilenames = stats.AppendOriginalFilenames,
				Bound = stats.Bound,
				CopyFilesFromRepository = stats.CopyFilesFromRepository,
				DataSourceArtifactID = stats.DataSourceArtifactID,
				Delimiter = stats.Delimiter,
				DestinationFilesystemFolder = stats.DestinationFilesystemFolder,
				DocumentExportCount = stats.DocumentExportCount,
				ErrorCount = stats.ErrorCount,
				ExportMultipleChoiceFieldsAsNested = stats.ExportMultipleChoiceFieldsAsNested,
				ExportNativeFiles = stats.ExportNativeFiles,
				ExportTextFieldAsFiles = stats.ExportTextFieldAsFiles,
				ExportedTextFieldID = stats.ExportedTextFieldID,
				ExportedTextFileEncodingCodePage = stats.ExportedTextFileEncodingCodePage,
				FileExportCount = stats.FileExportCount,
				FilePathSettings = stats.FilePathSettings,
				ImageFileType = (ImageFileExportType) Enum.Parse(typeof(ImageFileExportType), stats.ImageFileType.ToString(), true),
				ImageLoadFileFormat = (ImageLoadFileFormatType) Enum.Parse(typeof(ImageLoadFileFormatType), stats.ImageLoadFileFormat.ToString(), true),
				ImagesToExport = (ImagesToExportType) Enum.Parse(typeof(ImagesToExportType), stats.ImagesToExport.ToString(), true),
				MetadataLoadFileEncodingCodePage = stats.MetadataLoadFileEncodingCodePage,
				MetadataLoadFileFormat = (LoadFileFormat) Enum.Parse(typeof(LoadFileFormat), stats.MetadataLoadFileFormat.ToString(), true),
				MultiValueDelimiter = stats.MultiValueDelimiter,
				NestedValueDelimiter = stats.NestedValueDelimiter,
				NewlineProxy = stats.NewlineProxy,
				OverwriteFiles = stats.OverwriteFiles,
				ProductionPrecedence = stats.ProductionPrecedence,
				RunTimeInMilliseconds = stats.RunTimeInMilliseconds,
				SourceRootFolderID = stats.SourceRootFolderID,
				StartExportAtDocumentNumber = stats.StartExportAtDocumentNumber,
				SubdirectoryImagePrefix = stats.SubdirectoryImagePrefix,
				SubdirectoryMaxFileCount = stats.SubdirectoryMaxFileCount,
				SubdirectoryNativePrefix = stats.SubdirectoryNativePrefix,
				SubdirectoryStartNumber = stats.SubdirectoryStartNumber,
				SubdirectoryTextPrefix = stats.SubdirectoryTextPrefix,
				TextAndNativeFilesNamedAfterFieldID = stats.TextAndNativeFilesNamedAfterFieldID,
				TotalFileBytesExported = stats.TotalFileBytesExported,
				TotalMetadataBytesExported = stats.TotalMetadataBytesExported,
				Type = stats.Type,
				VolumeMaxSize = stats.VolumeMaxSize,
				VolumePrefix = stats.VolumePrefix,
				VolumeStartNumber = stats.VolumeStartNumber,
				WarningCount = stats.WarningCount
			};
		}
	}
}