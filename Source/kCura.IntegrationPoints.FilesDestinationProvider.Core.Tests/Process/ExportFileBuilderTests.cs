using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
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
			_exportSettings.ExportNatives = true;
			_exportSettings.ImageType = givenSetting;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(expectedSetting, exportFile.TypeOfImage);
		}

		[Test]
		public void ItShouldSetImageTypeToSinglePageWhenNotCopyingFilesFromRepository()
		{
			_exportSettings.ExportNatives = false;
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
			_exportSettings.ExportNatives = true;

			var incorrectEnumValue = Enum.GetValues(typeof (ExportSettings.ImageFileType)).Cast<ExportSettings.ImageFileType>().Max() + 1;
			_exportSettings.ImageType = incorrectEnumValue;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.ImageFileType ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldNotThrowExceptionForImageFileBeingNullWhenCopyingFromRepository()
		{
			_exportSettings.ExportNatives = true;

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
			_exportSettings.ExportNatives = true;
			_exportSettings.SelectedImageDataFileFormat = givenSetting;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(expectedSetting, exportFile.LogFileFormat);
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownImageDataFileFormatTypeWhenCopyingFromRepository()
		{
			_exportSettings.ExportNatives = true;

			var incorrectEnumValue = Enum.GetValues(typeof (ExportSettings.ImageDataFileFormat)).Cast<ExportSettings.ImageDataFileFormat>().Max() + 1;
			_exportSettings.SelectedImageDataFileFormat = incorrectEnumValue;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.ImageDataFileFormat ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldThrowExceptionForImageDataFileFormatBeingNullWhenCopyingFromRepository()
		{
			_exportSettings.ExportNatives = true;

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

		[Test]
		public void ItShouldSetExportFullTextAsFile()
		{
			const bool exportFullTextAsFile = true;

			_exportSettings.ExportFullTextAsFile = exportFullTextAsFile;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.AreEqual(exportFile.ExportFullTextAsFile, exportFullTextAsFile);
		}

		[Test]
		[TestCase(ExportSettings.ProductionPrecedenceType.Original, false, true)]
		[TestCase(ExportSettings.ProductionPrecedenceType.Produced, true, true)]
		[TestCase(ExportSettings.ProductionPrecedenceType.Produced, false, false)]
		public void ItShouldSetOriginalProductionAccordingly(ExportSettings.ProductionPrecedenceType productionPrecedenceType, bool includeOriginalImage, bool outputShouldIncludeOrigImage)
		{
			_exportSettings.ProductionPrecedence = productionPrecedenceType;
			_exportSettings.IncludeOriginalImages = includeOriginalImage;
			_exportSettings.ImagePrecedence = new List<ProductionDTO>();

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			var imagePrecedenceList = exportFile.ImagePrecedence
				.Where(item => item.Display == ExportFileBuilder.ORIGINAL_PRODUCTION_PRECEDENCE_TEXT).ToList();

			if (outputShouldIncludeOrigImage)
			{
				Assert.That(imagePrecedenceList.Any());
				Assert.That(imagePrecedenceList.Count, Is.EqualTo(1));
				Assert.That(imagePrecedenceList.First().Value,
					Is.EqualTo(ExportFileBuilder.ORIGINAL_PRODUCTION_PRECEDENCE_VALUE_TEXT));
			}
			else
			{
				Assert.That(!imagePrecedenceList.Any());
			}
		}

		[Test]
		public void ItShouldSetSelectedProductionPrecedence()
		{
			var productionPrecedenceList = new List<ProductionDTO>()
			{
				new ProductionDTO
				{
					ArtifactID = "19",
					DisplayName = "Prod1"
				},
				new ProductionDTO
				{
					ArtifactID = "153",
					DisplayName = "Prod2"
				}
			};

			_exportSettings.ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Produced;
			_exportSettings.IncludeOriginalImages = false;
			_exportSettings.ImagePrecedence = productionPrecedenceList;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.That(productionPrecedenceList.Count, Is.EqualTo(exportFile.ImagePrecedence.Length));

			Assert.True(productionPrecedenceList.All(x => exportFile.ImagePrecedence.Any(y => y.Display == x.DisplayName && y.Value == x.ArtifactID)));
		}

		[Test]
		public void ItShouldSetExportNativesWhenIncludingNativeFilesPath()
		{
			_exportSettings.IncludeNativeFilesPath = true;
			_exportSettings.ExportNatives = false;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.True(exportFile.ExportNative);
		}

		[Test]
		public void ItShouldSetSavedSearchSettings()
		{
			_exportSettings.TypeOfExport = ExportSettings.ExportType.SavedSearch;
			_exportSettings.SavedSearchArtifactId = 834;
			_exportSettings.SavedSearchName = "saved_search_name_327";

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.That(exportFile.ArtifactID, Is.EqualTo(_exportSettings.SavedSearchArtifactId));
			Assert.That(exportFile.LoadFilesPrefix, Is.EqualTo(_exportSettings.SavedSearchName));
		}

		[Test]
		[TestCase(ExportSettings.ExportType.Folder)]
		[TestCase(ExportSettings.ExportType.FolderAndSubfolders)]
		public void ItShouldSetFolderSettings(ExportSettings.ExportType exportType)
		{
			_exportSettings.TypeOfExport = exportType;
			_exportSettings.FolderArtifactId = 972;
			_exportSettings.ViewId = 171;
			_exportSettings.ViewName = "view_name_803";

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.That(exportFile.ArtifactID, Is.EqualTo(_exportSettings.FolderArtifactId));
			Assert.That(exportFile.ViewID, Is.EqualTo(_exportSettings.ViewId));
			Assert.That(exportFile.LoadFilesPrefix, Is.EqualTo(_exportSettings.ViewName));
		}

		[Test]
		public void ItShouldSetProductionSettings()
		{
			_exportSettings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			_exportSettings.ProductionId = 763;
			_exportSettings.ProductionName = "production_name_965";
			_exportSettings.ExportNativesToFileNamedFrom = ExportSettings.NativeFilenameFromType.Identifier;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.That(exportFile.ArtifactID, Is.EqualTo(_exportSettings.ProductionId));
			Assert.That(exportFile.LoadFilesPrefix, Is.EqualTo(_exportSettings.ProductionName));
		}

		[Test]
		[TestCase(null, ExportNativeWithFilenameFrom.Identifier)]
		[TestCase(ExportSettings.NativeFilenameFromType.Identifier, ExportNativeWithFilenameFrom.Identifier)]
		[TestCase(ExportSettings.NativeFilenameFromType.Production, ExportNativeWithFilenameFrom.Production)]
		public void ItShouldSetNativeFilenameFromAccordingly(ExportSettings.NativeFilenameFromType? givenSetting, ExportNativeWithFilenameFrom expectedSetting)
		{
			_exportSettings.ExportNativesToFileNamedFrom = givenSetting;

			var exportFile = _exportFileBuilder.Create(_exportSettings);

			Assert.That(exportFile.ExportNativesToFileNamedFrom, Is.EqualTo(expectedSetting));
		}

		[Test]
		public void ItShouldThrowExpectionForUnknownExportNativeWithFilenameFrom()
		{
			_exportSettings.TypeOfExport = ExportSettings.ExportType.ProductionSet;
			_exportSettings.ExportNativesToFileNamedFrom = Enum.GetValues(typeof(ExportSettings.NativeFilenameFromType)).Cast<ExportSettings.NativeFilenameFromType>().Max() + 1;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.NativeFilenameFromType ({_exportSettings.ExportNativesToFileNamedFrom})"));
		}

		[Test]
		public void ItShouldThrowExpectionForUnknownExportType()
		{
			_exportSettings.TypeOfExport = Enum.GetValues(typeof(ExportSettings.ExportType)).Cast<ExportSettings.ExportType>().Max() + 1;

			Assert.That(() => _exportFileBuilder.Create(_exportSettings),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ExportSettings.ExportType ({_exportSettings.TypeOfExport})"));
		}
	}
}