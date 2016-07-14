﻿using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.WinEDDS;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	public class VolumeInfoBuilderTests
	{
		private ExportFile _exportFile;
		private ExportSettings _exportSettings;
		private VolumeInfoBuilder _volumeInfoBuilder;

		[SetUp]
		public void SetUp()
		{
			_exportSettings = DefaultExportSettingsFactory.Create();
			_exportFile = new ExportFile(1);
			_volumeInfoBuilder = new VolumeInfoBuilder();
		}

		[Test]
		public void ItShouldRewriteSubdirectoryInfoSettings()
		{
			const int subdirectoryStartNumber = 10;
			const int subdirectoryMaxSize = 1000;
			const int subdirectoryDigitPadding = 20;
			const string subdirectoryImagePrefix = "image_prefix";
			const string subdirectoryTextPrefix = "text_prefix";
			const string subdirectoryNativePrefix = "native_prefix";

			_exportSettings.SubdirectoryStartNumber = subdirectoryStartNumber;
			_exportSettings.SubdirectoryMaxFiles = subdirectoryMaxSize;
			_exportSettings.SubdirectoryDigitPadding = subdirectoryDigitPadding;
			_exportSettings.SubdirectoryImagePrefix = subdirectoryImagePrefix;
			_exportSettings.SubdirectoryTextPrefix = subdirectoryTextPrefix;
			_exportSettings.SubdirectoryNativePrefix = subdirectoryNativePrefix;

			_volumeInfoBuilder.SetVolumeInfo(_exportSettings, _exportFile);

			Assert.AreEqual(subdirectoryStartNumber, _exportFile.VolumeInfo.SubdirectoryStartNumber);
			Assert.AreEqual(subdirectoryMaxSize, _exportFile.VolumeInfo.SubdirectoryMaxSize);
			Assert.AreEqual(subdirectoryImagePrefix, _exportFile.VolumeInfo.get_SubdirectoryImagePrefix(false));
			Assert.AreEqual(subdirectoryTextPrefix, _exportFile.VolumeInfo.get_SubdirectoryFullTextPrefix(false));
			Assert.AreEqual(subdirectoryNativePrefix, _exportFile.VolumeInfo.get_SubdirectoryNativePrefix(false));
		}

		[Test]
		public void ItShouldRewriteVolumeInfoSettings()
		{
			const int volumeMaxSize = 345;
			const int volumeStartNumber = 5;
			const string volumePrefix = "test_prefix";

			_exportSettings.VolumeMaxSize = volumeMaxSize;
			_exportSettings.VolumeStartNumber = volumeStartNumber;
			_exportSettings.VolumePrefix = volumePrefix;

			_volumeInfoBuilder.SetVolumeInfo(_exportSettings, _exportFile);

			Assert.AreEqual(volumeMaxSize, _exportFile.VolumeInfo.VolumeMaxSize);
			Assert.AreEqual(volumeStartNumber, _exportFile.VolumeInfo.VolumeStartNumber);
			Assert.AreEqual(volumePrefix, _exportFile.VolumeInfo.VolumePrefix);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void ItShouldRewriteCopyFilesFromRepository(bool copyFilesFromRepository)
		{
			_exportSettings.CopyFileFromRepository = copyFilesFromRepository;

			_volumeInfoBuilder.SetVolumeInfo(_exportSettings, _exportFile);

			Assert.AreEqual(copyFilesFromRepository, _exportFile.VolumeInfo.CopyFilesFromRepository);
		}
	}
}