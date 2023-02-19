using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class ProviderConfigurationValidatorTests
    {
        [Test]
        public void ItShouldPrevalidateModel()
        {
            // arrange
            var serializerMock = Substitute.For<ISerializer>();
            var settingsBuilderMock = Substitute.For<IExportSettingsBuilder>();
            var initProcessMock = Substitute.For<IExportInitProcessService>();
            var fileBuilderMock = Substitute.For<IExportFileBuilder>();

            var validatorMock = Substitute.For<ExportFileValidator>(serializerMock, settingsBuilderMock, initProcessMock, fileBuilderMock);
            validatorMock.Validate(Arg.Any<object>())
                .Returns(new ValidationResult());

            var validatorsFactoryMock = Substitute.For<IFileDestinationProviderValidatorsFactory>();
            validatorsFactoryMock.CreateExportFileValidator()
                .Returns(validatorMock);

            var exportSettingsBuilderMock = Substitute.For<IExportSettingsBuilder>();
            var validator = new FileDestinationProviderConfigurationValidator(serializerMock, validatorsFactoryMock, exportSettingsBuilderMock);

            // act
            var actual = validator.Prevalidate(new IntegrationPointProviderValidationModel());

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
        }
    }
}
