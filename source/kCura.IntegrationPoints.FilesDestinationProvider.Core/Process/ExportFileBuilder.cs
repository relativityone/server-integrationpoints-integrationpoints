using System.ComponentModel;
using kCura.WinEDDS;
using Relativity;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	internal class ExportFileBuilder : IExportFileBuilder
	{
		private readonly IDelimitersBuilder _delimitersBuilder;

		public ExportFileBuilder(IDelimitersBuilder delimitersBuilder)
		{
			_delimitersBuilder = delimitersBuilder;
		}

		public ExportFile Create(ExportSettings exportSettings)
		{
			var exportFile = new ExportFile(exportSettings.ArtifactTypeId);

			ExportFileHelper.SetDefaultValues(exportFile);

			exportFile.ArtifactID = exportSettings.ExportedObjArtifactId;

			exportFile.ExportNative = exportSettings.IncludeNativeFilesPath;
			exportFile.FolderPath = exportSettings.ExportFilesLocation;
			exportFile.Overwrite = exportSettings.OverwriteFiles;
			exportFile.VolumeInfo.CopyFilesFromRepository = exportSettings.CopyFileFromRepository;

			SetCaseInfo(exportSettings, exportFile);
			SetMetadataFileSettings(exportSettings, exportFile);
			SetImagesSettings(exportSettings, exportFile);

			_delimitersBuilder.SetDelimiters(exportFile, exportSettings);

			return exportFile;
		}

		private static void SetCaseInfo(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.CaseInfo = new CaseInfo { ArtifactID = exportSettings.WorkspaceId };
		}

		private static void SetMetadataFileSettings(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.LoadFileEncoding = exportSettings.DataFileEncoding;
			exportFile.LoadFileExtension = ParseDataFileFormat(exportSettings.OutputDataFileFormat);
			exportFile.LoadFileIsHtml = IsHtml(exportSettings.OutputDataFileFormat);
			exportFile.LoadFilesPrefix = exportSettings.ExportedObjName;
		}

		private static void SetImagesSettings(ExportSettings exportSettings, ExportFile exportFile)
		{
			exportFile.ExportImages = exportSettings.ExportImages;
			exportFile.LogFileFormat = ParseImageImageDataFileFormat(exportSettings.SelectedImageDataFileFormat);
			if (exportSettings.CopyFileFromRepository)
			{
				exportFile.TypeOfImage = ParseImageFileType(exportSettings.ImageType);
			}
			else
			{
				exportFile.TypeOfImage = ExportFile.ImageType.SinglePage;
			}
		}

		private static ExportFile.ImageType ParseImageFileType(ExportSettings.ImageFileType fileType)
		{
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

		private static LoadFileType.FileFormat ParseImageImageDataFileFormat(ExportSettings.ImageDataFileFormat imageDataFileFormat)
		{
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
	}
}