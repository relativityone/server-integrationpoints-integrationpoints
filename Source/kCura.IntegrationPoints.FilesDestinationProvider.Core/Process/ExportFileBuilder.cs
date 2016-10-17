using System;
using System.Collections.Generic;
using System.ComponentModel;
using Castle.Core.Internal;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Extensions;
using kCura.WinEDDS;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	internal class ExportFileBuilder : IExportFileBuilder
	{
		public const string ORIGINAL_PRODUCTION_PRECEDENCE_TEXT = "Original";
		public const string ORIGINAL_PRODUCTION_PRECEDENCE_VALUE_TEXT = "-1";
		private readonly IDelimitersBuilder _delimitersBuilder;
		private readonly IVolumeInfoBuilder _volumeInfoBuilder;

		public ExportFileBuilder(IDelimitersBuilder delimitersBuilder, IVolumeInfoBuilder volumeInfoBuilder)
		{
			_delimitersBuilder = delimitersBuilder;
			_volumeInfoBuilder = volumeInfoBuilder;
		}

		public ExportFile Create(ExportSettings exportSettings)
		{
			var exportFile = new ExportFile(exportSettings.ArtifactTypeId);

			ExportFileHelper.SetDefaultValues(exportFile);

			exportFile.TypeOfExport = ParseExportType(exportSettings.TypeOfExport);

			SetExportedObjectIdAndName(exportSettings, exportFile);

			SetStartDocumentNumber(exportSettings, exportFile);

			exportFile.FolderPath = exportSettings.ExportFilesLocation;
			exportFile.Overwrite = exportSettings.OverwriteFiles;

			_volumeInfoBuilder.SetVolumeInfo(exportSettings, exportFile);

			SetCaseInfo(exportSettings, exportFile);
			SetMetadataFileSettings(exportSettings, exportFile);
			SetDigitPaddings(exportSettings, exportFile);
			SetImagesSettings(exportSettings, exportFile);

			exportFile.TypeOfExportedFilePath = ParseFilePath(exportSettings.FilePath);
			exportFile.FilePrefix = exportSettings.UserPrefix;

			exportFile.AppendOriginalFileName = exportSettings.AppendOriginalFileName;
			exportFile.ExportNative = exportSettings.ExportNatives || exportSettings.IncludeNativeFilesPath;
			exportFile.MulticodesAsNested = exportSettings.ExportMultipleChoiceFieldsAsNested;
			exportFile.ExportFullTextAsFile = exportSettings.ExportFullTextAsFile;
			exportFile.TextFileEncoding = exportSettings.TextFileEncodingType;

			_delimitersBuilder.SetDelimiters(exportFile, exportSettings);
			SetImagePrecedence(exportSettings, exportFile);

			return exportFile;
		}

		private static void SetExportedObjectIdAndName(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.ExportNativesToFileNamedFrom = ParseNativesFilenameFromType(exportSettings.ExportNativesToFileNamedFrom);
			switch (exportSettings.TypeOfExport)
			{
				case ExportSettings.ExportType.SavedSearch:
					exportFile.ArtifactID = exportSettings.SavedSearchArtifactId;
					exportFile.LoadFilesPrefix = exportSettings.SavedSearchName;
					break;
				case ExportSettings.ExportType.Folder:
				case ExportSettings.ExportType.FolderAndSubfolders:
					exportFile.ArtifactID = exportSettings.FolderArtifactId;
					exportFile.ViewID = exportSettings.ViewId;
					exportFile.LoadFilesPrefix = exportSettings.ViewName;
					break;
				case ExportSettings.ExportType.ProductionSet:
					exportFile.ArtifactID = exportSettings.ProductionId;
					exportFile.LoadFilesPrefix = exportSettings.ProductionName;
					break;
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.ExportType ({exportSettings.TypeOfExport})");
			}
		}

		private void SetImagePrecedence(ExportSettings exportSettings, ExportFile exportFile)
		{
			var imagePrecs = new List<Pair>();
			if (exportSettings.ProductionPrecedence == ExportSettings.ProductionPrecedenceType.Produced)
			{
				foreach (var productionPrecedence in exportSettings.ImagePrecedence)
				{
					imagePrecs.Add(new Pair(productionPrecedence.ArtifactID, productionPrecedence.DisplayName));
				}
			}
			if ((exportSettings.ProductionPrecedence == ExportSettings.ProductionPrecedenceType.Original) || exportSettings.IncludeOriginalImages)
			{
				imagePrecs.Add(new Pair(ORIGINAL_PRODUCTION_PRECEDENCE_VALUE_TEXT, ORIGINAL_PRODUCTION_PRECEDENCE_TEXT));
			}

			exportFile.ImagePrecedence = imagePrecs.ToArray();
		}

		private static void SetStartDocumentNumber(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.StartAtDocumentNumber = exportSettings.StartExportAtRecord - 1;
		}

		private static void SetCaseInfo(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.CaseInfo = new CaseInfo {ArtifactID = exportSettings.WorkspaceId};
		}

		private static void SetMetadataFileSettings(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.LoadFileEncoding = exportSettings.DataFileEncoding;
			exportFile.LoadFileExtension = ParseDataFileFormat(exportSettings.OutputDataFileFormat);
			exportFile.LoadFileIsHtml = IsHtml(exportSettings.OutputDataFileFormat);
		}

		private void SetDigitPaddings(ExportSettings exportSettings, ExportFile exportFile)
		{
			if (exportSettings.SubdirectoryDigitPadding < 0)
			{
				throw new ArgumentException("Subdirectory Digit Padding must be non-negative number");
			}
			if (exportSettings.VolumeDigitPadding < 0)
			{
				throw new ArgumentException("Volume Digit Padding must be non-negative number");
			}
			exportFile.SubdirectoryDigitPadding = exportSettings.SubdirectoryDigitPadding;
			exportFile.VolumeDigitPadding = exportSettings.VolumeDigitPadding;
		}

		private static void SetImagesSettings(ExportSettings exportSettings, ExportFile exportFile)
		{
			if (exportSettings.ExportImages || (exportSettings.SelectedImageDataFileFormat != ExportSettings.ImageDataFileFormat.None))
			{
				exportFile.ExportImages = true;
			}
			exportFile.LogFileFormat = ParseImageImageDataFileFormat(exportSettings.SelectedImageDataFileFormat);
			SetTypeOfImage(exportSettings, exportFile);
		}

		private static void SetTypeOfImage(ExportSettings exportSettings, ExportFile exportFile)
		{
			if (exportSettings.ExportNatives)
			{
				exportFile.TypeOfImage = ParseImageFileType(exportSettings.ImageType);
			}
			else
			{
				exportFile.TypeOfImage = ExportFile.ImageType.SinglePage;
			}
		}

		private static ExportFile.ExportType ParseExportType(ExportSettings.ExportType exportType)
		{
			switch (exportType)
			{
				case ExportSettings.ExportType.Folder:
					return ExportFile.ExportType.ParentSearch;
				case ExportSettings.ExportType.FolderAndSubfolders:
					return ExportFile.ExportType.AncestorSearch;
				case ExportSettings.ExportType.ProductionSet:
					return ExportFile.ExportType.Production;
				case ExportSettings.ExportType.SavedSearch:
					return ExportFile.ExportType.ArtifactSearch;
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.ExportType ({exportType})");
			}
		}

		private static ExportFile.ImageType? ParseImageFileType(ExportSettings.ImageFileType? fileType)
		{
			if (!fileType.HasValue)
			{
				return null;
			}

			switch (fileType)
			{
				case ExportSettings.ImageFileType.SinglePage:
					return ExportFile.ImageType.SinglePage;
				case ExportSettings.ImageFileType.MultiPage:
					return ExportFile.ImageType.MultiPageTiff;
				case ExportSettings.ImageFileType.Pdf:
					return ExportFile.ImageType.Pdf;
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.ImageFileType ({fileType})");
			}
		}

		private static string ParseDataFileFormat(ExportSettings.DataFileFormat dataFileFormat)
		{
			switch (dataFileFormat)
			{
				case ExportSettings.DataFileFormat.CSV:
					return "csv";
				case ExportSettings.DataFileFormat.Concordance:
					return "dat";
				case ExportSettings.DataFileFormat.HTML:
					return "html";
				case ExportSettings.DataFileFormat.Custom:
					return "txt";
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.DataFileFormat ({dataFileFormat})");
			}
		}

		private static bool IsHtml(ExportSettings.DataFileFormat dataFileFormat)
		{
			return dataFileFormat == ExportSettings.DataFileFormat.HTML;
		}

		private static LoadFileType.FileFormat? ParseImageImageDataFileFormat(ExportSettings.ImageDataFileFormat? imageDataFileFormat)
		{
			if (!imageDataFileFormat.HasValue)
			{
				return null;
			}

			switch (imageDataFileFormat)
			{
				case ExportSettings.ImageDataFileFormat.None:
				case ExportSettings.ImageDataFileFormat.Opticon:
					return LoadFileType.FileFormat.Opticon;
				case ExportSettings.ImageDataFileFormat.IPRO:
					return LoadFileType.FileFormat.IPRO;
				case ExportSettings.ImageDataFileFormat.IPRO_FullText:
					return LoadFileType.FileFormat.IPRO_FullText;
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.ImageDataFileFormat ({imageDataFileFormat})");
			}
		}

		private static ExportFile.ExportedFilePathType ParseFilePath(ExportSettings.FilePathType filePath)
		{
			switch (filePath)
			{
				case ExportSettings.FilePathType.Relative:
					return ExportFile.ExportedFilePathType.Relative;
				case ExportSettings.FilePathType.Absolute:
					return ExportFile.ExportedFilePathType.Absolute;
				case ExportSettings.FilePathType.UserPrefix:
					return ExportFile.ExportedFilePathType.Prefix;
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.FilePathType ({filePath})");
			}
		}

		private static ExportNativeWithFilenameFrom ParseNativesFilenameFromType(ExportSettings.NativeFilenameFromType? exportSettingsExportNativesToFileNamedFrom)
		{
			if (!exportSettingsExportNativesToFileNamedFrom.HasValue)
			{
				// We can't return ExportNativeWithFilenameFrom.Select as this will couse issues in RDC Export code
				return ExportNativeWithFilenameFrom.Identifier;
			}
			switch (exportSettingsExportNativesToFileNamedFrom)
			{
				case ExportSettings.NativeFilenameFromType.Identifier:
					return ExportNativeWithFilenameFrom.Identifier;
				case ExportSettings.NativeFilenameFromType.Production:
					return ExportNativeWithFilenameFrom.Production;
				default:
					throw new InvalidEnumArgumentException($"Unknown ExportSettings.NativeFilenameFromType ({exportSettingsExportNativesToFileNamedFrom})");
			}
		}
	}
}