using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class FieldMappingsValidatorTest
	{
		private IValidator _instance;
		private IFieldProvider _fieldProvider;

		[SetUp]
		public void Setup()
		{
			_fieldProvider = Substitute.For<IFieldProvider>();
			_instance = new FieldsMappingValidator(new JSONSerializer(), _fieldProvider);
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]")]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]")]
		public void Validate_Valid_Field_Map(string fieldMap)
		{
			// Arrange
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject(fieldMap);

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]", FieldsMappingValidator.ERROR_DESTINATION_FIELD_NOT_MAPPED)]
		[TestCase("[{\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]", FieldsMappingValidator.ERROR_SOURCE_FIELD_NOT_MAPPED)]
		[TestCase("[{\"fieldMapType\":\"Identifier\"}]", FieldsMappingValidator.ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED)]
		public void Validate_Not_All_Fields_Mapped(string fieldMap, string errorMessage)
		{
			// Arrange
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject(fieldMap);

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(errorMessage)));
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":false,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]")]
		public void Validate_Identifier_Not_Matched_Correctly(string fieldMap)
		{
			// Arrange
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject(fieldMap);

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(FieldsMappingValidator.ERROR_IDENTIFIERS_NOT_MATCHED));
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", new [] { "Control Number [Object Identifier]"})]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", new[] { "Control Number [Object Identifier]", "Email To" })]
		public void Validate_All_Required_Fields_Mapped(string fieldMap, string[] requiredFields)
		{
			// Arrange
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject(fieldMap);
			var requiredFieldsList = requiredFields.Select(field => new FieldEntry() {DisplayName = field, IsRequired = true}).ToList();
			_fieldProvider.GetFields(string.Empty).Returns(requiredFieldsList);

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", new[] { "requiredField1" })]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", new[] { "requiredField_1", "requiredField_2" })]
		public void Validate_Required_Fields_Not_Mapped(string fieldMap, string[] requiredFields)
		{
			// Arrange
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject(fieldMap);
			var requiredFieldsList = requiredFields.Select(field => new FieldEntry() { DisplayName = field, IsRequired = true }).ToList();
			_fieldProvider.GetFields(string.Empty).Returns(requiredFieldsList);

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(FieldsMappingValidator.ERROR_FIELD_MUST_BE_MAPPED)));
			foreach (string requiredField in requiredFields)
			{
				Assert.IsTrue(result.Messages.Any(x => x.Contains(requiredField)));
			}
		}

		private IntegrationModelValidation GetFieldMapValidationObject(string fieldsMap)
		{
			return new IntegrationModelValidation()
			{
				FieldsMap = fieldsMap,
				SourceProviderId = IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
				DestinationProviderId = Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
				DestinationConfiguration = string.Empty
			};
		}
	}
}
