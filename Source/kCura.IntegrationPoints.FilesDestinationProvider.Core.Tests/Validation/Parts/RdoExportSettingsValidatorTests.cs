using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
	[TestFixture]
	public class RdoExportSettingsValidatorTests
	{
		[Test]
		public void ItShouldValidateExportLocation()
		{
			// arrange
			var settings = new ExportSettings
			{
				ExportFilesLocation = "location"
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateExportLocation(settings.ExportFilesLocation);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[TestCase(null)]
		[TestCase("     ")]
		public void ItShouldFailValidationForUnknownExportLocation(string location)
		{
			// arrange
			var settings = new ExportSettings
			{
				ExportFilesLocation = location
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateExportLocation(settings.ExportFilesLocation);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_UNKNOWN_LOCATION)));
		}

		[Test]
		public void ItShouldValidateLoadFileSettings()
		{
			// arrange
			var settings = new ExportSettings
			{
				OutputDataFileFormat = ExportSettings.DataFileFormat.Concordance,
				DataFileEncoding = Encoding.Unicode,
				FilePath = ExportSettings.FilePathType.Absolute
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateLoadFile(settings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[Test]
		public void ItShouldFailValidationForInvalidLoadFileSettings()
		{
			// arrange
			var settings = new ExportSettings();

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateLoadFile(settings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(FileDestinationProviderValidationMessages.SETTINGS_LOADFILE_UNKNOWN_ENCODING));
		}

		[Test]
		public void ItShouldValidateNativesSettings()
		{
			// arrange
			var settings = new ExportSettings
			{
				SubdirectoryNativePrefix = "prefix"
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateNatives(settings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[TestCase(null)]
		[TestCase("     ")]
		public void ItShouldFailValidationForInvalidNativesSettings(string prefix)
		{
			// arrange
			var settings = new ExportSettings
			{
				SubdirectoryNativePrefix = prefix
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateNatives(settings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.First().Contains(FileDestinationProviderValidationMessages.SETTINGS_NATIVES_UNKNOWN_SUBDIR_PREFIX));
		}

		[Test]
		public void ItShouldValidateTextFilesSettings()
		{
			// arrange
			var settings = new ExportSettings
			{
				ExportFullTextAsFile = true,
				TextFileEncodingType = Encoding.Unicode,
				TextPrecedenceFieldsIds = new List<int> { 1, 2, 3 },
				SubdirectoryTextPrefix = "prefix"
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateTextFieldsAsFiles(settings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[Test]
		public void ItShouldFailValidationForInvalidTextFilesSettings()
		{
			// arrange
			var settings = new ExportSettings
			{
				ExportFullTextAsFile = true
			};

			var validator = new RdoExportSettingsValidator();

			// act
			var actual = validator.ValidateTextFieldsAsFiles(settings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_ENCODING)));
			Assert.IsTrue(actual.Messages.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_PRECEDENCE)));
			Assert.IsTrue(actual.Messages.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_SUBDIR_PREFIX)));
		}
	}
}