using System.Collections.Generic;
using System.Linq;
using System.Text;
using kCura.IntegrationPoints.Core.Validation.Helpers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
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

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateExportLocation(settings.ExportFilesLocation);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
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

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateExportLocation(settings.ExportFilesLocation);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_UNKNOWN_LOCATION)));
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

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateLoadFile(settings);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
        }

        [Test]
        public void ItShouldFailValidationForInvalidLoadFileSettings()
        {
            // arrange
            var settings = new ExportSettings();

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateLoadFile(settings);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.First().Contains(FileDestinationProviderValidationMessages.SETTINGS_LOADFILE_UNKNOWN_ENCODING));
        }

        [Test]
        public void ItShouldValidateNativesSettings()
        {
            // arrange
            var settings = new ExportSettings
            {
                SubdirectoryNativePrefix = "prefix"
            };

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateNatives(settings);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
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

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateNatives(settings);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.First().Contains(FileDestinationProviderValidationMessages.SETTINGS_NATIVES_UNKNOWN_SUBDIR_PREFIX));
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

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateTextFieldsAsFiles(settings);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
        }

        [Test]
        public void ItShouldFailValidationForInvalidTextFilesSettings()
        {
            // arrange
            var settings = new ExportSettings
            {
                ExportFullTextAsFile = true
            };

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateTextFieldsAsFiles(settings);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_ENCODING)));
            Assert.IsTrue(actual.MessageTexts.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_PRECEDENCE)));
            Assert.IsTrue(actual.MessageTexts.Any(x => x.Contains(FileDestinationProviderValidationMessages.SETTINGS_TEXTFILES_UNKNOWN_SUBDIR_PREFIX)));
        }

        [Test]
        public void ItShouldValidateVolumePrefix()
        {
            // arrange
            var settings = new ExportSettings
            {
                VolumePrefix = "VOL"
            };

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            var actual = validator.ValidateVolumePrefix(settings);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.FirstOrDefault(), Is.Null);
        }

        [TestCase("VOL<")]
        [TestCase("\\1234")]
        public void ItShouldCallNonValidCharactersValidatorWithProperArguments(string volumePrefix)
        {
            // arrange
            var settings = new ExportSettings
            {
                VolumePrefix = volumePrefix
            };

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            validator.ValidateVolumePrefix(settings);

            // assert
            nonValidCharactersValidator.Received().Validate(volumePrefix,
                FileDestinationProviderValidationMessages.SETTINGS_VOLUME_PREFIX_ILLEGAL_CHARACTERS);
        }

        [TestCase(false, "EM1")]
        [TestCase(false, "Error message")]
        [TestCase(true, null)]
        [TestCase(false, null)]
        public void ItShouldReturErrorsReturnedByNonValidCharactersValidatorForNonEmptyVolumePrefix(bool isValid, string errorMessage)
        {
            // arrange
            var settings = new ExportSettings
            {
                VolumePrefix = "VolPrefix"
            };

            var nonValidCharactersValidator = Substitute.For<INonValidCharactersValidator>();
            ValidationResult validationResult = errorMessage != null
                ? new ValidationResult(isValid, errorMessage)
                : new ValidationResult(isValid);
            nonValidCharactersValidator.Validate(Arg.Any<string>(), Arg.Any<string>()).Returns(validationResult);
            var validator = new RdoExportSettingsValidator(nonValidCharactersValidator);

            // act
            ValidationResult actual = validator.ValidateVolumePrefix(settings);

            // assert
            Assert.AreEqual(isValid, actual.IsValid);
            if (errorMessage != null)
            {
                Assert.IsTrue(actual.MessageTexts.Contains(errorMessage));
            }
        }
    }
}