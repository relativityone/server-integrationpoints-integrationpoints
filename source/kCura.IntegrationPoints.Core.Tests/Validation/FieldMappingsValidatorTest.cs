using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class FieldMappingsValidatorTest
	{
		private IValidator _instance;

		[SetUp]
		public void Setup()
		{
			_instance = new FieldMappingsValidator(new JSONSerializer());
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]")]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]")]
		public void Validate_Valid_Field_Map(string fieldMap)
		{
			// Arrange

			// Act
			ValidationResult result = _instance.Validate(fieldMap);

			// Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[Test]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]", FieldMappingsValidator.ERROR_DESTINATION_FIELD_NOT_MAPPED)]
		[TestCase("[{\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]", FieldMappingsValidator.ERROR_SOURCE_FIELD_NOT_MAPPED)]
		[TestCase("[{\"fieldMapType\":\"Identifier\"}]", FieldMappingsValidator.ERROR_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED)]
		public void Validate_Not_All_Fields_Mapped(string fieldMap, string errorMessage)
		{
			// Arrange

			// Act
			ValidationResult result = _instance.Validate(fieldMap);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(errorMessage)));
		}
	}
}
