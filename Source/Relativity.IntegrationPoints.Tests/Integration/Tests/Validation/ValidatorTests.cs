using FluentAssertions;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Validation
{
	[IdentifiedTestFixture("0B863864-88CB-4E28-87B0-05E105E8247F")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class ValidatorTests : TestsBase
	{
		[IdentifiedTest("F4C94F16-EFD3-49B4-9C93-9ABCD3588E47")]
		public void ArtifactValidator_ShouldValidate()
		{
			// Arrange

			ArtifactValidator sut = PrepareSut<ArtifactValidator>();

			// Act
			ValidationResult result = sut.Validate(0);

			// Assert
			VerifyValidationPassed(result);
		}

		private T PrepareSut<T>() where T: IValidator
		{
			return Container.Resolve<T>();
		}

		private void VerifyValidationPassed(ValidationResult result)
		{
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}
	}
}
