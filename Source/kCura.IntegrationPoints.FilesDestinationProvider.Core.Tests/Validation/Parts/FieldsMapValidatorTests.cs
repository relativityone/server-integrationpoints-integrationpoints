using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation.Parts;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Validation.Parts
{
    [TestFixture, Category("Unit")]
    public class FieldsMapValidatorTests
    {
        private const string _EXPORTABLE_FIELDS = "[{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},{\"displayName\":\"Analytics Index\",\"isIdentifier\":true,\"fieldIdentifier\":\"1001009\",\"isRequired\":true}]";

        [TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Analytics Index\",\"isIdentifier\":false,\"fieldIdentifier\":\"1001009\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Analytics Index\",\"isIdentifier\":false,\"fieldIdentifier\":\"1001009\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", _EXPORTABLE_FIELDS)]
        public void ItShouldValidateFieldsMap(string fieldsMap, string exportableFields)
        {
            // arrange
            var serializer = new JSONSerializer();
            var exportableFieldsArray = serializer.Deserialize<IEnumerable<FieldEntry>>(exportableFields).ToArray();

            IExportFieldsService exportFieldServiceMock = Substitute.For<IExportFieldsService>();
            exportFieldServiceMock.GetAllExportableFields(Arg.Any<int>(), Arg.Any<int>())
                .Returns(exportableFieldsArray);
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new FieldsMapValidator(logger, serializer, exportFieldServiceMock);

            var model = new IntegrationPointProviderValidationModel
            {
                ArtifactTypeId = 42,
                SourceConfiguration = "{\"SourceWorkspaceArtifactId\":1000000}",
                FieldsMap = fieldsMap
            };

            // act
            var actual = validator.Validate(model);

            // assert
            Assert.IsTrue(actual.IsValid);
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(0));
        }

        [TestCase(null, _EXPORTABLE_FIELDS)]
        [TestCase("", _EXPORTABLE_FIELDS)]
        [TestCase("     ", _EXPORTABLE_FIELDS)]        
        public void ItShouldFailValidationForInvalidMappingObject(string fieldsMap, string exportableFields)
        {
            // arrange
            var serializer = new JSONSerializer();

            var exportableFieldsArray = serializer.Deserialize<IEnumerable<FieldEntry>>(exportableFields).ToArray();

            IExportFieldsService exportFieldServiceMock = Substitute.For<IExportFieldsService>();
            exportFieldServiceMock.GetAllExportableFields(Arg.Any<int>(), Arg.Any<int>())
                .Returns(exportableFieldsArray);
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new FieldsMapValidator(logger, serializer, exportFieldServiceMock);

            var model = new IntegrationPointProviderValidationModel
            {
                ArtifactTypeId = 42,
                SourceConfiguration = "{\"SourceWorkspaceArtifactId\":1000000}",
                FieldsMap = fieldsMap
            };

            // act
            var actual = validator.Validate(model);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.First().Contains(FileDestinationProviderValidationMessages.FIELD_MAP_NO_FIELDS));
        }

        [TestCase("[{\"sourceField\":{\"displayName\":\"\",\"isIdentifier\":false,\"fieldIdentifier\":\"\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"\",\"isIdentifier\":false,\"fieldIdentifier\":\"\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"\",\"isIdentifier\":false,\"fieldIdentifier\":\"\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"\",\"isIdentifier\":false,\"fieldIdentifier\":\"\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", _EXPORTABLE_FIELDS)]
        public void ItShouldFailValidationForUnknownFields(string fieldsMap, string exportableFields)
        {
            // arrange
            var serializer = new JSONSerializer();

            var exportableFieldsArray = serializer.Deserialize<IEnumerable<FieldEntry>>(exportableFields).ToArray();

            IExportFieldsService exportFieldServiceMock = Substitute.For<IExportFieldsService>();
            exportFieldServiceMock.GetAllExportableFields(Arg.Any<int>(), Arg.Any<int>())
                .Returns(exportableFieldsArray);
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new FieldsMapValidator(logger, serializer, exportFieldServiceMock);

            var model = new IntegrationPointProviderValidationModel
            {
                ArtifactTypeId = 42,
                SourceConfiguration = "{\"SourceWorkspaceArtifactId\":1000000}",
                FieldsMap = fieldsMap
            };

            // act
            var actual = validator.Validate(model);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.First().Contains(FileDestinationProviderValidationMessages.FIELD_MAP_UNKNOWN_FIELD));
        }

        [Test]
        public void ItShouldIncludeUnknownFieldNameInValidationMessage()
        {
            // arrange
            var fieldName = "Control Number";
            var fieldsMap = "[{\"sourceField\":{\"displayName\":\"" + fieldName + "\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";

            IExportFieldsService exportFieldServiceMock = Substitute.For<IExportFieldsService>();
            exportFieldServiceMock.GetAllExportableFields(Arg.Any<int>(), Arg.Any<int>())
                .Returns(Enumerable.Empty<FieldEntry>());

            var serializer = new JSONSerializer();
            IAPILog logger = Substitute.For<IAPILog>();
            var validator = new FieldsMapValidator(logger, serializer, exportFieldServiceMock);

            var model = new IntegrationPointProviderValidationModel
            {
                ArtifactTypeId = 42,
                SourceConfiguration = "{\"SourceWorkspaceArtifactId\":1000000}",
                FieldsMap = fieldsMap
            };

            // act
            var actual = validator.Validate(model);

            // assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsTrue(actual.MessageTexts.First().Contains(fieldName));
        }
    }
}