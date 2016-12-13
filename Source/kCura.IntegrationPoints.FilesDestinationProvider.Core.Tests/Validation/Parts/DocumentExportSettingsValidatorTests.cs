using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
	[TestFixture]
	public class DocumentExportSettingsValidatorTests
	{
		[Test]
		public void ItShouldValidateImagesSettings()
		{
			// arrange
			var settings = new ExportSettings
			{
				ExportImages = true,
				SelectedImageDataFileFormat = ExportSettings.ImageDataFileFormat.IPRO,
				ProductionPrecedence = ExportSettings.ProductionPrecedenceType.Original,
				SubdirectoryImagePrefix = "prefix"
			};

			var validator = new DocumentExportSettingsValidator();

			// act
			var actual = validator.ValidateImages(settings);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.FirstOrDefault(), Is.Null);
		}

		[Test]
		public void ItShouldFailValidationForInvalidImagesSettings()
		{
			// arrange
			var settings = new ExportSettings
			{
				ExportImages = true
			};

			var validator = new DocumentExportSettingsValidator();

			// act
			var actual = validator.ValidateImages(settings);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_FORMAT)));
			Assert.IsTrue(actual.Messages.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_SUBDIR_PREFIX)));
		}
	}
}