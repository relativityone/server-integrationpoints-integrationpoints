using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
    [TestFixture, Category("Unit")]
    public class FirstAndLastNameMappedValidatorTests
    {
        private FirstAndLastNameMappedValidator _sut;
        private Mock<IAPILog> _loggerFake;
        private const string _FIRST_NAME_FIELD_MAP = "{\"sourceField\":{\"displayName\":\""+EntityFieldNames.FirstName+"\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false}," +
                                                     "\"destinationField\":{\"displayName\":\""+EntityFieldNames.FirstName+"\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false}," +
                                                     "\"fieldMapType\":\"None\"}";
        private const string _FULLNAME_FIELD_MAP = "{\"sourceField\":{\"displayName\":\""+EntityFieldNames.FullName+"\",\"isIdentifier\":true,\"fieldIdentifier\":\"1035368\",\"isRequired\":false}," +
                                                    "\"destinationField\":{\"displayName\":\""+EntityFieldNames.FullName+"\",\"isIdentifier\":true,\"fieldIdentifier\":\"1035368\",\"isRequired\":false}," +
                                                    "\"fieldMapType\":\"Identifier\"}";
        private const string _LAST_NAME_FIELD_MAP = "{\"sourceField\":{\"displayName\":\""+EntityFieldNames.LastName+"\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false}," +
                                                              "\"destinationField\":{\"displayName\":\""+EntityFieldNames.LastName+"\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false}," +
                                                              "\"fieldMapType\":\"None\"}";

        [SetUp]
        public void SetUp()
        {
            _loggerFake = new Mock<IAPILog>();
            _loggerFake.Setup(x => x.ForContext<FirstAndLastNameMappedValidator>()).Returns(_loggerFake.Object);
            _sut = new FirstAndLastNameMappedValidator(new JSONSerializer(), _loggerFake.Object);
        }

        [Test]
        public void ItShouldValidateFieldMapWhenFirstAndLastAreNamePresent()
        {
            //Arrange
            string fieldMap = $"[{_FIRST_NAME_FIELD_MAP},{_LAST_NAME_FIELD_MAP}]";
            IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
            
            //Act
            ValidationResult validationResult = _sut.Validate(integrationPointProviderValidationModel);

            //Assert
            validationResult.IsValid.Should().BeTrue();
            validationResult.Messages.Should().BeEmpty();
        }
        
        [TestCase(_FIRST_NAME_FIELD_MAP)]
        [TestCase(_LAST_NAME_FIELD_MAP)]
        [TestCase(_FULLNAME_FIELD_MAP)]
        public void ItShouldFailValidationFieldMapWhenFirstOrLastNameIsMissing(string field)
        {
            //Arrange
            string fieldMap = $"[{field}]";
            IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
            
            //Act
            ValidationResult validationResult = _sut.Validate(integrationPointProviderValidationModel);

            //Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Messages.Should().NotBeEmpty();
        }

        private IntegrationPointProviderValidationModel GetFieldMapValidationObject(string fieldsMap)
        {
            return new IntegrationPointProviderValidationModel()
            {
                FieldsMap = fieldsMap,
                ObjectTypeGuid = ObjectTypeGuids.Entity
            };
        }
    }
}