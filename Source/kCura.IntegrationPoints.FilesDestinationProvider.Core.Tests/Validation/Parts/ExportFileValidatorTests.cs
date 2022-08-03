using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
    public class ExportFileValidatorTests
    {
        [Test]
        public void ItShouldThrowForInvalidModel()
        {
            // arrange
            var serializer = Substitute.For<ISerializer>();
            var settingsBuilder = Substitute.For<IExportSettingsBuilder>();
            var initProcessService = Substitute.For<IExportInitProcessService>();
            var fileBuilder = Substitute.For<IExportFileBuilder>();

            var validator = new ExportFileValidator(serializer, settingsBuilder, initProcessService, fileBuilder);

            var model = new IntegrationPointProviderValidationModel();

            // act & assert
            Assert.Throws<NullReferenceException>(() => validator.Validate(model));
        }
    }
}