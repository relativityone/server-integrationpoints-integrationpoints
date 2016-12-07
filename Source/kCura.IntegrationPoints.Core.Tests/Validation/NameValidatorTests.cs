using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class NameValidatorTests
	{
		[Test]
		public void ItShouldValidateProperName()
		{
			// arrange
			var name = "the name";

			var validator = new NameValidator();

			// act
			var actual = validator.Validate(name);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}

		[TestCase(null)]
		[TestCase("\t")]
		[TestCase("    ")]
		[TestCase("\r\n")]
		public void ItShouldFailValidationForInvalidName(string name)
		{
			// arrange
			var validator = new NameValidator();

			// act
			var actual = validator.Validate(name);

			// assert
			Assert.IsFalse(actual.IsValid);
			Assert.IsTrue(actual.Messages.Contains(IntegrationPointProviderValidationMessages.ERROR_INTEGRATION_POINT_NAME_EMPTY));
		}
	}
}