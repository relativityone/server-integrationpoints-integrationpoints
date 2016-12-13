using System.Linq;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class TransferredObjectValidatorTests
	{
		[Test]
		public void ItShouldValidateTransferredObjectValid()
		{
			// arrange
			var validator = new TransferredObjectValidator();

			// act
			ValidationResult result = validator.Validate((int) ArtifactType.Document);

			// assert
			Assert.IsTrue(result.IsValid);
			Assert.That(result.Messages.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ItShouldValidateTransferredObjectInvalid()
		{
			// arrange
			var validator = new TransferredObjectValidator();
			const int objectId = -1;

			// act
			ValidationResult result = validator.Validate(objectId);

			// assert
			Assert.IsFalse(result.IsValid);
			Assert.That(result.Messages.Any(x => x.Contains(RelativityProviderValidationMessages.TRANSFERRED_OBJECT_INVALIDA_TYPE)));
		}
	}
}
