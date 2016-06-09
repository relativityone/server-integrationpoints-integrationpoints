﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	public class ExportSettingsBuilderTests
	{
		private ExportSettingsBuilder _exportSettingsBuilder;

		[SetUp]
		public void SetUp()
		{
			_exportSettingsBuilder = new ExportSettingsBuilder();
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownImageFileType()
		{
			var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.ImageFileType)).Cast<ExportSettings.ImageFileType>().Max() + 1;

			var sourceSettings = CreateSourceSettings();
			sourceSettings.SelectedImageFileType = ((int) incorrectEnumValue).ToString();
						
			Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ImageFileType ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldThrowExceptionForUnknownDataFileFormat()
		{
			var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.DataFileFormat)).Cast<ExportSettings.DataFileFormat>().Max() + 1;

			var sourceSettings = CreateSourceSettings();
			sourceSettings.SelectedDataFileFormat = ((int)incorrectEnumValue).ToString();

			Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown DataFileFormat ({incorrectEnumValue})"));
		}


		[Test]
		public void ItShouldThrowExceptionForUnknownImageDataFileFormat()
		{
			var incorrectEnumValue = Enum.GetValues(typeof(ExportSettings.ImageDataFileFormat)).Cast<ExportSettings.ImageDataFileFormat>().Max() + 1;

			var sourceSettings = CreateSourceSettings();
			sourceSettings.SelectedImageDataFileFormat = ((int)incorrectEnumValue).ToString();

			Assert.That(() => _exportSettingsBuilder.Create(sourceSettings, new List<FieldMap>(), 1),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown ImageDataFileFormat ({incorrectEnumValue})"));
		}

		[Test]
		public void ItShouldSelectFieldIdentifiers()
		{
			var fields = new List<FieldMap>()
			{
				new FieldMap() {SourceField = new FieldEntry() {FieldIdentifier = "1"}},
				new FieldMap() {SourceField = new FieldEntry() {FieldIdentifier = "2"}}
			};

			var exportSettings = _exportSettingsBuilder.Create(CreateSourceSettings(), fields, 1);

			CollectionAssert.AreEqual(exportSettings.SelViewFieldIds, new List<int> {1, 2});
		}
		private ExportUsingSavedSearchSettings CreateSourceSettings()
		{
			return new ExportUsingSavedSearchSettings()
			{
				SelectedImageFileType = ((int)default(ExportSettings.ImageFileType)).ToString(),
				SelectedDataFileFormat = ((int)default(ExportSettings.DataFileFormat)).ToString(),
				SelectedImageDataFileFormat = ((int)default(ExportSettings.ImageDataFileFormat)).ToString(),
				DataFileEncodingType = "Unicode"
			};
		}
	}
}
