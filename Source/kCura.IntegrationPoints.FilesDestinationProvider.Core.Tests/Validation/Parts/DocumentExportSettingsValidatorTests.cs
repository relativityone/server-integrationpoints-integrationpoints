using System.Linq;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
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

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new DocumentExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateImages(settings);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
        }

        [Test]
        public void ItShouldFailValidationForInvalidImagesSettings()
        {
            // arrange
            var settings = new ExportSettings
            {
                ExportImages = true
            };

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new DocumentExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateImages(settings);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_FORMAT)));
            Assert.IsTrue(actual.MessageTexts.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_IMAGES_UNKNOWN_SUBDIR_PREFIX)));
        }
    }
}
