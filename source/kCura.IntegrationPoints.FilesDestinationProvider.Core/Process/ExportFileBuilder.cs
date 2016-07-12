using System;
using System.ComponentModel;
using kCura.WinEDDS;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	internal class ExportFileBuilder : IExportFileBuilder
	{
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

			exportFile.ArtifactID = exportSettings.ExportedObjArtifactId;

			SetStartDocumentNumber(exportSettings, exportFile);

			exportFile.ExportNative = exportSettings.IncludeNativeFilesPath;
			exportFile.FolderPath = exportSettings.ExportFilesLocation;
			exportFile.Overwrite = exportSettings.OverwriteFiles;

			_volumeInfoBuilder.SetVolumeInfo(exportSettings, exportFile);

			SetCaseInfo(exportSettings, exportFile);
			SetMetadataFileSettings(exportSettings, exportFile);
			SetDigitPaddings(exportSettings, exportFile);
			SetImagesSettings(exportSettings, exportFile);

			exportFile.TypeOfExportedFilePath = ParseFilePath(exportSettings.FilePath);
			exportFile.FilePrefix = exportSettings.UserPrefix;

			exportFile.MulticodesAsNested = exportSettings.ExportMultipleChoiceFieldsAsNested;

			_delimitersBuilder.SetDelimiters(exportFile, exportSettings);

			return exportFile;
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
			exportFile.LoadFilesPrefix = exportSettings.ExportedObjName;
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
			exportFile.ExportImages = exportSettings.ExportImages;
			exportFile.LogFileFormat = ParseImageImageDataFileFormat(exportSettings.SelectedImageDataFileFormat);
			SetTypeOfImage(exportSettings, exportFile);
		}

		private static void SetTypeOfImage(ExportSettings exportSettings, ExportFile exportFile)
		{
			if (exportSettings.CopyFileFromRepository)
			{
				exportFile.TypeOfImage = ParseImageFileType(exportSettings.ImageType);
			}
			else
			{
				exportFile.TypeOfImage = ExportFile.ImageType.SinglePage;
			}
		}

		private static ExportFile.ImageType? ParseImageFileType(ExportSettings.ImageFileType? fileType)
		{
			if (!fileType.HasValue)
				return null;

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
				return null;

			switch (imageDataFileFormat)
			{
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
	}
}