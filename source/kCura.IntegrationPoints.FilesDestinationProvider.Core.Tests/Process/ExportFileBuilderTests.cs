using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	public class ExportFileBuilderTests
	{
		private ExportFileBuilder _exportFileBuilder;
		private ExportSettings _exportSettings;

		[SetUp]
		public void SetUp()
		{
			_exportSettings = DefaultExportSettingsFactory.Create();
			_exportFileBuilder = new ExportFileBuilder(Substitute.For<IDelimitersBuilder>(), Substitute.For<IVolumeInfoBuilder>());
		}

		[Test]
		[TestCase(ExportSettings.ImageFileType.SinglePage, ExportFile.ImageType.SinglePage)]
		[TestCase(ExportSettings.ImageFileType.MultiPage, ExportFile.ImageType.MultiPageTiff)]
		[TestCase(ExportSettings.ImageFileType.Pdf, ExportFile.ImageType.Pdf)]
		[TestCase(null, null)]
		public void ItShouldSetCorrectImageTypeWhenCopyingFilesFromRepository(ExportSettings.ImageFileType givenSetting, ExportFile.ImageType expectedSetting)
		{
			_exportSettings.CopyFileFromRepository = true;
			_exportSettings.ImageType = givenSetting;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(expectedSetting, exportFile.TypeOfImage);
		}

		[Test]
		public void ItShouldSetImageTypeToSinglePageWhenNotCopyingFilesFromRepository()
		{
			_exportSettings.CopyFileFromRepository = false;
			_exportSettings.ImageType = ExportSettings.ImageFileType.MultiPage;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(ExportFile.ImageType.SinglePage, exportFile.TypeOfImage);
		}

		[Test]
		[TestCase(ExportSettings.DataFileFormat.CSV, "csv")]
		[TestCase(ExportSettings.DataFileFormat.Concordance, "dat")]
		[TestCase(ExportSettings.DataFileFormat.Custom, "txt")]
		[TestCase(ExportSettings.DataFileFormat.HTML, "html")]
		public void ItShouldSetFileExtensionBasedOnFileFormat(ExportSettings.DataFileFormat givenFileFormat, string expectedExtension)
		{
			_exportSettings.OutputDataFileFormat = givenFileFormat;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(expectedExtension, exportFile.LoadFileExtension);
		}

		[Test]
		[TestCase(ExportSettings.DataFileFormat.CSV, false)]
		[TestCase(ExportSettings.DataFileFormat.Concordance, false)]
		[TestCase(ExportSettings.DataFileFormat.Custom, false)]
		[TestCase(ExportSettings.DataFileFormat.HTML, true)]
		public void ItShouldSetIsHtmlPropertyBasedOnFileFormat(ExportSettings.DataFileFormat givenFileFormat, bool isHtml)
		{
			_exportSettings.OutputDataFileFormat = givenFileFormat;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(isHtml, exportFile.LoadFileIsHtml);
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownImageFileTypeWhenCopyingFromRepository()
		{
			_exportSettings.CopyFileFromRepository = true;

			var incorrectEnumValue = Enum.GetValues(typeof (ExportSettings.ImageFileType)).Cast<ExportSettings.ImageFileType>().Max() + 1;
			_exportSettings.ImageType = incorrectEnumValue;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.ImageFileType ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldNotThrowExceptionForImageFileBeingNullWhenCopyingFromRepository()
		{
			_exportSettings.CopyFileFromRepository = true;

			_exportSettings.ImageType = null;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings), Throws.Nothing);
		}

		[Test]
		[TestCase(ExportSettings.ImageDataFileFormat.Opticon, LoadFileType.FileFormat.Opticon)]
		[TestCase(ExportSettings.ImageDataFileFormat.IPRO, LoadFileType.FileFormat.IPRO)]
		[TestCase(ExportSettings.ImageDataFileFormat.IPRO_FullText, LoadFileType.FileFormat.IPRO_FullText)]
		[TestCase(null, null)]
		public void ItShouldSetCorrectImageDataFileFormatWhenCopyingFilesFromRepository(ExportSettings.ImageDataFileFormat givenSetting, LoadFileType.FileFormat expectedSetting)
		{
			_exportSettings.CopyFileFromRepository = true;
			_exportSettings.SelectedImageDataFileFormat = givenSetting;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(expectedSetting, exportFile.LogFileFormat);
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownImageDataFileFormatTypeWhenCopyingFromRepository()
		{
			_exportSettings.CopyFileFromRepository = true;

			var incorrectEnumValue = Enum.GetValues(typeof (ExportSettings.ImageDataFileFormat)).Cast<ExportSettings.ImageDataFileFormat>().Max() + 1;
			_exportSettings.SelectedImageDataFileFormat = incorrectEnumValue;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.ImageDataFileFormat ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldThrowExceptionForImageDataFileFormatBeingNullWhenCopyingFromRepository()
		{
			_exportSettings.CopyFileFromRepository = true;

			_exportSettings.SelectedImageDataFileFormat = null;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings), Throws.Nothing);
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownDataFileFormat()
		{
			var incorrectEnumValue = Enum.GetValues(typeof (ExportSettings.DataFileFormat)).Cast<ExportSettings.DataFileFormat>().Max() + 1;
			_exportSettings.OutputDataFileFormat = incorrectEnumValue;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.DataFileFormat ({incorrectEnumValue})"));
		}

		[Test]
		[TestCase(ExportSettings.FilePathType.Relative, ExportFile.ExportedFilePathType.Relative)]
		[TestCase(ExportSettings.FilePathType.Absolute, ExportFile.ExportedFilePathType.Absolute)]
		[TestCase(ExportSettings.FilePathType.UserPrefix, ExportFile.ExportedFilePathType.Prefix)]
		public void ItShouldSetCorrectFilePathWhenCopyingFilesFromRepository(ExportSettings.FilePathType givenSetting, ExportFile.ExportedFilePathType expectedSetting)
		{
			_exportSettings.FilePath = givenSetting;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(expectedSetting, exportFile.TypeOfExportedFilePath);
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownFilePath()
		{
			var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.FilePathType)).Cast<ExportSettings.FilePathType>().Max() + 1;
			_exportSettings.FilePath = incorrectEnumValue;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.FilePathType ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldRewriteDigitPaddings()
		{
			const int volumeDigitPadding = 15;
			const int subdirectoryDigitPadding = 23;

			_exportSettings.VolumeDigitPadding = volumeDigitPadding;
			_exportSettings.SubdirectoryDigitPadding = subdirectoryDigitPadding;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(volumeDigitPadding, exportFile.VolumeDigitPadding);
			Assert.AreEqual(subdirectoryDigitPadding, exportFile.SubdirectoryDigitPadding);
		}

		[Test]
		public void ItShouldThrowExceptionForNegativeVolumeDigitPadding()
		{
			_exportSettings.VolumeDigitPadding = -1;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Volume Digit Padding must be non-negative number"));
		}

		[Test]
		public void ItShouldThrowExceptionForNegativeSubdirectoryDigitPadding()
		{
			_exportSettings.SubdirectoryDigitPadding = -1;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Subdirectory Digit Padding must be non-negative number"));
		}

		[Test]
		[TestCase(1)]
		[TestCase(10)]
		[TestCase(9876)]
		public void ItShouldSetStartExportAtRecordDecreasedByOne(int startExportAtRecord)
		{
			_exportSettings.StartExportAtRecord = startExportAtRecord;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(startExportAtRecord - 1, exportFile.StartAtDocumentNumber);
		}

		public void ItShouldRewriteOtherSettings()
		{
			const int artifactTypeId = 10;
			const int exportedObjArtifactId = 20;
			const bool includeNativeFilesPath = true;
			const string exportFilesLocation = "folder_path";
			const bool overwriteFiles = true;
			const int workspaceId = 30;
			var dataFileEncoding = Encoding.UTF8;
			const string exportedObjName = "files_prefix";
			const bool exportImages = true;
			const bool exportMultipleChoiceFieldsAsNested = true;

			_exportSettings.ArtifactTypeId = artifactTypeId;
			_exportSettings.ExportedObjArtifactId = exportedObjArtifactId;
			_exportSettings.IncludeNativeFilesPath = includeNativeFilesPath;
			_exportSettings.ExportFilesLocation = exportFilesLocation;
			_exportSettings.OverwriteFiles = overwriteFiles;
			_exportSettings.WorkspaceId = workspaceId;
			_exportSettings.DataFileEncoding = dataFileEncoding;
			_exportSettings.ExportedObjName = exportedObjName;
			_exportSettings.ExportImages = exportImages;
			_exportSettings.ExportMultipleChoiceFieldsAsNested = exportMultipleChoiceFieldsAsNested;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(exportFile.ArtifactTypeID, artifactTypeId);
			Assert.AreEqual(exportFile.ArtifactID, exportedObjArtifactId);
			Assert.AreEqual(exportFile.ExportNative, includeNativeFilesPath);
			Assert.AreEqual(exportFile.FolderPath, exportFilesLocation);
			Assert.AreEqual(exportFile.Overwrite, overwriteFiles);
			Assert.AreEqual(exportFile.CaseInfo.ArtifactID, workspaceId);
			Assert.AreEqual(exportFile.LoadFileEncoding, dataFileEncoding);
			Assert.AreEqual(exportFile.LoadFilesPrefix, exportedObjName);
			Assert.AreEqual(exportFile.ExportImages, exportImages);
			Assert.AreEqual(exportFile.MulticodesAsNested, exportMultipleChoiceFieldsAsNested);
		}
	}
}